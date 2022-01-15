using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCostCollection : Action
{
    public enum eCostValueType
    {
        FlatValue,                              // Value is not multiplied by anything
        CurrentMult,                            // Value is multiplied by an entity's current depletable value
        MaxMult                                 // Value is multiplied by an entity's max depletable value
    }

    public string DepletableName;               // One of the depletable attributes defined in game data.
    public eCostValueType ValueType;            // Determines how the value is calculated.
    public float Value;                         // How much is depleted.
    public bool Optional;                       // If optional, the skill can be executed and continue without taking the cost. 
                                                // Can be used to change how skill works depending on whether the cost condition is met.

    public override bool NeedsTarget()
    {
        return false;
    }

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
                return entity.DepletablesCurrent[DepletableName] * Value;
            }
            case eCostValueType.MaxMult:
            {
                return entity.DepletablesMax[DepletableName] * Value;
            }
            default:
            {
                Debug.LogError($"Unsupported value type: {ValueType}");
                return Value;
            }
        }
    }

    public bool CanCollectCost(Entity entity)
    {
        return (GetValue(entity) <= entity.DepletablesCurrent[DepletableName]);
    }

    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();

        if (!ConditionsMet(entity) || !CanCollectCost(entity))
        {
            return;
        }

        var value = -GetValue(entity);

        entity.ApplyChangeToDepletable(DepletableName, value);

        actionResult.Success = true;
        actionResult.Value = value;
    }
}
