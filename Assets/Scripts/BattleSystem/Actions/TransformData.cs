using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformData
{
    public enum ePositionOrigin
    {
        WorldPosition,          // A position in the world
        CasterPosition,         // The entity casting the skill
        TargetPosition,         // Selected entity
        TaggedEntityPosition,   // Entity referenced with a string tag
        //PositionFromInput     
    }

    public enum eForwardSource
    {
        CasterForward,          // The entity casting the skill
        TargetForward,          // Forward of the target
        CasterToTarget,         // Direction vector between caster and target
        TaggedEntityForward,    // Forward of a tagged entity
        CasterToTaggedEntity,   // Direction vector between caster and tagged entity
        CasterToPositionOrigin, // Direction vector between caster and position origin
        NorthDirection          // North direction in the world
    }

    public enum eTaggedTargetPriority
    {
        Random,
        Nearest,
        Furthest
    }

    public ePositionOrigin PositionOrigin;              // Where a skill is positioned
    public eForwardSource ForwardSource;                // How the direction is determined
    public string EntityTag;                            // If using tagged entity position
    public eTaggedTargetPriority TaggedTargetPriority;  // If there is more than one entity with a given tag, this is used.

    public Vector3 PositionOffset;          // Position offset from position origin. Relative to forward direction.
    public Vector3 RandomPositionOffset;    // Range of a random offset from the summon position, for each x and y axis

    public float ForwardRotationOffset;     // The forward vector can be rotated.
    public float RandomForwardOffset;       // Randomness can be applied to this as well. 

    public bool TryGetTransformFromData(Entity caster, Entity target, out Vector3 position, out Vector2 forward)
    {
        position = new Vector3();
        forward = new Vector3();

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
                var taggedEntity = ChooseTaggedEntity(caster);
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
            default:
            {
                Debug.LogError($"Unsupported position origin type: {PositionOrigin}");
                break;
            }
        }

        switch (ForwardSource)
        {
            case eForwardSource.CasterForward:
            {
                forward = Utility.Get2DVector(caster.transform.forward);
                break;
            }
            case eForwardSource.TargetForward:
            {
                if (target != null)
                {
                    forward = Utility.Get2DVector(target.transform.forward);
                }
                else
                {
                    // No target
                    return false;
                }
                break;
            }
            case eForwardSource.CasterToTarget:
            {
                if (target != null)
                {
                    forward = Utility.Get2DVector(target.transform.position - caster.transform.position).normalized;
                }
                else
                {
                    // No target
                    return false;
                }
                break;
            }
            case eForwardSource.TaggedEntityForward:
            {
                var taggedEntity = ChooseTaggedEntity(caster);
                if (taggedEntity != null)
                {
                    forward = Utility.Get2DVector(taggedEntity.transform.forward);
                }
                else
                {
                    // No tagged entity
                    return false;
                }
                break;
            }
            case eForwardSource.CasterToTaggedEntity:
            {
                var taggedEntity = ChooseTaggedEntity(caster);
                if (taggedEntity != null)
                {
                    forward = Utility.Get2DVector(taggedEntity.transform.forward - caster.transform.position).normalized;
                }
                else
                {
                    // No tagged entity
                    return false;
                }
                break;
            }
            case eForwardSource.CasterToPositionOrigin:
            {
                forward = Utility.Get2DVector(position - caster.transform.position).normalized;
                break;
            }
            case eForwardSource.NorthDirection:
            {
                forward = Vector2.up;
                break;
            }
            default:
            {
                Debug.LogError($"Unsupported forward source: {ForwardSource}");
                return false;
            }
        }

        // Add offsets.
        var randomOffset = RandomForwardOffset * (Random.value - 0.5f);
        var forwardRotation = ForwardRotationOffset + Random.Range(0.0f, randomOffset);
        forward = Utility.Rotate(forward, forwardRotation);
        forward.Normalize();

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
        var positionOffset2D = Utility.Get2DVector(positionOffset) * Utility.Get2DVector(forward);
        positionOffset.x = positionOffset2D.x;
        positionOffset.z = positionOffset2D.y;

        position += positionOffset;

        return true;
    }

    Entity ChooseTaggedEntity(Entity caster)
    {
        var taggedEntities = caster.GetEntitiesWithTag(EntityTag);
        if (taggedEntities.Count > 0)
        {
            var taggedEntity = taggedEntities[0];
            var tagCount = taggedEntities.Count;
            if (tagCount > 1)
            {
                switch (TaggedTargetPriority)
                {
                    case eTaggedTargetPriority.Random:
                    {
                        taggedEntity = taggedEntities[Random.Range(0, tagCount)];
                        break;
                    }
                    case eTaggedTargetPriority.Nearest:
                    {
                        for (int i = 1; i < tagCount; i++)
                        {
                            var taggedEntity2 = taggedEntities[i];
                            if ((caster.transform.position - taggedEntity2.transform.position).sqrMagnitude <
                                (caster.transform.position - taggedEntity.transform.position).sqrMagnitude)
                            {
                                taggedEntity = taggedEntity2;
                            }
                        }
                        break;
                    }
                    case eTaggedTargetPriority.Furthest:
                    {
                        for (int i = 1; i < tagCount; i++)
                        {
                            var taggedEntity2 = taggedEntities[i];
                            if ((caster.transform.position - taggedEntity2.transform.position).sqrMagnitude >
                                (caster.transform.position - taggedEntity.transform.position).sqrMagnitude)
                            {
                                taggedEntity = taggedEntity2;
                            }
                        }
                        break;
                    }
                    default:
                    {
                        Debug.LogError($"Unsupported tagged target priority: {TaggedTargetPriority}");
                        break;
                    }
                }
            }
            return taggedEntity;
        }
        else return null;
    }
}
