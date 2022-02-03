using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTrigger : Action
{
    public TriggerData.eTrigger TriggerType;
    public enum eTriggerTarget
    {
        Caster,
        Target
    }
    public eTriggerTarget TriggerTarget;

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, actionResults))
        {
            return;
        }

        switch (TriggerTarget)
        {
            case eTriggerTarget.Caster:
            {
                entity.OnTrigger(TriggerType, entity);
                break;
            }
            case eTriggerTarget.Target:
            {
                target.OnTrigger(TriggerType, entity);
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented trigger target: {TriggerTarget}");
                break;
            }
        }

        actionResults[ActionID].Success = true;
    }
}
