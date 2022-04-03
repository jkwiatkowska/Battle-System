using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformData
{
    public enum ePositionOrigin
    {
        WorldPosition,          // A position in the world.
        EntityPosition,         // World position of an entity.
        EntityOrigin,           // Origin of an entity.
        //PositionFromInput     
    }

    public ePositionOrigin PositionOrigin;              // Where a skill is positioned.
    public TransformTargetEntity TargetEntity;          // For entity origins.

    public DirectionData Direction;                     // Direction along which the offset is applied.
    public Vector3 PositionOffset;                      // Position offset from position origin. Relative to forward direction.
    public Vector3 RandomPositionOffset;                // Range of a random offset from the summon position, for each x and y axis.

    public TransformData()
    {
        PositionOrigin = ePositionOrigin.EntityPosition;
        TargetEntity = new TransformTargetEntity();
        Direction = new DirectionData();
        PositionOffset = Vector3.zero;
        RandomPositionOffset = Vector3.zero;
    }

    public bool TryGetTransformFromData(Entity caster, Entity target, out Vector3 position, out Vector3 forward)
    {
        position = new Vector3();
        if (!Direction.TryGetDirectionFromData(caster, target, out forward))
        {
            return false;
        }

        var positionOrigin = PositionOrigin;
        switch (positionOrigin)
        {
            case ePositionOrigin.WorldPosition:
            {
                position = Vector3.zero;
                break;
            }
            case ePositionOrigin.EntityPosition:
            {
                var entity = TargetEntity.GetEntity(caster, target);
                if (entity == null)
                {
                    return false;
                }

                position = entity.transform.position;
                break;
            }
            case ePositionOrigin.EntityOrigin:
            {
                var entity = TargetEntity.GetEntity(caster, target);
                if (entity == null)
                {
                    return false;
                }

                position = entity.Origin;
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented position origin type: {PositionOrigin}");
                break;
            }
        }

        // Offset
        var positionOffset = PositionOffset;

        if (RandomPositionOffset.x != 0.0f)
        {
            positionOffset.x += Random.Range(0.0f, RandomPositionOffset.x);
        }
        if (RandomPositionOffset.y != 0.0f)
        {
            positionOffset.y += Random.Range(0.0f, RandomPositionOffset.y);
        }
        if (RandomPositionOffset.z != 0.0f)
        {
            positionOffset.z += Random.Range(0.0f, RandomPositionOffset.z);
        }

        positionOffset = VectorUtility.ApplyDirection(positionOffset, forward);
        position += positionOffset;

        return true;
    }
}

public class DirectionData
{
    public enum eDirectionSource
    {
        EntityForward,          // Facing direction of the entity.
        EntityToEntity,         // Direction from one entity to another.
        WorldForward,           // North direction in the world.
    }

    public eDirectionSource DirectionSource;            // How the direction is determined

    public TransformTargetEntity EntityFrom;            // For entity direction sources.
    public TransformTargetEntity EntityTo;

    public float DirectionOffset;                       // The forward vector can be rotated around its Y axis.
    public float RandomDirectionOffset;                 // Randomness can be applied to this as well. 

    public DirectionData()
    {
        DirectionOffset = 0.0f;
        RandomDirectionOffset = 0.0f;
        EntityFrom = new TransformTargetEntity();
        EntityTo = new TransformTargetEntity();
    }

    public bool TryGetDirectionFromData(Entity caster, Entity target, out Vector3 forward)
    {
        forward = new Vector3();

        var directionSource = DirectionSource;

        switch (directionSource)
        {
            case eDirectionSource.EntityForward:
            {
                var entity = EntityFrom.GetEntity(caster, target);
                if (entity == null)
                {
                    return false;
                }
                forward = entity.transform.forward;
                break;
            }
            case eDirectionSource.EntityToEntity:
            {
                var from = EntityFrom.GetEntity(caster, target);
                var to = EntityTo.GetEntity(caster, target);
                if (from == null || to == null)
                {
                    return false;
                }
                else
                {
                    forward = (to.transform.position - from.transform.position).normalized;
                }
                break;
            }
            case eDirectionSource.WorldForward:
            {
                forward = Vector3.forward;
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented forward source: {DirectionSource}");
                return false;
            }
        }

        // Add offsets.
        var randomOffset = RandomDirectionOffset * (Random.value - 0.5f);
        var forwardRotation = DirectionOffset + Random.Range(0.0f, randomOffset);
        forward = VectorUtility.Rotate(forward, forwardRotation);
        forward.Normalize();
        return true;
    }
}

public class TransformTargetEntity
{
    public enum eEntity
    {
        Caster,
        Target,
        Selected,
        Tagged,
    }

    public eEntity EntityTarget;

    public string EntityTag;                            // If using tagged entity position.
    public TagData.eTagPriority TaggedTargetPriority;   // If there is more than one entity with a given tag, this is used.

    public TransformTargetEntity()
    {
        EntityTarget = eEntity.Caster;
    }

    public Entity GetEntity(Entity caster, Entity target)
    {
        var eTarget = EntityTarget;

        if (eTarget == eEntity.Target && caster == target)
        {
            eTarget = eEntity.Selected;
        }

        switch (eTarget)
        {
            case eEntity.Caster:
            {
                return caster;
            }
            case eEntity.Target:
            {
                return target;
            }
            case eEntity.Selected:
            {
                return caster?.Target;
            }
            case eEntity.Tagged:
            {
                if (caster == null)
                {
                    return null;
                }

                return TagData.ChooseTaggedEntity(caster, EntityTag, TaggedTargetPriority);
            }
            default:
            {
                Debug.LogError($"Unimplemented transform target: {EntityTarget}");
                return null;
            }
        }
    }
}