using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPayloadArea : ActionPayload
{
    public class Area
    {
        public enum eShape
        {
            Circle,
            Rectangle
        }

        public eShape Shape;
        public Vector2 Dimensions;              // Rectangle length/width or circle radius/cone angle
        public Vector2 InnerDimensions;         // Can make a smaller shape to create a cutout (for example a donut)

        public TransformData AreaTransform;

        // To do: add a way to define falloff ratios
    }

    public List<Area> AreasAffected;

    public override bool NeedsTarget()
    {
        if (Target == eTarget.EnemyEntities)
        {
            foreach (var area in AreasAffected)
            {
                if (area.AreaTransform.PositionOrigin == TransformData.ePositionOrigin.SelectedTargetPosition)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();

        if (!ConditionMet(entity))
        {
            return;
        }
    }

    public override List<Entity> GetTargetsForAction(Entity entity)
    {
        var targets = new List<Entity>();
        var potentialTargets = new List<Entity>();
        var targetingSystem = entity.EntityTargetingSystem;

        switch (Target)
        {
            case eTarget.EnemyEntities:
            {
                potentialTargets = targetingSystem.GetAllEnemyEntites();
                break;
            }
            case eTarget.FriendlyEntities:
            {
                potentialTargets = targetingSystem.GetAllFriendlyEntites();
                break;
            }
            default:
            {
                Debug.LogError($"{Target} target type not supported by area actions.");
                break;
            }
        }

        foreach (var area in AreasAffected)
        {
            var foundPosition = area.AreaTransform.TryGetTransformFromData(entity, out Vector2 areaPosition, out Vector2 areaForward);
            if (!foundPosition)
            {
                continue;
            }

            switch (area.Shape)
            {
                case Area.eShape.Circle:
                {
                    var minDistance = area.InnerDimensions.x * area.InnerDimensions.x;
                    var maxDistance = area.Dimensions.x * area.Dimensions.x;

                    var minAngle = area.InnerDimensions.y;
                    var maxAngle = area.InnerDimensions.x;

                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {
                        var target = potentialTargets[i];
                        var targetPosition = Utility.Get2DPosition(target.transform.position);

                        // Check if the target is inside circle
                        var distance = Vector2.SqrMagnitude(areaPosition - targetPosition);
                        if (distance < minDistance || distance > maxDistance)
                        {
                            continue;
                        }

                        // Check if the target is inside cone
                        if (minAngle > 0.0f || maxAngle < 360.0f) // If not a circle
                        {
                            var angle = Vector2.Angle(areaPosition, targetPosition);
                            if (angle < minAngle || angle > maxAngle)
                            {
                                continue;
                            }
                        }

                        targets.Add(target);
                        potentialTargets.Remove(target);
                    }
                    break;
                }
                case Area.eShape.Rectangle:
                {
                    foreach (var target in potentialTargets)
                    {

                    }
                    break;
                }
            }
        }

        return targets;
    }
}
