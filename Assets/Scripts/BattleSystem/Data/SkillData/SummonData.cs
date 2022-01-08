using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonData : ActionData
{
    public string EntityID;

    public PositionData SummonAtPosition;

    public float SummonDuration;                        // 0 if infinite
    public Dictionary<string, float> SharedAttributes;  // Summoned entity can inherit the caster's attributes
                                                        // The float value is a multiplier
                                                        // (for example an entry of {atk, 0.5} means the entity has half of the caster's atk attribute)

    public bool LifeLink;                               // If true, the entity will disappear when the caster dies

    public override bool NeedsTarget()
    {
        return SummonAtPosition.PositionOrigin != PositionData.ePositionOrigin.SelectedTargetPosition;
    }
}
