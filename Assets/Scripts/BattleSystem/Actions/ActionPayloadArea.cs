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

    public override List<Entity> GetTargetsForAction(Entity entity, Entity target)
    {
        var targets = new List<Entity>();
        var potentialTargets = new List<Entity>();
        var targetingSystem = entity.EntityTargetingSystem;

        switch (Target)
        {
            case eTarget.EnemyEntities:
            {
                potentialTargets = targetingSystem.EnemyEntities.FindAll(t => CheckTargetableState(t));
                break;
            }
            case eTarget.FriendlyEntities:
            {
                potentialTargets = targetingSystem.FriendlyEntities.FindAll(t => CheckTargetableState(t));
                break;
            }
            case eTarget.AllEntities:
            {
                potentialTargets = targetingSystem.AllEntities.FindAll(t => CheckTargetableState(t));
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
            var foundPosition = area.AreaTransform.TryGetTransformFromData(entity, target, out Vector2 areaPos, out Vector2 areaForward);
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
                        var t = potentialTargets[i];
                        var tPos = Utility.Get2DPosition(t.transform.position);

                        // Check if the target is inside circle
                        var distance = Vector2.SqrMagnitude(areaPos - tPos);
                        if (distance < minDistance || distance > maxDistance)
                        {
                            continue;
                        }

                        // Check if the target is inside cone
                        if (minAngle > 0.0f || maxAngle < 360.0f) // If not a circle
                        {
                            var angle = Vector2.Angle(areaPos, tPos);
                            if (angle < minAngle || angle > maxAngle)
                            {
                                continue;
                            }
                        }

                        targets.Add(t);
                        potentialTargets.Remove(t);
                    }
                    break;
                }
                case Area.eShape.Rectangle:
                {
                    foreach (var t in potentialTargets)
                    {

                    }
                    break;
                }
            }
        }

        return targets;
    }
}
