using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillChargeData
{
    public float RequiredChargeTime;                // Charge time required for a skill to execute.
    public float FullChargeTime;                    // Full charge time can be different, for example if charge time is to affect skill potency.
    public bool MovementCancelsCharge;              // If false, skill can be charged while moving.
    public ActionTimeline PreChargeTimeline;        // Mostly for animation/sound to execute before skill starts charging. 
    public bool ShowUI;                             // An indication of charge progress is displayed if true.

    public SkillChargeData()
    {
        RequiredChargeTime = 1.0f;
        FullChargeTime = 1.0f;
        MovementCancelsCharge = true;
        PreChargeTimeline = new ActionTimeline();
        ShowUI = true;
    }

    public float RequiredChargeTimeForEntity(Entity entity)
    {
        return Formulae.RequiredChargeTime(entity, this);
    }

    public float FullChargeTimeForEntity(Entity entity)
    {
        return Formulae.FullChargeTime(entity, this);
    }
}
