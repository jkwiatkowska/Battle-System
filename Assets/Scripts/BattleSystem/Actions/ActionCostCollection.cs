using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCostCollection : Action
{
    public enum eCostValueType
    {
        FlatValue,                              // Value is not multiplied by anything
        CurrentMult,                            // Value is multiplied by an entity's current resource value
        MaxMult                                 // Value is multiplied by an entity's max resource value
    }

    public string ResourceName;               // One of the resource attributes defined in game data.
    public eCostValueType ValueType;            // Determines how the value is calculated.
    public float Value;                         // How much is depleted.
    public bool Optional;                       // If optional, the skill can be executed and continue without taking the cost. 
                                                // Can be used to change how skill works depending on whether the cost condition is met.

    public float GetValue(Entity entity)
    {
        switch (ValueType)
        {
            case eCostValueType.FlatValue:
            {
                return Value;
            }
            case eCostValueType.CurrentMult:
            {
                return entity.ResourcesCurrent[ResourceName] * Value;
            }
            case eCostValueType.MaxMult:
            {
                return entity.ResourcesMax[ResourceName] * Value;
            }
            default:
            {
                Debug.LogError($"Unimplemented value type: {ValueType}");
                return Value;
            }
        }
    }

    public bool CanCollectCost(Entity entity)
    {
        return (GetValue(entity) <= entity.ResourcesCurrent[ResourceName]);
    }

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, actionResults) || !CanCollectCost(entity))
        {
            return;
        }

        var value = -GetValue(entity);

        entity.ApplyChangeToResource(ResourceName, value);

        actionResults[ActionID].Success = true;
        actionResults[ActionID].Value = value;
    }

    public override void SetTypeDefaults()
    {
        ResourceName = "";
        ValueType = eCostValueType.FlatValue;
        Value = 100.0f;
        Optional = false;
    }
}
