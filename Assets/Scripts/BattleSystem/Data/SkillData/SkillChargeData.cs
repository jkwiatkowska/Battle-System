using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillChargeData
{
    public float RequiredChargeTime;                // Charge time required for a skill to execute.
    public float FullChargeTime;                    // Full charge time can be different, for example if charge time is to affect skill potency.
    public bool MovementCancelsCharge;              // If false, skill can be charged while moving.
    public List<ActionData> PreChargeTimeline; // Mostly for animation/sound to execute before skill starts charging. 
    public bool ShowUI;                             // An indication of charge progress is displayed if true.
}
