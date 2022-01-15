using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformData
{
    public enum ePositionOrigin
    {
        WorldPosition,          // A position in the world
        CasterPosition,         // The entity casting the skill
        SelectedTargetPosition, // Selected targetable
        TaggedEntityPosition,   // Entity referenced with a string tag
        //PositionFromInput     
    }

    public enum eTaggedTargetPriority
    {
        Random,
        Nearest,
        Furthest
    }

    public ePositionOrigin PositionOrigin;              // Where a skill is positioned
    public string EntityTag;                            // If using tagged entity position
    public eTaggedTargetPriority TaggedTargetPriority;  // If there is more than one entity with a given tag, this is used.

    public Vector2 PositionOffset;          // Position offset from position origin
    public Vector2 RandomPositionOffset;    // Range of a random offset from the summon position, for each x and y axis

    public float ForwardRotationOffset;     // The forward vector can be rotated.
    public float RandomForwardOffset;       // Randomness can be applied to this as well. 

    public bool TryGetTransformFromData(Entity entity, out Vector2 position, out Vector2 forward)
    {
        position = new Vector2();
        forward = new Vector2();

        var targetingSystem = entity.EntityTargetingSystem;

        switch (PositionOrigin)
        {
            case ePositionOrigin.WorldPosition:
            {
                position = Vector2.zero;
                forward = Vector2.zero;
                break;
            }
            case ePositionOrigin.CasterPosition:
            {
                position = Utility.Get2DPosition(entity.transform.position);
                forward = Utility.Get2DPosition(entity.transform.forward);
                break;
            }
            case ePositionOrigin.SelectedTargetPosition:
            {
                if (targetingSystem.SelectedTarget == null)
                {
                    Debug.LogError($"Area action requires a target, but none is selected.");
                }
                position = Utility.Get2DPosition(targetingSystem.SelectedTarget.transform.position);
                forward = Utility.Get2DPosition(targetingSystem.SelectedTarget.transform.forward);
                break;
            }
            case ePositionOrigin.TaggedEntityPosition:
            {
                if (entity.TaggedEntities.ContainsKey(EntityTag) && entity.TaggedEntities[EntityTag] != null && entity.TaggedEntities[EntityTag].Count > 0)
                {
                    var taggedEntity = entity.TaggedEntities[EntityTag][0];
                    var tagCount = entity.TaggedEntities[EntityTag].Count;
                    if (tagCount > 1)
                    {
                        switch (TaggedTargetPriority)
                        {
                            case eTaggedTargetPriority.Random:
                            {
                                taggedEntity = entity.TaggedEntities[EntityTag][Random.Range(0, tagCount)];
                                break;
                            }
                            case eTaggedTargetPriority.Nearest:
                            {
                                for (int i = 1; i < tagCount; i++)
                                {
                                    var taggedEntity2 = entity.TaggedEntities[EntityTag][i];
                                    if ((entity.transform.position - taggedEntity2.transform.position).sqrMagnitude < 
                                        (entity.transform.position - taggedEntity.transform.position).sqrMagnitude)
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
                                    var taggedEntity2 = entity.TaggedEntities[EntityTag][i];
                                    if ((entity.transform.position - taggedEntity2.transform.position).sqrMagnitude >
                                        (entity.transform.position - taggedEntity.transform.position).sqrMagnitude)
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
                    position = Utility.Get2DPosition(taggedEntity.transform.position);
                    forward = Utility.Get2DPosition(taggedEntity.transform.forward);
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

        // Add offsets.
        var forwardRotation = ForwardRotationOffset + Random.Range(0.0f, RandomForwardOffset);
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
        positionOffset *= forward;

        position += positionOffset;

        return true;
    }
}
