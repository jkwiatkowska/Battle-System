using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagData
{
    public enum eTagPriority
    {
        Random,
        Nearest,
        Furthest
    }

    public string TagID;
    public int TagLimit;
    public float TagDuration;

    public static Entity ChooseTaggedEntity(Entity caster, string tag, eTagPriority tagPriority)
    {
        var taggedEntities = caster.GetEntitiesWithTag(tag);
        if (taggedEntities.Count > 0)
        {
            var taggedEntity = taggedEntities[0];
            var tagCount = taggedEntities.Count;
            if (tagCount > 1)
            {
                switch (tagPriority)
                {
                    case TagData.eTagPriority.Random:
                    {
                        taggedEntity = taggedEntities[Random.Range(0, tagCount)];
                        break;
                    }
                    case TagData.eTagPriority.Nearest:
                    {
                        for (int i = 1; i < tagCount; i++)
                        {
                            var taggedEntity2 = taggedEntities[i];
                            if ((caster.Position - taggedEntity2.Position).sqrMagnitude <
                                (caster.Position - taggedEntity.Position).sqrMagnitude)
                            {
                                taggedEntity = taggedEntity2;
                            }
                        }
                        break;
                    }
                    case TagData.eTagPriority.Furthest:
                    {
                        for (int i = 1; i < tagCount; i++)
                        {
                            var taggedEntity2 = taggedEntities[i];
                            if ((caster.Position - taggedEntity2.Position).sqrMagnitude >
                                (caster.Position - taggedEntity.Position).sqrMagnitude)
                            {
                                taggedEntity = taggedEntity2;
                            }
                        }
                        break;
                    }
                    default:
                    {
                        Debug.LogError($"Unimplemented tagged target priority: {tagPriority}");
                        break;
                    }
                }
            }
            return taggedEntity;
        }
        else return null;
    }
}
