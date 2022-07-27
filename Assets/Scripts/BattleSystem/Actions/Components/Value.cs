using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Info
// Information about an entity, used when applying payloads and calculating values. 
public class EntityInfo
{
    public string UID;
    public Entity Entity;
    public int Level;
    public Dictionary<string, float> Attributes;
    public Dictionary<string, float> AttributeOverride;

    public EntityInfo(Entity entity, Dictionary<string, float> entityAttributes)
    {
        if (entity == null)
        {
            Debug.Log($"Trying to create Entity Info for a null entity.");
        }

        UID = entity.UID;
        Entity = entity;
        Level = entity.Level;
        Attributes = entityAttributes;
        AttributeOverride = null;
    }

    public EntityInfo(Entity entity) : this(entity, entityAttributes: entity.EntityAttributes())
    {

    }

    public EntityInfo(Entity entity, PayloadData payloadData) : this(entity, entityAttributes: entity.EntityAttributes(payloadData))
    {

    }

    public float Attribute(string attribute)
    {
        if (AttributeOverride != null && AttributeOverride.ContainsKey(attribute))
        {
            return AttributeOverride[attribute];
        }
        else if (Attributes != null && Attributes.ContainsKey(attribute))
        {
            return Attributes[attribute];
        }
        else
        {
            return 0.0f;
        }
    }
}

// Information passed to a value when calculating it
public class ValueInfo
{
    public EntityInfo Caster;
    public EntityInfo Target;

    public Dictionary<string, ActionResult> ActionResults;
    public string ActionID;

    public ValueInfo(EntityInfo casterInfo, EntityInfo targetInfo, Dictionary<string, ActionResult> actionResults, string actionID = null)
    {
        Caster = casterInfo;
        Target = targetInfo;
        ActionResults = actionResults;
        ActionID = actionID;
    }

    public ValueInfo(Payload payloadInfo)
    {
        Caster = payloadInfo.Caster;
        Target = payloadInfo.Target;
        ActionResults = payloadInfo.ActionResults;
        ActionID = payloadInfo.Action?.ActionID;
    }

    public EntityInfo GetEntityInfo(ValueComponent.eEntity entityType)
    {
        switch(entityType)
        {
            case ValueComponent.eEntity.Caster:
            {
                return Caster;
            }
            case ValueComponent.eEntity.Target:
            {
                return Target;
            }
            default:
            {
                Debug.LogError($"The value info class does not contain entity info for {entityType}");
                return null;
            }
        }
    }
}
#endregion

#region Component
// A value is a sum of its components.
public class ValueComponent
{
    public enum eValueComponentType
    {
        FlatValue,
        EntityLevel,
        EntityAttributeBase,
        EntityAttributeCurrent,
        EntityResourceMax,
        EntityResourceCurrent,
        EntityResourceRatio,
        EntityStatusEffectStacksHighest,
        EntityStatusEffectStacksTotal,
        DistanceFromTarget,
        RandomValue,
        SavedValue,
        ActionResultValue,
        CasterSkillChargeRatio,
    }

    public enum eEntity
    {
        Caster,
        Target,
    }

    public eValueComponentType ComponentType;
    public eEntity Entity;

    public float Potency;                           // Multiplier
    public string StringValue;                      // The value can scale off different attributes, resources or action results (for example damage dealt or cost collected).
    public string StringValue2;                     // In case more string values are needed, for example to specify the kind of action result.

    public ValueComponent()
    {
        Potency = 1.0f;
        StringValue = "";
        StringValue2 = "";
    }

    public ValueComponent(eValueComponentType type, float potency, string stringValue = "")
    {
        ComponentType = type;
        Potency = potency;
        StringValue = stringValue;
        StringValue2 = "";
    }

    public ValueComponent Copy => new ValueComponent(ComponentType, Potency, StringValue);

    public float GetValue(ValueInfo valueInfo)
    {
        // Simple flat value
        if (ComponentType == eValueComponentType.FlatValue)
        {
            return Potency;
        }

        // Other component types
        if (valueInfo == null)
        {
            Debug.LogError($"Value component is not a flat value, but value info was not provided");
            return 0.0f;
        }

        // Result of an action
        if (ComponentType == eValueComponentType.ActionResultValue)
        {
            var actionID = StringValue;
            if (string.IsNullOrEmpty(actionID))
            {
                actionID = valueInfo.ActionID;
            }
            if (string.IsNullOrEmpty(actionID))
            {
                return 0.0f;
            }

            if (valueInfo.ActionResults != null && valueInfo.ActionResults.ContainsKey(actionID) && valueInfo.ActionResults[actionID].Values.ContainsKey(StringValue2))
            {
                return Potency * valueInfo.ActionResults[actionID].Values[StringValue2];
            }
            else
            {
                return 0.0f;
            }
        }

        // Entity related values
        if (ComponentType == eValueComponentType.EntityAttributeBase                || ComponentType == eValueComponentType.EntityAttributeCurrent          ||
            ComponentType == eValueComponentType.EntityLevel                        || ComponentType == eValueComponentType.EntityResourceCurrent           ||
            ComponentType == eValueComponentType.EntityResourceMax                  || ComponentType == eValueComponentType.EntityResourceRatio             ||
            ComponentType == eValueComponentType.EntityStatusEffectStacksHighest    || ComponentType == eValueComponentType.EntityStatusEffectStacksTotal)
        {
            var entityInfo = valueInfo.GetEntityInfo(Entity);

            if (entityInfo == null)
            {
                Debug.LogError($"Value component is an entity value, but corresponding entity info was not provided");
                return 0.0f;
            }

            switch(ComponentType)
            {
                case eValueComponentType.EntityLevel:
                {
                    return Potency * entityInfo.Level;
                }
                case eValueComponentType.EntityAttributeCurrent:
                {
                    if (entityInfo.Attributes == null || !entityInfo.Attributes.ContainsKey(StringValue))
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Attributes[StringValue];
                }
                case eValueComponentType.EntityAttributeBase:
                {
                    if (entityInfo.Entity == null || !entityInfo.Entity.BaseAttributes.ContainsKey(StringValue))
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Entity.BaseAttribute(StringValue);
                }
                case eValueComponentType.EntityResourceMax:
                {
                    if (entityInfo.Entity == null || !entityInfo.Entity.ResourcesMax.ContainsKey(StringValue))
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Entity.ResourcesMax[StringValue];
                }
                case eValueComponentType.EntityResourceCurrent:
                {
                    if (entityInfo.Entity == null || !entityInfo.Entity.ResourcesCurrent.ContainsKey(StringValue))
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Entity.ResourcesCurrent[StringValue];
                }
                case eValueComponentType.EntityResourceRatio:
                {
                    if (entityInfo.Entity == null || !entityInfo.Entity.ResourcesCurrent.ContainsKey(StringValue))
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Entity.ResourceRatio(StringValue);
                }
                case eValueComponentType.EntityStatusEffectStacksHighest:
                {
                    if (entityInfo.Entity == null)
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Entity.GetHighestStatusEffectStacks(StringValue);
                }
                case eValueComponentType.EntityStatusEffectStacksTotal:
                {
                    if (entityInfo.Entity == null)
                    {
                        return 0.0f;
                    }
                    return Potency * entityInfo.Entity.GetTotalStatusEffectStacks(StringValue);
                }
            }
        }

        // Charge ratio
        if (ComponentType == eValueComponentType.CasterSkillChargeRatio)
        {
            var entityBattle = valueInfo?.Caster?.Entity?.EntityBattle;
            if (entityBattle == null)
            {
                return 0.0f;
            }
            return Potency * entityBattle.SkillChargeRatio;
        }

        //Distance
        if (ComponentType == eValueComponentType.DistanceFromTarget)
        {
            if (valueInfo.Caster?.Entity == null || valueInfo.Target?.Entity == null)
            {
                Debug.LogError($"Trying to check the distance between caster and target, but one of them is null");
                return 0.0f;
            }
            return Potency * Vector3.Distance(valueInfo.Caster.Entity.Origin, valueInfo.Target.Entity.Origin);
        }

        //Saved Values
        if (ComponentType == eValueComponentType.SavedValue)
        {
            if (valueInfo.Caster?.Entity != null)
            {
                return valueInfo.Caster.Entity.GetSavedValue(StringValue, 0.0f);
            }
            else
            {
                return 0.0f;
            }
        }

        // Random
        if (ComponentType == eValueComponentType.RandomValue)
        {
            return UnityEngine.Random.Range(0.0f, Potency);
        }

        // Invalid/unimplemented component. A new case needs to be added if the code reaches this point. 
        Debug.LogError($"Unimplemented value component type: {ComponentType}");
        return 0.0f;
    }

    #region Context
    public enum eValueContext
    {
        SkillAction,        // Value used in a skill action.
        ResourceSetup,      // Value used when calculating entity resources
        Entity,             // Values that require information about an entity.
        NonAction,          // Individual payloads and other values not directly tied to actions and skills.
        TargetingPriority,  // Some values can be used to determine target priority.
    }

    public static Dictionary<eValueContext, List<eValueComponentType>> AvailableComponentTypes = new Dictionary<eValueContext, List<eValueComponentType>>()
    {
        [eValueContext.SkillAction] = new List<eValueComponentType>()
        {
            eValueComponentType.FlatValue,
            eValueComponentType.EntityLevel,
            eValueComponentType.EntityAttributeBase,
            eValueComponentType.EntityAttributeCurrent,
            eValueComponentType.EntityResourceMax,
            eValueComponentType.EntityResourceCurrent,
            eValueComponentType.EntityResourceRatio,
            eValueComponentType.EntityStatusEffectStacksHighest,
            eValueComponentType.EntityStatusEffectStacksTotal,
            eValueComponentType.DistanceFromTarget,
            eValueComponentType.SavedValue,
            eValueComponentType.RandomValue,
            eValueComponentType.ActionResultValue,
            eValueComponentType.CasterSkillChargeRatio
        },
        [eValueContext.ResourceSetup] = new List<eValueComponentType>()
        {
            eValueComponentType.FlatValue,
            eValueComponentType.EntityLevel,
            eValueComponentType.EntityAttributeBase,
            eValueComponentType.EntityAttributeCurrent,
            eValueComponentType.RandomValue,
        },
        [eValueContext.Entity] = new List<eValueComponentType>()
        {
            eValueComponentType.EntityLevel,
            eValueComponentType.EntityAttributeBase,
            eValueComponentType.EntityAttributeCurrent,
            eValueComponentType.EntityResourceMax,
            eValueComponentType.EntityResourceCurrent,
            eValueComponentType.EntityResourceRatio,
            eValueComponentType.EntityStatusEffectStacksHighest,
            eValueComponentType.EntityStatusEffectStacksTotal,
        },
        [eValueContext.NonAction] = new List<eValueComponentType>()
        {
            eValueComponentType.FlatValue,
            eValueComponentType.EntityLevel,
            eValueComponentType.EntityAttributeBase,
            eValueComponentType.EntityAttributeCurrent,
            eValueComponentType.EntityResourceMax,
            eValueComponentType.EntityResourceCurrent,
            eValueComponentType.EntityResourceRatio,
            eValueComponentType.EntityStatusEffectStacksHighest,
            eValueComponentType.EntityStatusEffectStacksTotal,
            eValueComponentType.DistanceFromTarget,
            eValueComponentType.SavedValue,
            eValueComponentType.RandomValue,
        }
    };
    #endregion
}
#endregion

public class Value
{
    // When a value is calculated, its components are all added together.
    // After that additional values can be calculated and applied in chosen operations. Nesting is possible.
    public class ValueOperation
    {
        public enum eOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide, 
            LimitUpper, // Value will be set to this operation value if it exceeds it.
            LimitLower  // Value will be set to this operation value if it's lower than it.
        }

        public eOperation Operation;
        public Value Value;

        public ValueOperation()
        {
            Value = new Value(false);
        }

        public ValueOperation(eOperation operation, Value value)
        {
            Operation = operation;
            Value = value;
        }
    }

    public List<ValueComponent> Components;
    public List<ValueOperation> Operations;

    public Value()
    {
        Components = new List<ValueComponent>();
        Operations = new List<ValueOperation>();
    }

    public Value(bool addComponent)
    {
        Components = new List<ValueComponent>();
        Operations = new List<ValueOperation>();
        if (addComponent)
        {
            Components.Add(new ValueComponent());
        }
    }

    public Value OutgoingValue(ValueInfo valueInfo)
    {
        var value = new Value(false);
        var totalValue = 0.0f;

        foreach (var component in Components)
        {
            if (component.Entity == ValueComponent.eEntity.Target &&
                (component.ComponentType == ValueComponent.eValueComponentType.EntityResourceCurrent ||
                 component.ComponentType == ValueComponent.eValueComponentType.EntityResourceMax))
            {
                value.Components.Add(component);
            }
            else
            {
                totalValue += component.GetValue(valueInfo);
            }
        }

        if (totalValue > Constants.Epsilon || totalValue < -Constants.Epsilon)
        {
            value.Components.Add(new ValueComponent(ValueComponent.eValueComponentType.FlatValue, totalValue));
        }

        foreach (var operation in Operations)
        {
            value.Operations.Add(new ValueOperation(operation.Operation, operation.Value.OutgoingValue(valueInfo)));
        }

        return value;
    }

    public float CalculateValue(ValueInfo valueInfo)
    {
        var totalValue = 0.0f;

        // Caclulate all components
        foreach (var component in Components)
        {
            totalValue += component.GetValue(valueInfo);
        }

        // Apply any operations
        foreach (var operation in Operations)
        {
            var operationValue = operation.Value.CalculateValue(valueInfo);

            switch (operation.Operation)
            {
                case ValueOperation.eOperation.Add:
                {
                    totalValue += operationValue;
                    break;
                }
                case ValueOperation.eOperation.Subtract:
                {
                    totalValue -= operationValue;
                    break;
                }
                case ValueOperation.eOperation.Multiply:
                {
                    totalValue *= operationValue;
                    break;
                }
                case ValueOperation.eOperation.Divide:
                {
                    if (operationValue != 0.0f)
                    {
                        totalValue /= operationValue;
                    }
                    break;
                }
                case ValueOperation.eOperation.LimitUpper:
                {
                    if (operationValue < totalValue)
                    {
                        totalValue = operationValue;
                    }
                    break;
                }
                case ValueOperation.eOperation.LimitLower:
                {
                    if (operationValue > totalValue)
                    {
                        totalValue = operationValue;
                    }
                    break;
                }
            }
        }

        return totalValue;
    }
}
