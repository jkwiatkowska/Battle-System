using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCostCollection : Action
{
    public string ResourceName;                 // Resource collected
    public Value Cost;

    public bool Optional;                       // If optional, the skill can be executed and continue without taking the cost. 
                                                // Can be used to change how skill works depending on whether the cost condition is met.

    public float GetValue(Entity entity)
    {
        var valueInfo = new ValueInfo(new EntityInfo(entity), targetInfo: null, actionResults: null);
        return Cost.CalculateValue(valueInfo);
    }

    public bool CanCollectCost(Entity entity)
    {
        return (GetValue(entity) <= entity.ResourcesCurrent[ResourceName]);
    }

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, target, actionResults))
        {
            return;
        }

        var value = GetValue(entity);

        entity.UpdateResource(ResourceName, -value);

        actionResults[ActionID].Success = true;
        actionResults[ActionID].Values["cost"] = value;
        entity.OnActionUsed(this, actionResults[ActionID], actionResults);
    }

    public override void SetTypeDefaults()
    {
        ResourceName = "";
        Cost = new Value();
        Optional = false;
    }
}
