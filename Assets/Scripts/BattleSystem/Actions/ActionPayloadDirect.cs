using System.Collections.Generic;
using UnityEngine;

public class ActionPayloadDirect : ActionPayload
{
    public enum eDirectActionTargets
    {
        Self,
        SelectedEntity,
        AllEntities,
        TaggedEntity
    }

    public eDirectActionTargets ActionTargets;

    public string EntityTag;

    public override List<Entity> GetTargetsForAction(Entity entity, Entity target)
    {
        var targets = new List<Entity>();

        switch (ActionTargets)
        {
            case eDirectActionTargets.Self:
            {
                targets.Add(entity);
                break;
            }
            case eDirectActionTargets.SelectedEntity:
            {
                // Return empty list if target was lost.
                if (target == null)
                {
                    return targets;
                }

                if (Target == eTarget.FriendlyEntities)
                {
                    if (BattleSystem.IsFriendly(entity.EntityUID, target.EntityUID))
                    {
                        targets.Add(entity.Target);
                    }
                    else
                    {
                        targets.Add(entity);
                    }
                }
                else if (Target == eTarget.EnemyEntities)
                {
                    if (BattleSystem.IsEnemy(entity.EntityUID, target.EntityUID))
                    {
                        targets.Add(entity.Target);
                    }
                    else
                    {
                        Debug.LogError($"Attempting to execute skill action {ActionID}, but no enemy target is selected.");
                    }
                }
                break;
            }
            case eDirectActionTargets.AllEntities:
            {
                var targetingSystem = entity.TargetingSystem;

                switch (Target)
                {
                    case eTarget.EnemyEntities:
                    {
                        targets = targetingSystem.EnemyEntities.FindAll(t => CheckTargetableState(t));
                        break;
                    }
                    case eTarget.FriendlyEntities:
                    {
                        targets = targetingSystem.FriendlyEntities.FindAll(t => CheckTargetableState(t));
                        break;
                    }
                    case eTarget.AllEntities:
                    {
                        targets = targetingSystem.AllEntities.FindAll(t => CheckTargetableState(t));
                        break;
                    }
                    default:
                    {
                        Debug.LogError($"{Target} target type not supported by area actions.");
                        break;
                    }
                }
                break;
            }
            case eDirectActionTargets.TaggedEntity:
            {
                var taggedEntities = entity.GetEntitiesWithTag(EntityTag);

                for (int i = 0; i < taggedEntities.Count; i++)
                {
                    targets.Add(taggedEntities[i]);
                }
                break;
            }
            default:
            {
                Debug.LogError($"Unsupported direct action target type: {TargetPriority}");
                break;
            }
        }

        return targets;
    }
}
