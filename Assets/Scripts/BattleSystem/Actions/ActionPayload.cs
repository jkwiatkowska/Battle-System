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
    public PayloadData PayloadData;

    public int TargetLimit;                 // Targets can be limited for actions that affect multiple targets
    public eTargetPriority TargetPriority;  // If there's a target limit, targets can be prioritised based on specified criteria

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, actionResults))
        {
            return;
        }

        var targets = GetTargetsForAction(entity, target);

        if (targets.Count == 0)
        {
            return;
        }

        if (TargetLimit > 0 && targets.Count > TargetLimit)
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
                    Debug.LogError($"Unimplemented target priority - {TargetPriority}");
                    break;
                }
            }
        }

        var payload = new Payload(entity, PayloadData, action: this, statusID: null, actionResults);
        foreach (var t in targets)
        {
            var result = new PayloadResult(PayloadData, entity, t, SkillID, ActionID);

            var immunity = t.HasImmunityAgainstAction(this);
            if (immunity != null)
            {
                // On hit resisted
                continue;
            }

            // Apply payload and update result if succesfull. 
            if (!payload.ApplyPayload(entity, t, result))
            {
                continue;
            }
            actionResults[ActionID].Value += result.Change;
            actionResults[ActionID].Count += 1;
        }

        actionResults[ActionID].Success = true;
        entity.OnActionUsed(this, actionResults[ActionID]);
    }

    public abstract List<Entity> GetTargetsForAction(Entity entity, Entity target);

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

        Debug.LogError($"Unimplemented target state: {TargetState}");
        return false;
    }
}
