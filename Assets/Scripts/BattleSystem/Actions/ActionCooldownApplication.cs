using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCooldownApplication : Action
{
    public enum eCooldownTarget
    {
        Skill,
        SkillGroup
    }

    public float Cooldown;                      // After using a skill, it goes on cooldown and cannot be used again until this much time passes.
    public eCooldownTarget CooldownTarget;      // A cooldown can be applied to a singular skill or a group of skills.
    public string CooldownTargetName;           // Name of a skill or skill group.
    
    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, actionResults))
        {
            return;
        }

        switch (CooldownTarget)
        {
            case eCooldownTarget.Skill:
            {
                var availableTime = BattleSystem.Time + Formulae.CooldownTime(entity, CooldownTargetName, this);
                entity.SetSkillAvailableTime(CooldownTargetName, availableTime);
                break;
            }
            case eCooldownTarget.SkillGroup:
            {
                if (!GameData.SkillGroups.ContainsKey(CooldownTargetName))
                {
                    Debug.LogError($"Invalid skill group name: {CooldownTargetName} for action {ActionID}");
                    return;
                }

                foreach (var skill in GameData.SkillGroups[CooldownTargetName])
                {
                    var availableTime = BattleSystem.Time + Formulae.CooldownTime(entity, skill, this);
                    entity.SetSkillAvailableTime(skill, availableTime);
                }
                break;
            }
        }

        actionResults[ActionID].Success = true;
    }
}
