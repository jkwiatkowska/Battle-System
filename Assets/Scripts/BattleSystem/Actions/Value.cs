using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueComponent
{
    public enum eValueComponentType
    {
        FlatValue,
        CasterAttribute,
        CasterResourceMax,
        CasterResourceCurrent,
        TargetResourceMax,
        TargetResourceCurrent,
        ActionResultValue
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

    public float GetValue(Dictionary<string, float> entityAttributes)
    {
        switch (ComponentType)
        {
            case eValueComponentType.CasterAttribute:
            {
                if (entityAttributes == null || !entityAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}].");
                    return 0.0f;
                }
                return Potency * entityAttributes[Attribute];
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
            case eValueComponentType.CasterAttribute:
            {
                if (casterAttributes == null || !casterAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}].");
                    return 0.0f;
                }
                return Potency * casterAttributes[Attribute];
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
    public float GetValue(Dictionary<string, float> entityAttributes)
    {
        var totalValue = 0.0f;

        foreach (var value in this)
        {
            totalValue += value.GetValue(entityAttributes);
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
            else
            {
                Debug.LogError($"Incoming value can only be calculated on a value obtained through the OutgoingValue function.");
            }
        }

        return totalValue;
    }
}
