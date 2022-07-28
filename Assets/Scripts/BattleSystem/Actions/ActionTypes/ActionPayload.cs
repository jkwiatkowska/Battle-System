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

    public Value SuccessChance;             // Chance of the payload being applied succesfully.

    public override void Execute(Entity caster, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(caster, target, actionResults))
        {
            return;
        }

        var targets = GetTargetsForAction(caster, target);

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
                    targets.Sort((t1, t2) => (caster.Position - t1.Position).sqrMagnitude.
                                CompareTo((caster.Position - t2.Position).sqrMagnitude));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.Furthest:
                {
                    targets.Sort((t2, t1) => (caster.Position - t2.Position).sqrMagnitude.
                                CompareTo((caster.Position - t1.Position).sqrMagnitude));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceCurrentLowest:
                {
                    targets.Sort((t1, t2) => t1.ResourcesCurrent[Resource].CompareTo(t2.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceCurrentHighest:
                {
                    targets.Sort((t1, t2) => t2.ResourcesCurrent[Resource].CompareTo(t1.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceMaxLowest:
                {
                    targets.Sort((t1, t2) => t1.ResourcesMax[Resource].CompareTo(t2.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceMaxHighest:
                {
                    targets.Sort((t1, t2) => t2.ResourcesMax[Resource].CompareTo(t1.ResourcesCurrent[Resource]));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceRatioLowest:
                {
                    targets.Sort((t1, t2) => t1.ResourceRatio(Resource).CompareTo(t2.ResourceRatio(Resource)));
                    targets = targets.GetRange(0, TargetLimit);
                    break;
                }
                case eTargetPriority.ResourceRatioHighest:
                {
                    targets.Sort((t1, t2) => t2.ResourceRatio(Resource).CompareTo(t1.ResourceRatio(Resource)));
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
                // Ensure target is in a correct state
                if (t == null || (TargetState == eTargetState.Alive && !t.Alive) || (TargetState == eTargetState.Dead && t.Alive))
                {
                    continue;
                }

                // Set up payload
                var payload = new Payload(caster, payloadData, action: this, actionResults: actionResults, statusID: null);

                // Chance
                if (SuccessChance != null)
                {
                    var chance = Formulae.PayloadSuccessChance(this, payload);
                    if (Random.value > chance)
                    {
                        caster.OnHitMissed(target, payload);
                        continue;
                    }
                }

                // Apply payload
                if (payload.ApplyPayload(t, out var results))
                {
                    actionResults[ActionID].Count += 1;
                }

                foreach (var result in results)
                {
                    foreach (var cat in result.Payload.PayloadData.Categories)
                    {
                        if (!actionResults[ActionID].Values.ContainsKey(cat))
                        {
                            actionResults[ActionID].Values[cat] = 0.0f;
                        }
                        actionResults[ActionID].Values[cat] += result.ResultValue;
                    }
                }
            }
        }

        actionResults[ActionID].Success = true;
        caster.OnActionUsed(this, actionResults[ActionID], actionResults);
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
