using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDoNothing : Action
{
    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        actionResults[ActionID].Success = !ConditionsMet(entity, target, actionResults);
    }

    public override void SetTypeDefaults()
    {
        
    }
}
