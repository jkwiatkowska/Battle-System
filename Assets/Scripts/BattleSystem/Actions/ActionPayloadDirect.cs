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
                if (CheckTargetableState(entity))
                {
                    targets.Add(entity);
                }
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
                    if (entity.IsFriendly(target.Faction))
                    {
                        if (CheckTargetableState(target))
                        {
                            targets.Add(target);
                        }
                    }
                    else
                    {
                        if (CheckTargetableState(entity))
                        {
                            targets.Add(entity);
                        }
                    }
                }
                else if (Target == eTarget.EnemyEntities)
                {
                    if (entity.IsEnemy(target.Faction))
                    {
                        if (CheckTargetableState(target))
                        {
                            targets.Add(target);
                        }
                    }
                    else
                    {
                        return targets;
                    }
                }
                break;
            }
            case eDirectActionTargets.AllEntities:
            {
                var targetingSystem = entity.EntityTargetingSystem;

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
                        targets = targetingSystem.DetectedEntities.FindAll(t => CheckTargetableState(t));
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
                Debug.LogError($"Unimplemented direct action target type: {TargetPriority}");
                break;
            }
        }

        return targets;
    }

    public override void SetTypeDefaults()
    {
        base.SetTypeDefaults();

        ActionTargets = eDirectActionTargets.SelectedEntity;
        EntityTag = "";
    }
}
