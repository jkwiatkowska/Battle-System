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

    public override void Execute(Entity entity, out ActionResult actionResult, Entity target)
    {
        actionResult = new ActionResult();

        if (!ConditionsMet(entity))
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
                Debug.LogError($"Unsupported trigger target: {TriggerTarget}");
                break;
            }
        }
    }
}
