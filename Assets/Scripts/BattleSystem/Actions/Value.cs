using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueComponent
{
    public enum eValueComponentType
    {
        FlatValue,
        CasterAttribute,
        CasterDepletableMax,
        CasterDepletableCurrent,
        TargetDepletableMax,
        TargetDepletableCurrent,
        ActionResultValue
    }
    public eValueComponentType ComponentType;

    public float Potency;                           // Multiplier
    public string Attribute;                        // The value can scale off different attributes, depletables or action results (for example damage dealt or cost collected).

    public ValueComponent(eValueComponentType type, float potency, string attribute = "")
    {
        ComponentType = type;
        Potency = potency;
        Attribute = attribute;
    }

    public float GetValue(Entity caster, Entity target, Dictionary<string, ActionResult> actionResults = null)
    {
        switch (ComponentType)
        {
            case eValueComponentType.FlatValue:
            {
                return Potency;
            }
            case eValueComponentType.CasterAttribute:
            {
                if (!caster.BaseAttributes.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}]. It should be an entity attribute name.");
                    return 0.0f;
                }
                return Potency * caster.BaseAttributes[Attribute];
            }
            case eValueComponentType.CasterDepletableMax:
            {
                if (!caster.DepletablesMax.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}]. The attribute should be a name of a depletable value.");
                    return 0.0f;
                }
                return Potency * caster.DepletablesMax[Attribute];
            }
            case eValueComponentType.CasterDepletableCurrent:
            {
                if (!caster.DepletablesCurrent.ContainsKey(Attribute))
                {
                    Debug.LogError($"Invalid attribute [{Attribute}]. The attribute should be a name of a depletable value.");
                    return 0.0f;
                }
                return Potency * caster.DepletablesCurrent[Attribute];
            }
            case eValueComponentType.TargetDepletableMax:
            {
                if (target == null || !target.DepletablesMax.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * target.DepletablesMax[Attribute];
            }
            case eValueComponentType.TargetDepletableCurrent:
            {
                if (target == null || !target.DepletablesCurrent.ContainsKey(Attribute))
                {
                    return 0.0f;
                }
                return Potency * caster.DepletablesCurrent[Attribute];
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
                Debug.LogError($"Unimplemented payload component type: {ComponentType}");
                return 0.0f;
            }
        }
    }
}

public class Value : List<ValueComponent>
{
    public Value OutgoingValues(Entity caster, Dictionary<string, ActionResult> actionResults)
    {
        var value = new Value();
        var totalValue = 0.0f;

        foreach (var component in this)
        {
            if (component.ComponentType == ValueComponent.eValueComponentType.TargetDepletableCurrent ||
                component.ComponentType == ValueComponent.eValueComponentType.TargetDepletableMax)
            {
                value.Add(component);
            }
            else
            {
                totalValue += component.GetValue(caster, null, actionResults);
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
            if (component.ComponentType == ValueComponent.eValueComponentType.TargetDepletableCurrent ||
                component.ComponentType == ValueComponent.eValueComponentType.TargetDepletableMax ||
                component.ComponentType == ValueComponent.eValueComponentType.FlatValue)
            {
                totalValue += component.GetValue(null, target, null);
            }
            else
            {
                Debug.LogError($"Incoming value can only be calculated on a value obtained through the OutgoingValue function.");
            }
        }

        return totalValue;
    }
}
