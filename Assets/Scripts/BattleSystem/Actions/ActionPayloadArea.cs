using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionPayloadArea : ActionPayload
{
    public class Area
    {
        public enum eShape
        {
            Cylinder,
            Sphere,
            Cube
        }

        public eShape Shape;
        public Vector3 Dimensions;              // Cylinder: radius/cone angle/height
                                                // Sphere: Radius/cone angle
                                                // Cube: length/width/height
        public Vector2 InnerDimensions;         // Can make a smaller shape to create a cutout (resulting in a donut/frame-like shape)

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
            var foundPosition = area.AreaTransform.TryGetTransformFromData(entity, target, out var areaPos, out var areaForward);
            if (!foundPosition)
            {
                continue;
            }
            var areaPos2D = Utility.Get2DVector(areaPos);

            switch (area.Shape)
            {
                case Area.eShape.Cylinder:
                {
                    var minDistance = area.InnerDimensions.x * area.InnerDimensions.x;
                    var maxDistance = area.Dimensions.x * area.Dimensions.x;

                    var maxAngle = area.Dimensions.y * 0.5f;
                    var minAngle = area.InnerDimensions.y * 0.5f;

                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {
                        var t = potentialTargets[i];
                        var tPos = t.transform.position;

                        // Check if the target is at the correct height
                        var tBottom = tPos.y;
                        var tTop = tPos.y + t.EntityData.Height;
                        var areaBottom = areaPos.y;
                        var areaTop = areaPos.y + area.Dimensions.z;

                        if (tBottom < areaBottom || tTop > areaTop)
                        {
                            continue;
                        }

                        // Check if the target is inside circle
                        var distance = Utility.Distance2D(areaPos2D, t);
                        if (distance < minDistance || distance > maxDistance)
                        {
                            continue;
                        }

                        // Check if the target is inside cone
                        if (maxAngle < 180.0f || minAngle > 0) // If not a circle.
                        {
                            var direction = (tPos - areaPos).normalized;

                            var angle = Utility.Angle(areaForward, Utility.Get2DVector(direction));
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
                case Area.eShape.Sphere:
                {
                    var minDistance = area.InnerDimensions.x * area.InnerDimensions.x;
                    var maxDistance = area.Dimensions.x * area.Dimensions.x;

                    var maxAngle = area.Dimensions.y * 0.5f;
                    var minAngle = area.InnerDimensions.y * 0.5f;

                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {
                        var t = potentialTargets[i];
                        var tPos = t.transform.position;

                        // Check if the target is inside sphere
                        var distance = Utility.Distance3D(areaPos, t);
                        if (distance < minDistance || distance > maxDistance)
                        {
                            continue;
                        }

                        // Check if the target is inside cone
                        if (maxAngle < 180.0f || minAngle > 0) // If not a circle.
                        {
                            var direction = (tPos - areaPos).normalized;

                            var angle = Utility.Angle(areaForward, Utility.Get2DVector(direction));
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
                case Area.eShape.Cube:
                {
                    for (int i = potentialTargets.Count - 1; i >= 0; i--)
                    {
                        var t = potentialTargets[i];
                        var tPos2D = Utility.RotateAroundPosition(Utility.Get2DVector(t.transform.position), Utility.Angle(areaForward), areaPos2D);

                        // Check if the target is at the correct height
                        var tBottom = t.transform.position.y;
                        var tTop = tBottom + t.EntityData.Height;
                        var areaBottom = areaPos.y;
                        var areaTop = areaPos.y + area.Dimensions.z;

                        if (tBottom < areaBottom || tTop > areaTop)
                        {
                            continue;
                        }

                        // Check if the target is within outer bounds
                        var halfWidth = area.Dimensions.x * 0.5f + t.EntityData.Radius;
                        var maxX = halfWidth + areaPos.x;
                        var minX = -halfWidth + areaPos.x;

                        var halfLength = area.Dimensions.y * 0.5f + t.EntityData.Radius;
                        var maxY = halfLength + areaPos.z;
                        var minY = -halfLength + areaPos.z;

                        if (tPos2D.x < minX || tPos2D.x > maxX || tPos2D.y < minY || tPos2D.y > maxY)
                        {
                            continue;
                        }

                        // Ensure the target is outside inner bounds
                        halfWidth = area.InnerDimensions.x / 2 - t.EntityData.Radius;
                        halfLength = area.InnerDimensions.y / 2 - t.EntityData.Radius;

                        if (halfWidth > 0.0f && halfLength > 0.0f)
                        {
                            maxX = halfWidth + areaPos.x;
                            minX = -halfWidth + areaPos.x;

                            maxY = halfLength + areaPos.z;
                            minY = -halfLength + areaPos.z;

                            if (tPos2D.x > minX && tPos2D.x < maxX && tPos2D.y > minY && tPos2D.y < maxY)
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
