using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData
{
    public Dictionary<string, Vector2> BaseAttributes;  // Attributes like atk, def, etc. Values at level 1 and max level. 
    public Dictionary<string, Vector2> MaxDepletables;  // Depletable values like hp, mp and stamina at level 1 and max level.

    public bool IsTargetable;                           // If true, skills can be used on the entity.

    public string Faction;

    public bool IsAI;                                   // AI entities hold a list of skills that they automatically execute.

    public Dictionary<string, float> GetAttributesForLevel(int level)
    {
        var attributes = new Dictionary<string, float>();

        foreach(var attribute in BaseAttributes)
        {
            var attributeValue = GetValueForLevel(attribute.Value.x, attribute.Value.y, level, GameData.MaxEntityLevel);
            attributes.Add(attribute.Key, attributeValue);
        }

        return attributes;
    }

    public Dictionary<string, Vector2> GetDepletablesForLevel(int level)
    {
        var depletables = new Dictionary<string, Vector2>();

        foreach (var depletable in MaxDepletables)
        {
            var depletableValue = GetValueForLevel(depletable.Value.x, depletable.Value.y, level, GameData.MaxEntityLevel);
            depletables.Add(depletable.Key, new Vector2(depletableValue, depletableValue));
        }

        return depletables;
    }

    public float GetValueForLevel(float min, float max, int level, int maxLevel)
    {
        return Mathf.Lerp(min, max, level / maxLevel);
    }
}
