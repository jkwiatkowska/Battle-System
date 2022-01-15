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

    public override bool NeedsTarget()
    {
        return ActionTargets == eDirectActionTargets.SelectedEntity && Target == eTarget.EnemyEntities;
    }

    public override List<Entity> GetTargetsForAction(Entity entity)
    {
        var targets = new List<Entity>();
        var targetingSystem = entity.EntityTargetingSystem;

        switch (ActionTargets)
        {
            case eDirectActionTargets.Self:
            {
                targets.Add(entity);
                break;
            }
            case eDirectActionTargets.SelectedEntity:
            {
                if (Target == eTarget.FriendlyEntities)
                {
                    if (targetingSystem.FriendlySelected)
                    {
                        targets.Add(targetingSystem.SelectedTarget.Entity);
                    }
                    else
                    {
                        targets.Add(entity);
                    }
                }
                else if (Target == eTarget.EnemyEntities)
                {
                    if (targetingSystem.EnemySelected)
                    {
                        targets.Add(targetingSystem.SelectedTarget.Entity);
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
                switch (Target)
                {
                    case eTarget.EnemyEntities:
                    {
                        targets = targetingSystem.GetAllEnemyEntities().FindAll(t => CheckTargetableState(t));
                        break;
                    }
                    case eTarget.FriendlyEntities:
                    {
                        targets = targetingSystem.GetAllFriendlyEntities().FindAll(t => CheckTargetableState(t));
                        break;
                    }
                    case eTarget.AllEntities:
                    {
                        targets = targetingSystem.GetAllTargetableEntities().FindAll(t => CheckTargetableState(t));
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
                if (entity.TaggedEntities.ContainsKey(EntityTag) && entity.TaggedEntities[EntityTag] != null)
                {
                    var taggedEntities = entity.TaggedEntities[EntityTag];
                    for (int i = 0; i < taggedEntities.Count; i++)
                    {
                        targets.Add(taggedEntities[i]);
                    }
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
