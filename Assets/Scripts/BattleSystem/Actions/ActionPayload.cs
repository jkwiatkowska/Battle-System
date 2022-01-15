using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionPayload : Action
{
    public enum eTarget
    {
        AllEntities,
        EnemyEntities,
        FriendlyEntities
    }

    public enum eTargetState
    {
        Alive,
        Dead,
        Any
    }

    public enum eTargetPriority
    {
        Random, 
        Nearest,
        Furthest
    }

    public eTarget Target;                  // Which entities the payload affects
    public eTargetState TargetState;        // What state the target has to be in to be affected
    public PayloadData Payload;

    public int TargetLimit;                 // Targets can be limited for actions that affect multiple targets
    public eTargetPriority TargetPriority;  // If there's a target limit, targets can be prioritised based on specified criteria

    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();

        if (!ConditionsMet(entity))
        {
            return;
        }

        var targets = GetTargetsForAction(entity);

        if (targets.Count == 0)
        {
            return;
        }

        if (targets.Count > TargetLimit)
        {
            switch (TargetPriority)
            {
                case eTargetPriority.Random:
                {
                    while (targets.Count > TargetLimit)
                    {
                        targets.RemoveAt(Random.Range(0, targets.Count));
                    }
                    break;
                }
                case eTargetPriority.Nearest:
                {
                    targets.Sort((t1, t2) => (entity.transform.position - t1.transform.position).sqrMagnitude.
                                    CompareTo((entity.transform.position - t2.transform.position).sqrMagnitude));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.Furthest:
                {
                    targets.Sort((t2, t1) => (entity.transform.position - t1.transform.position).sqrMagnitude.
                                    CompareTo((entity.transform.position - t2.transform.position).sqrMagnitude));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                default:
                {
                    Debug.LogError($"Unsupported target priority - {TargetPriority}");
                    break;
                }
            }
        }

        var payload = new Payload(entity, this);
        foreach (var target in targets)
        {
            var result = new PayloadResult(Payload, entity, target, SkillID, ActionID, 0.0f, new List<string>());

            // If payload isn't guaranteed to trigger.
            var chance = Formulae.PayloadSuccessChance(Payload, entity, target);
            if (Random.value > chance)
            {
                HUDPopupTextDisplay.Instance.DisplayMiss(target);
                entity.OnTrigger(TriggerData.eTrigger.OnHitMissed, result);
                continue;
            }

            // Apply payload and update result.
            payload.ApplyPayload(entity, target, result);
            actionResult.Value += result.Change;
            actionResult.Count += 1;

            entity.OnTrigger(TriggerData.eTrigger.OnHitOutgoing, result);
            target.OnTrigger(TriggerData.eTrigger.OnHitIncoming, result);

            // Show damage number on HUD
            if (result.Change != 0.0f)
            {
                HUDPopupTextDisplay.Instance.DisplayDamage(target, this, -result.Change, result.Flags);
            }
        }

        actionResult.Success = actionResult.Count > 0;
    }

    public abstract List<Entity> GetTargetsForAction(Entity entity);

    protected bool CheckTargetableState(Entity target)
    {
        if (TargetState == eTargetState.Any)
        {
            return true;
        }

        if (TargetState == eTargetState.Alive)
        {
            return target.Alive;
        }

        if (TargetState == eTargetState.Dead)
        {
            return !target.Alive;
        }

        Debug.LogError($"Unsupported target state: {TargetState}");
        return false;
    }
}
