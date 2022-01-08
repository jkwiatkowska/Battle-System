using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCooldownApplication : Action
{
    public float Cooldown;                      // After using a skill, it goes on cooldown and cannot be used again until this much time passes.
    public List<string> SharedCooldown;         // Skills that will go on cooldown when this skill is used.
    public override bool NeedsTarget()
    {
        return false;
    }
}
