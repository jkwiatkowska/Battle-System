using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCooldown : Action
{
    public enum eChangeMode
    {
        Set,
        Modify
    }

    public enum eCooldownTarget
    {
        Skill,
        SkillGroup
    }

    public float Cooldown;                      // After using a skill, it goes on cooldown and cannot be used again until this much time passes.
    public eChangeMode ChangeMode;              // A skill cooldown can be set or modified.
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
                if (ChangeMode == eChangeMode.Modify)
                {
                    entity.EntityBattle.ModifySkillAvailableTime(CooldownTargetName, Cooldown);
                }
                else
                {
                    var availableTime = BattleSystem.Time + Formulae.CooldownTime(entity, CooldownTargetName, this);
                    entity.EntityBattle.SetSkillAvailableTime(CooldownTargetName, availableTime);
                }

                break;
            }
            case eCooldownTarget.SkillGroup:
            {
                if (!BattleData.SkillGroups.ContainsKey(CooldownTargetName))
                {
                    Debug.LogError($"Invalid skill group name: {CooldownTargetName} for action {ActionID}");
                    return;
                }

                foreach (var skill in BattleData.SkillGroups[CooldownTargetName])
                {
                    if (ChangeMode == eChangeMode.Modify)
                    {
                        entity.EntityBattle.ModifySkillAvailableTime(skill, Cooldown);
                    }
                    else
                    {
                        var availableTime = BattleSystem.Time + Formulae.CooldownTime(entity, skill, this);
                        entity.EntityBattle.SetSkillAvailableTime(skill, availableTime);
                    }
                }
                break;
            }
        }

        actionResults[ActionID].Success = true;
    }

    public override void SetTypeDefaults()
    {
        Cooldown = 0.5f;
        ChangeMode = eChangeMode.Set;
        CooldownTarget = eCooldownTarget.Skill;
        CooldownTargetName = SkillID;
    }
}
