using System.Collections.Generic;
using UnityEngine;

public class ActionPayloadDirect : ActionPayload
{
    public enum eDirectActionTargets
    {
        SelectedEntity,
        AllEntities,
        RandomEntities,
        TaggedEntity
    }

    public eDirectActionTargets ActionTargets;
    public int TargetCount;
    public string EntityTag;

    public override bool NeedsTarget()
    {
        return ActionTargets != eDirectActionTargets.SelectedEntity && Target == eTarget.EnemyEntities;
    }

    public override List<Entity> GetTargetsForAction(Entity entity)
    {
        var targets = new List<Entity>();
        var targetingSystem = entity.EntityTargetingSystem;

        switch (ActionTargets)
        {
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
                if (Target == eTarget.FriendlyEntities)
                {
                    targets = targetingSystem.GetAllFriendlyEntites();
                }
                else if (Target == eTarget.EnemyEntities)
                {
                    targets = targetingSystem.GetAllEnemyEntites();
                }
                break;
            }
            case eDirectActionTargets.RandomEntities:
            {
                if (Target == eTarget.FriendlyEntities)
                {
                    targets = targetingSystem.GetAllFriendlyEntites();
                }
                else if (Target == eTarget.EnemyEntities)
                {
                    targets = targetingSystem.GetAllEnemyEntites();
                }

                // Randomly remove entities from list until the desired number is left
                while (targets.Count > TargetCount)
                {
                    targets.RemoveAt(Random.Range(0, targets.Count));
                }
                break;
            }
            case eDirectActionTargets.TaggedEntity:
            {
                if (entity.TaggedEntities.ContainsKey(EntityTag) && entity.TaggedEntities[EntityTag] != null)
                {
                    targets.Add(entity.TaggedEntities[EntityTag]);
                }
                break;
            }
            default:
            {
                Debug.LogError($"Unsupported direct action target type: {ActionTargets}");
                break;
            }
        }

        return targets;
    }
}
