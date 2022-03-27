using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formulae
{
    // This class can be used to customise skill-related values such as damage and casting time based on entity attributes, payload flags and other variables. 
    #region Damage
    public static float OutgoingDamage(Entity caster, float rawDamage, PayloadData payloadData)
    {
        var outgoingDamage = rawDamage;

        return outgoingDamage;
    }

    public static float IncomingDamage(Entity caster, Entity target, float rawDamage, Payload payload, ref List<string> resultFlags)
    {
        var payloadData = payload.PayloadData;
        var flags = payloadData.Flags;
        var targetAttributes = payload.Action != null ? target.EntityAttributes(payload.Action.SkillID, payload.Action.ActionID, 
                               payload.StatusID, payload.PayloadData.Categories, payload.PayloadData.TargetAttributesIgnored) :
                               target.EntityAttributes();

        var isCrit = flags.Contains("canCrit") && payload.CasterAttributes["critChance"] >= Random.value;
        if (isCrit)
        {
            resultFlags.Add("critical");
        }
        var critMultiplier = isCrit ? (1.0f + payload.CasterAttributes["critDamage"]) : 1.0f;

        var isDamage = rawDamage > 0.0f;
        var defMultiplier = isDamage ? 1.0f - targetAttributes["def"] / (targetAttributes["def"] + 5 * caster.Level + 500.0f) : 1.0f;
        var incomingDamage = rawDamage * critMultiplier * defMultiplier;

        return incomingDamage;
    }
    #endregion

    public static float EntityBaseAttribute(Entity entity, string attribute)
    {
        var min = entity.EntityData.BaseAttributes[attribute].x;
        var max = entity.EntityData.BaseAttributes[attribute].y;
        var baseAttribute = Mathf.Lerp(min, max, (entity.Level) / 100.0f);

        return baseAttribute;
    }

    #region Resources
    public static float ResourceMaxValue(Entity entity, Dictionary<string, float> entityAttributes, string resource)
    {
        if (BattleData.EntityResources.ContainsKey(resource))
        {
            return BattleData.EntityResources[resource].GetValueFromAttributes(entity, entityAttributes);
        }

        Debug.LogError($"Resource {resource} not found in game data.");
        return 1.0f;
    }

    public static float ResourceStartValue(Entity entity, string resource)
    {
        var startRatio = 1.0f;
        var startValue = entity.ResourcesMax[resource] * startRatio;

        return startValue;
    }

    public static float ResourceRecoveryRate(Entity entity, Dictionary<string, float> entityAttributes, EntityData.EntityResource resource)
    {
        if (entity.EntityBattle.InCombat)
        {
            if (resource != null && resource.ChangePerSecondInCombat != null && resource.ChangePerSecondInCombat.Count > 0)
            {
                return resource.ChangePerSecondInCombat.IncomingValue(entity, entityAttributes);
            }
            else
            {
                return 0.0f;
            }
        }
        else
        {
            if (resource != null && resource.ChangePerSecondOutOfCombat != null && resource.ChangePerSecondOutOfCombat.Count > 0)
            {
                return resource.ChangePerSecondOutOfCombat.IncomingValue(entity, entityAttributes);
            }
            else
            {
                return 0.0f;
            }
        }
    }
    #endregion

    #region Action time and speed
    public static float ActionTime(Entity entity, float actionTimestamp)
    {
        var timeMultiplier = 1.0f;
        var actionTime = actionTimestamp * timeMultiplier;

        return actionTime;
    }

    public static float RequiredChargeTime(Entity entity, SkillChargeData skillChargeData)
    {
        var chargeTime = skillChargeData.RequiredChargeTime;

        return chargeTime;
    }

    public static float FullChargeTime(Entity entity, SkillChargeData skillChargeData)
    {
        var chargeTime = skillChargeData.FullChargeTime;

        return chargeTime;
    }

    public static float CooldownTime(Entity entity, string skillID, ActionCooldown action)
    {
        var cooldown = action.Cooldown;

        return cooldown;
    }

    public static float StatusDurationTime(Entity caster, Entity targer, StatusEffectData statusEffect)
    {
        var duration = statusEffect.Duration;

        return duration;
    }

    public static float AutoAttackInterval(Entity caster, Entity target)
    {
        // Depending on the auto attack settings target might be null.
        var interval = caster.EntityData.Skills.AutoAttackInterval;

        return interval;
    }

    public static float SkillDelay(Entity caster)
    {
        var delay = Random.Range(caster.EntityData.Skills.SkillDelayMin, caster.EntityData.Skills.SkillDelayMax);

        return delay;
    }
    #endregion

    public static float PayloadSuccessChance(PayloadData payloadData, Entity caster, Entity target)
    {
        var successChance = payloadData.SuccessChance;

        return successChance;
    }

    #region Movement
    public static float EntityMovementSpeed(Entity entity, bool running = false)
    {
        var speed = entity.EntityData.Movement.MovementSpeed;
        if (running)
        {
            speed *= entity.EntityData.Movement.MovementSpeedRunMultiplier;
        }

        return speed;
    }

    public static float EntityRotateSpeed(Entity entity)
    {
        var speed = entity.EntityData.Movement.RotateSpeed;

        return speed;
    }

    public static float EntityJumpHeight(Entity entity)
    {
        var height = entity.EntityData.Movement.JumpHeight;

        return height;
    }
    #endregion
}
