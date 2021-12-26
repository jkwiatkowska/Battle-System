using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonData : SkillActionData
{
    public string EntityID;
    public float SummonDuration;                        // 0 if infinite
    public Dictionary<string, float> SharedAttributes;  // Summoned entity can inherit the caster's attributes
                                                        // The float value is a multiplier
                                                        // (for example an entry of {atk, 0.5} means the entity has half of the caster's atk attribute)
}
