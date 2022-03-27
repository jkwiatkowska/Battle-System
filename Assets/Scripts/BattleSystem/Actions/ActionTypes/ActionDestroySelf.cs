using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDestroySelf : Action
{
    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, actionResults))
        {
            return;
        }

        entity.DestroyEntity();
        actionResults[ActionID].Success = true;
    }

    public override void SetTypeDefaults()
    {
        
    }
}
