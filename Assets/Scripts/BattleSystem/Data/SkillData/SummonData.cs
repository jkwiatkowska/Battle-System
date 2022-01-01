using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonData : SkillActionData
{
    public enum eSummonPosition
    {
        CasterPosition,
        TargetPosition
    }

    public string EntityID;
    public eSummonPosition SummonAtPosition;            // Summon can appear near caster or target
    public Vector2 PositionOffset;                      // Position offset from caster/target
    public Vector2 RandomPositionOffset;                // Range of a random offset from the summon position
    public float SummonDuration;                        // 0 if infinite
    public Dictionary<string, float> SharedAttributes;  // Summoned entity can inherit the caster's attributes
                                                        // The float value is a multiplier
                                                        // (for example an entry of {atk, 0.5} means the entity has half of the caster's atk attribute)

    public bool LifeLink;                               // If true, the entity will disappear when the caster dies

    public override bool NeedsTarget()
    {
        return SummonAtPosition != eSummonPosition.TargetPosition;
    }
}
