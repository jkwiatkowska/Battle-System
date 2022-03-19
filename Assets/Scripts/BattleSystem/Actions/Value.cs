using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueComponent
{
    public enum eValueComponentType
    {
        FlatValue               = 0,
        CasterAttributeBase     = 1,
        CasterAttributeCurrent  = 2,
        CasterResourceMax       = 3,
        CasterResourceCurrent   = 4,
        TargetResourceMax       = 5,
        TargetResourceCurrent   = 6,
        ActionResultValue       = 7,
    }
    public eValueComponentType ComponentType;

    public float Potency;                           // Multiplier
    public string Attribute;                        // The value can scale off different attributes, resources or action results (for example damage dealt or cost collected).

    public ValueComponent(eValueComponentType type, float potency, string attribute = "")
    {
        ComponentType = type;
        Potency = potency;
        Attribute = attribute;
    }

    public ValueComponent Copy => new ValueComponent(ComponentType, Potency, Attribute);

    public float GetValue(Entity entity, Dictionary<string, float> entityAttributes)
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

    public float GetValue(Entity caster, Entity target, Dictionary<string, float> casterAttributes, Dictionary<string, ActionResult> actionResults = null)
    {
        switch (ComponentType)
        {
            case eValueComponentType.FlatValue:
            {
                return Potency;
            }
            case eValueComponentType.CasterAttributeCurrent:
            {
                if (casterAttributes == null || !casterAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}].");
                    return 0.0f;
                }
                return Potency * casterAttributes[Attribute];
            }
            case eValueComponentType.CasterAttributeBase:
            {
                if (caster == null || !caster.BaseAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}].");
                    return 0.0f;
                }
                return Potency * caster.BaseAttribute(Attribute);
            }
            case eValueComponentType.CasterResourceMax:
            {
                if (!caster.ResourcesMax.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}]. The attribute should be a name of a resource value.");
                    return 0.0f;
                }
                return Potency * caster.ResourcesMax[Attribute];
            }
            case eValueComponentType.CasterResourceCurrent:
            {
                if (!caster.ResourcesCurrent.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}]. The attribute should be a name of a resource value.");
                    return 0.0f;
                }
                return Potency * caster.ResourcesCurrent[Attribute];
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
            case eValueComponentType.ActionResultValue:
            {
                if (actionResults != null && actionResults.ContainsKey(Attribute))
                {
                    return actionResults[Attribute].Value;
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

    public float GetValue(Entity entity, Dictionary<string, float> entityAttributes)
    {
        var totalValue = 0.0f;

        foreach (var value in this)
        {
            totalValue += value.GetValue(entity, entityAttributes);
        }

        return totalValue;
    }

    public Value OutgoingValues(Entity caster, Dictionary<string, float> casterAttributes, Dictionary<string, ActionResult> actionResults)
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
                totalValue += component.GetValue(caster, target: null, casterAttributes, actionResults);
            }
        }

        if (totalValue > Constants.Epsilon || totalValue < -Constants.Epsilon)
        {
            value.Add(new ValueComponent(ValueComponent.eValueComponentType.FlatValue, totalValue));
        }

        return value;
    }

    public float IncomingValue(Entity target)
    {
        var totalValue = 0.0f;

        foreach (var component in this)
        {
            if (component.ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                component.ComponentType == ValueComponent.eValueComponentType.TargetResourceMax ||
                component.ComponentType == ValueComponent.eValueComponentType.FlatValue)
            {
                totalValue += component.GetValue(caster: null, target, casterAttributes: null, actionResults: null);
            }
            else if (component.ComponentType == ValueComponent.eValueComponentType.CasterResourceCurrent ||
                     component.ComponentType == ValueComponent.eValueComponentType.CasterResourceMax)
            {
                totalValue += component.GetValue(caster: target, target: null, casterAttributes: null, actionResults: null);
            }
            else
            {
                Debug.LogError($"Incoming value can only be calculated on a value obtained through the OutgoingValue function.");
            }
        }

        return totalValue;
    }
}
