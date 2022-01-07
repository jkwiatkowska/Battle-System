using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData
{
    public List<string> Affinities;                         // This doesn't do anything, but can be used in damage calculations.
    public Dictionary<string, Vector2> BaseAttributes;      // Attributes like atk, def, etc. Values at level 1 and max level. 
    public Dictionary<string, Vector2> MaxDepletables;      // Depletable values like hp, mp and stamina at level 1 and max level.
    public Dictionary<string, float> StartingDepletables;   // Starting values, value between 0 and 1.
    public Dictionary<string, Vector2> DepletableRecovery;  // Natural recovery of the depletables, in and out of combat.

    public bool IsTargetable;                               // If true, skills can be used on the entity.

    public string Faction;

    public bool IsAI;                                       // AI entities hold a list of skills that they automatically execute.

    public Dictionary<string, float> GetAttributesForLevel(int level)
    {
        var attributes = new Dictionary<string, float>();

        foreach(var attribute in BaseAttributes)
        {
            var value = GetValueForLevel(attribute.Value.x, attribute.Value.y, level, GameData.MaxEntityLevel);
            attributes.Add(attribute.Key, value);
        }

        return attributes;
    }

    public Dictionary<string, float> GetStartingDepletablesForLevel(int level)
    {
        var maxDepletables = GetMaxDepletablesForLevel(level);

        var startDepletables = new Dictionary<string, float>();

        foreach (var startDepletable in StartingDepletables)
        {
            var value = maxDepletables[startDepletable.Key] * startDepletable.Value;
            startDepletables.Add(startDepletable.Key, value);
        }

        return startDepletables;
    }

    public Dictionary<string, float> GetMaxDepletablesForLevel(int level)
    {
        var maxDepletables = new Dictionary<string, float>();

        foreach (var depletable in MaxDepletables)
        {
            var value = GetValueForLevel(depletable.Value.x, depletable.Value.y, level, GameData.MaxEntityLevel);
            maxDepletables.Add(depletable.Key, value);
        }

        return maxDepletables;
    }

    public float GetValueForLevel(float min, float max, int level, int maxLevel)
    {
        return Mathf.Lerp(min, max, level / maxLevel);
    }
}
