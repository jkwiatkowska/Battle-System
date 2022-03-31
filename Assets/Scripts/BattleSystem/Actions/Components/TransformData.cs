using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformData
{
    public enum ePositionOrigin
    {
        WorldPosition,          // A position in the world.
        CasterPosition,         // The entity casting the skill.
        TargetPosition,         // Selected entity.
        TaggedEntityPosition,   // Entity referenced with a string tag.
        CasterOrigin,           // Center of the entity casting the skill.
        TargetOrigin,           // Center of the selected entity.
        TaggedEntityOrigin,     // Center of tagged entity.
        //PositionFromInput     
    }

    public ePositionOrigin PositionOrigin;              // Where a skill is positioned.

    public DirectionData Direction;                     // Direction along which the offset is applied.
    public Vector3 PositionOffset;                      // Position offset from position origin. Relative to forward direction.
    public Vector3 RandomPositionOffset;                // Range of a random offset from the summon position, for each x and y axis.

    public string EntityTag;                            // If using tagged entity position.
    public TagData.eTagPriority TaggedTargetPriority;   // If there is more than one entity with a given tag, this is used.

    public TransformData()
    {
        PositionOrigin = ePositionOrigin.CasterPosition;
        Direction = new DirectionData();
        EntityTag = "";
        TaggedTargetPriority = TagData.eTagPriority.Random;
        PositionOffset = Vector3.zero;
        RandomPositionOffset = Vector3.zero;
    }

    public bool TryGetTransformFromData(Entity caster, Entity target, out Vector3 position, out Vector3 forward)
    {
        position = new Vector3();
        if (!Direction.TryGetRotationFromData(caster, target, out forward))
        {
            return false;
        }

        switch (PositionOrigin)
        {
            case ePositionOrigin.WorldPosition:
            {
                position = Vector3.zero;
                break;
            }
            case ePositionOrigin.CasterPosition:
            {
                position = caster.transform.position;
                break;
            }
            case ePositionOrigin.TargetPosition:
            {
                // No position if target was lost. 
                if (target == null)
                {
                    return false;
                }

                position = target.transform.position;
                break;
            }
            case ePositionOrigin.TaggedEntityPosition:
            {
                var taggedEntity = TagData.ChooseTaggedEntity(caster, EntityTag, TaggedTargetPriority);
                if (taggedEntity != null)
                { 
                    position = taggedEntity.transform.position;
                }
                else
                {
                    // No tagged entity
                    return false;
                }
                break;
            }
            case ePositionOrigin.CasterOrigin:
            {
                position = caster.Origin;
                break;
            }
            case ePositionOrigin.TargetOrigin:
            {
                // No position if target was lost. 
                if (target == null)
                {
                    return false;
                }

                position = target.Origin;
                break;
            }
            case ePositionOrigin.TaggedEntityOrigin:
            {
                var taggedEntity = TagData.ChooseTaggedEntity(caster, EntityTag, TaggedTargetPriority);
                if (taggedEntity != null)
                {
                    position = taggedEntity.Origin;
                }
                else
                {
                    // No tagged entity
                    return false;
                }
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
        CasterForward,          // The entity casting the skill.
        TargetForward,          // Forward of the target.
        CasterToTarget,         // Direction vector between caster and target.
        TaggedEntityForward,    // Forward of a tagged entity.
        CasterToTaggedEntity,   // Direction vector between caster and tagged entity.
        Vector3Forward,         // North direction in the world.
    }

    public eDirectionSource DirectionSource;            // How the direction is determined
    public float DirectionOffset;                       // The forward vector can be rotated around its Y axis.
    public float RandomDirectionOffset;                 // Randomness can be applied to this as well. 

    public string EntityTag;                            // If using tagged entity position
    public TagData.eTagPriority TaggedTargetPriority;   // If there is more than one entity with a given tag, this is used.

    public DirectionData()
    {
        DirectionOffset = 0.0f;
        RandomDirectionOffset = 0.0f;
    }

    public bool TryGetRotationFromData(Entity caster, Entity target, out Vector3 forward)
    {
        forward = new Vector3();

        switch (DirectionSource)
        {
            case eDirectionSource.CasterForward:
            {
                forward = caster.transform.forward;
                break;
            }
            case eDirectionSource.TargetForward:
            {
                if (target != null)
                {
                    forward = target.transform.forward;
                }
                else
                {
                    // No target
                    return false;
                }
                break;
            }
            case eDirectionSource.CasterToTarget:
            {
                if (target != null)
                {
                    forward = (target.transform.position - caster.transform.position).normalized;
                }
                else
                {
                    // No target
                    return false;
                }
                break;
            }
            case eDirectionSource.TaggedEntityForward:
            {
                var taggedEntity = TagData.ChooseTaggedEntity(caster, EntityTag, TaggedTargetPriority);
                if (taggedEntity != null)
                {
                    forward = taggedEntity.transform.forward;
                }
                else
                {
                    // No tagged entity
                    return false;
                }
                break;
            }
            case eDirectionSource.CasterToTaggedEntity:
            {
                var taggedEntity = TagData.ChooseTaggedEntity(caster, EntityTag, TaggedTargetPriority);
                if (taggedEntity != null)
                {
                    forward = (taggedEntity.transform.forward - caster.transform.position).normalized;
                }
                else
                {
                    // No tagged entity
                    return false;
                }
                break;
            }
            case eDirectionSource.Vector3Forward:
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