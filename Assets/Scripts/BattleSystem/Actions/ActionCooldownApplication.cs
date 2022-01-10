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

        foreach (var skill in SharedCooldown)
        {
            var availableTime = BattleSystem.TimeSinceStart + Formulae.CooldownTime(entity, skill, Cooldown);
            entity.SetSkillAvailableTime(skill, availableTime);
        }

        actionResult.Success = true;
    }

    public override bool NeedsTarget()
    {
        return false;
    }
}
