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

                    var maxAngle = area.Dimensions.y / 2.0f;
                    var minAngle = area.InnerDimensions.y / 2.0f;

                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {
                        var t = potentialTargets[i];
                        var tPos = Utility.Get2DPosition(t.transform.position);

                        // Check if the target is inside circle
                        var distance = Utility.Distance(areaPos, t);
                        if (distance < minDistance || distance > maxDistance)
                        {
                            continue;
                        }

                        // Check if the target is inside cone
                        if (maxAngle < 180.0f || minAngle > 0) // If not a circle.  
                        {
                            var direction = (tPos - areaPos).normalized;
                            var angle = Utility.Angle(areaForward, direction);
                            if (angle > maxAngle || angle < minAngle)
                            {
                                continue;
                            }
                        }

                        // Target is inside area.
                        targets.Add(t);

                        // Each target can only be hit once, so remove it from the list of potential targets. 
                        potentialTargets.Remove(t);
                    }
                    break;
                }
                case Area.eShape.Rectangle:
                {
                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {

                    }
                    break;
                }
                default:
                {
                    Debug.LogError($"Unsupported area shape: {area.Shape}.");
                    break;
                }
            }
        }

        return targets;
    }
}
