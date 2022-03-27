using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueComponent
{
    public enum eValueComponentType
    {
        FlatValue,
        CasterLevel,
        CasterAttributeBase,
        CasterAttributeCurrent,
        CasterResourceMax,
        CasterResourceCurrent,
        CasterResourceRatio,
        TargetLevel,
        TargetAttributeBase,
        TargetAttributeCurrent,
        TargetResourceMax,
        TargetResourceCurrent,
        TargetResourceRatio,
        ActionResultValue,
    }
    public eValueComponentType ComponentType;

    public float Potency;                           // Multiplier
    public string Attribute;                        // The value can scale off different attributes, resources or action results (for example damage dealt or cost collected).

    public ValueComponent()
    {
        Potency = 1.0f;
    }

    public ValueComponent(eValueComponentType type, float potency, string attribute = "")
    {
        ComponentType = type;
        Potency = potency;
        Attribute = attribute;
    }

    public ValueComponent Copy => new ValueComponent(ComponentType, Potency, Attribute);

    public float GetValueFromAttribute(Entity entity, Dictionary<string, float> entityAttributes)
    {
        switch (ComponentType)
        {
            case eValueComponentType.CasterAttributeCurrent:
            {
                if (entityAttributes == null || !entityAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}].");
                    return 0.0f;
                }
                return Potency * entityAttributes[Attribute];
            }
            case eValueComponentType.CasterAttributeBase:
            {
                if (entity == null || !entity.BaseAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}].");
                    return 0.0f;
                }
                return Potency * entity.BaseAttribute(Attribute);
            }
            case eValueComponentType.FlatValue:
            {
                return Potency;
            }
            default:
            {
                Debug.LogError($"This GetValue function doesn't support the {ComponentType} component type.");
                return 0.0f;
            }
        }
    }

    public float GetValue(Entity caster, Entity target, Dictionary<string, float> casterAttributes, 
                          Dictionary<string, float> targetAttributes, Dictionary<string, ActionResult> actionResults = null)
    {
        switch (ComponentType)
        {
            case eValueComponentType.FlatValue:
            {
                return Potency;
            }
            case eValueComponentType.CasterLevel:
            {
                if (caster == null)
                {
                    return 0.0f;
                }
                return Potency * caster.Level;
            }
            case eValueComponentType.CasterAttributeCurrent:
            {
                if (casterAttributes == null || !casterAttributes.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * casterAttributes[Attribute];
            }
            case eValueComponentType.CasterAttributeBase:
            {
                if (caster == null || !caster.BaseAttributes.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * caster.BaseAttribute(Attribute);
            }
            case eValueComponentType.CasterResourceMax:
            {
                if (caster == null || !caster.ResourcesMax.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * caster.ResourcesMax[Attribute];
            }
            case eValueComponentType.CasterResourceCurrent:
            {
                if (caster == null ||!caster.ResourcesCurrent.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * caster.ResourcesCurrent[Attribute];
            }
            case eValueComponentType.CasterResourceRatio:
            {
                if (caster == null || !caster.ResourcesCurrent.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * caster.ResourceRatio(Attribute);
            }
            case eValueComponentType.TargetLevel:
            {
                if (target == null)
                {
                    return 0.0f;
                }
                return Potency * target.Level;
            }
            case eValueComponentType.TargetAttributeCurrent:
            {
                if (targetAttributes == null || !targetAttributes.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * targetAttributes[Attribute];
            }
            case eValueComponentType.TargetAttributeBase:
            {
                if (target == null || !target.BaseAttributes.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * target.BaseAttribute(Attribute);
            }
            case eValueComponentType.TargetResourceMax:
            {
                if (target == null || !target.ResourcesMax.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * target.ResourcesMax[Attribute];
            }
            case eValueComponentType.TargetResourceCurrent:
            {
                if (target == null || !target.ResourcesCurrent.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * caster.ResourcesCurrent[Attribute];
            }
            case eValueComponentType.TargetResourceRatio:
            {
                if (caster == null || !target.ResourcesCurrent.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * target.ResourceRatio(Attribute);
            }
            case eValueComponentType.ActionResultValue:
            {
                if (actionResults != null && actionResults.ContainsKey(Attribute))
                {
                    return Potency * actionResults[Attribute].Value;
                }
                else
                {
                    Debug.LogError($"Action result for action {Attribute} could not be found.");
                    return 0.0f;
                }
            }
            default:
            {
                Debug.LogError($"Unimplemented value component type: {ComponentType}");
                return 0.0f;
            }
        }
    }

    public enum eValueType
    {
        SkillAction,
        Resource,
        CasterOnly,
        NonAction,
        TargetingPriority,
        Aggro,
    }

    public static List<eValueComponentType> AvailableComponentTypes(eValueType valueType)
    {
        switch (valueType)
        {
            case eValueType.SkillAction:
            {
                return new List<eValueComponentType>()
                {
                    eValueComponentType.FlatValue,
                    eValueComponentType.CasterLevel,
                    eValueComponentType.CasterAttributeBase,
                    eValueComponentType.CasterAttributeCurrent,
                    eValueComponentType.CasterResourceMax,
                    eValueComponentType.CasterResourceCurrent,
                    eValueComponentType.CasterResourceRatio,
                    eValueComponentType.TargetLevel,
                    eValueComponentType.TargetResourceMax,
                    eValueComponentType.TargetResourceCurrent,
                    eValueComponentType.TargetResourceRatio,
                    eValueComponentType.ActionResultValue
                };
            }
            case eValueType.Resource:
            {
                return new List<eValueComponentType>()
                {
                    eValueComponentType.FlatValue,
                    eValueComponentType.CasterLevel,
                    eValueComponentType.CasterAttributeBase,
                    eValueComponentType.CasterAttributeCurrent,
                };
            }
            case eValueType.CasterOnly:
            {
                return new List<eValueComponentType>()
                {
                    eValueComponentType.FlatValue,
                    eValueComponentType.CasterLevel,
                    eValueComponentType.CasterAttributeBase,
                    eValueComponentType.CasterAttributeCurrent,
                    eValueComponentType.CasterResourceMax,
                    eValueComponentType.CasterResourceCurrent,
                    eValueComponentType.CasterResourceRatio
                };
            }
            case eValueType.NonAction:
            {
                return new List<eValueComponentType>()
                {
                    eValueComponentType.FlatValue,
                    eValueComponentType.CasterLevel,
                    eValueComponentType.CasterAttributeBase,
                    eValueComponentType.CasterAttributeCurrent,
                    eValueComponentType.CasterResourceMax,
                    eValueComponentType.CasterResourceCurrent,
                    eValueComponentType.CasterResourceRatio,
                    eValueComponentType.TargetLevel,
                    eValueComponentType.TargetResourceMax,
                    eValueComponentType.TargetResourceCurrent,
                    eValueComponentType.TargetResourceRatio,
                };
            }
            case eValueType.TargetingPriority:
            {
                return new List<eValueComponentType>()
                {
                    eValueComponentType.TargetResourceMax,
                    eValueComponentType.TargetResourceCurrent,
                    eValueComponentType.TargetResourceRatio,
                    eValueComponentType.TargetAttributeCurrent,
                    eValueComponentType.TargetAttributeBase,
                    eValueComponentType.TargetLevel,
                };
            }
            case eValueType.Aggro:
            {
                return new List<eValueComponentType>()
                {
                    eValueComponentType.FlatValue,
                    eValueComponentType.CasterAttributeCurrent,
                    eValueComponentType.CasterAttributeBase,
                    eValueComponentType.CasterResourceMax,
                    eValueComponentType.CasterResourceCurrent,
                    eValueComponentType.CasterResourceRatio,
                    eValueComponentType.CasterLevel,
                };
            }
        }

        return null;
    }
}

public class Value : List<ValueComponent>
{
    public Value Copy()
    {
        var value = new Value();
        foreach (var component in this)
        {
            value.Add(component.Copy);
        }

        return value;
    }

    public float GetValueFromAttributes(Entity entity, Dictionary<string, float> entityAttributes)
    {
        var totalValue = 0.0f;

        foreach (var value in this)
        {
            totalValue += value.GetValueFromAttribute(entity, entityAttributes);
        }

        return totalValue;
    }

    public float GetValue(Entity caster = null, Dictionary<string, float> casterAttributes = null, 
                          Entity target = null, Dictionary<string, float> targetAttributes = null,
                          Dictionary<string, ActionResult> actionResults = null, Value maxValue = null)
    {
        return OutgoingValues(caster, casterAttributes, actionResults).IncomingValue(target, targetAttributes, maxValue);
    }

    public Value OutgoingValues(Entity caster, Dictionary<string, float> casterAttributes, Dictionary<string, ActionResult> actionResults = null)
    {
        var value = new Value();
        var totalValue = 0.0f;

        foreach (var component in this)
        {
            if (component.ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                component.ComponentType == ValueComponent.eValueComponentType.TargetResourceMax)
            {
                value.Add(component);
            }
            else
            {
                totalValue += component.GetValue(caster, target: null, casterAttributes, targetAttributes: null, actionResults);
            }
        }

        if (totalValue > Constants.Epsilon || totalValue < -Constants.Epsilon)
        {
            value.Add(new ValueComponent(ValueComponent.eValueComponentType.FlatValue, totalValue));
        }

        return value;
    }

    public float IncomingValue(Entity target, Dictionary<string, float> targetAttributes, Value maxValue = null)
    {
        var totalValue = 0.0f;

        foreach (var component in this)
        {
            if (component.ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                component.ComponentType == ValueComponent.eValueComponentType.TargetResourceMax ||
                component.ComponentType == ValueComponent.eValueComponentType.TargetResourceRatio ||
                component.ComponentType == ValueComponent.eValueComponentType.FlatValue)
            {
                totalValue += component.GetValue(caster: null, target, casterAttributes: null, targetAttributes, actionResults: null);
            }
            else if (component.ComponentType == ValueComponent.eValueComponentType.CasterResourceCurrent ||
                     component.ComponentType == ValueComponent.eValueComponentType.CasterResourceMax ||
                     component.ComponentType == ValueComponent.eValueComponentType.CasterResourceRatio)
            {
                totalValue += component.GetValue(caster: target, target: null, casterAttributes: null, targetAttributes, actionResults: null);
            }
            else
            {
                Debug.LogError($"Incoming value can only be calculated on a value obtained through the OutgoingValue function.");
            }
        }

        if (maxValue != null && maxValue.Count > 0)
        {
            var max = maxValue.IncomingValue(target, targetAttributes);

            if (totalValue > 0.0f && totalValue > max)
            {
                totalValue = max;
            }
            else if (totalValue < 0.0f && totalValue < max)
            {
                totalValue = -max;
            }
        }

        return totalValue;
    }
}
