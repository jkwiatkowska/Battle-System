using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCooldownApplication : Action
{
    public float Cooldown;                      // After using a skill, it goes on cooldown and cannot be used again until this much time passes.
    public List<string> SharedCooldown;         // Skills affected by this cooldown.

    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();

        if (!ConditionMet(entity))
        {
            return;
        }

        var availableTime = BattleSystem.TimeSinceStart + Cooldown;

        foreach (var skill in SharedCooldown)
        {
            entity.SetSkillAvailableTime(skill, availableTime);
        }

        actionResult.Success = true;
    }

    public override bool NeedsTarget()
    {
        return false;
    }
}
