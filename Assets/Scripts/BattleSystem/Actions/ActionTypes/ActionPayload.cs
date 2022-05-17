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

    public enum eTargetPriority
    {
        Random,
        Furthest,
        Nearest,
        ResourceCurrentHighest,
        ResourceCurrentLowest,
        ResourceMaxHighest,
        ResourceMaxLowest,
        ResourceRatioHighest,
        ResourceRatioLowest,
    }

    public eTarget Target;                  // Which entities the payload affects.
    public eTargetState TargetState;        // What state the target has to be in to be affected.
    public List<PayloadData> PayloadData;

    public int TargetLimit;                 // Targets can be limited for actions that affect multiple targets.
    public eTargetPriority TargetPriority;  // If there's a target limit, targets can be prioritised based on specified criteria.
    public string Resource;                 // If targets are prioritised by how much of a resource they hold.

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, target, actionResults))
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
                    targets.Sort((t2, t1) => (entity.transform.position - t2.transform.position).sqrMagnitude.
                                CompareTo((entity.transform.position - t1.transform.position).sqrMagnitude));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceCurrentLowest:
                {
                    targets.Sort((t2, t1) => t1.ResourcesCurrent[Resource].CompareTo(t2.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceCurrentHighest:
                {
                    targets.Sort((t2, t1) => t2.ResourcesCurrent[Resource].CompareTo(t1.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceMaxLowest:
                {
                    targets.Sort((t2, t1) => t1.ResourcesMax[Resource].CompareTo(t2.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceMaxHighest:
                {
                    targets.Sort((t2, t1) => t2.ResourcesMax[Resource].CompareTo(t1.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceRatioLowest:
                {
                    targets.Sort((t2, t1) => t1.ResourceRatio(Resource).CompareTo(t2.ResourceRatio(Resource)));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceRatioHighest:
                {
                    targets.Sort((t2, t1) => t2.ResourceRatio(Resource).CompareTo(t1.ResourceRatio(Resource)));
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

        foreach (var payloadData in PayloadData)
        {
            foreach (var t in targets)
            {
                // Target is in an incorrect state.
                if (t == null || (TargetState == eTargetState.Alive && !t.Alive) || (TargetState == eTargetState.Dead && t.Alive))
                {
                    continue;
                }

                var payloadToApply = payloadData;

                // A payload can be applied if it has no conditions or conditions are met.
                var canApplyPayload = payloadToApply.PayloadCondition == null || !payloadToApply.PayloadCondition.CheckCondition(entity, t);

                // If conditions aren't met, but an alternate payload exist, check the alternate payload's conditions.
                // Keep going until a payload that can be applied is found or all options are exhausted.
                while (!canApplyPayload)
                {
                    payloadToApply = payloadToApply.AlternatePayload;
                    if (payloadToApply == null)
                    {
                        continue;
                    }

                    canApplyPayload = payloadToApply.PayloadCondition == null || payloadToApply.PayloadCondition.CheckCondition(entity, t);
                }

                var payload = new Payload(entity, payloadToApply, action: this, statusID: null, actionResults);
                var result = new PayloadResult(payloadData, entity, t, SkillID, ActionID);

                var immunity = t.HasImmunityAgainstAction(this);
                if (immunity != null)
                {
                    // On hit resisted
                    t.OnImmune(entity, result);
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

    public override void SetTypeDefaults()
    {
        Target = eTarget.EnemyEntities;
        TargetState = eTargetState.Alive;
        PayloadData = new List<PayloadData>();
        TargetLimit = 0;
        TargetPriority = eTargetPriority.Random;
    }
}
