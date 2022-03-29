using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLoopBack : Action
{
    public float GoToTimestamp;
    public int Loops;

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        if (!actionResults.ContainsKey(ActionID))
        {
            actionResults[ActionID] = new ActionResult();
        }

        actionResults[ActionID].Count++;
        actionResults[ActionID].Success = ConditionsMet(entity, target, actionResults);
    }

    public override bool ConditionsMet(Entity entity, Entity target, Dictionary<string, ActionResult> actionResults)
    {
        if (!base.ConditionsMet(entity, target, actionResults))
        {
            return false;
        }

        return Loops == 0 || Loops > actionResults[ActionID].Count;
    }

    public override void SetTypeDefaults()
    {
        GoToTimestamp = 0.0f;
        Loops = 1;
    }
}
