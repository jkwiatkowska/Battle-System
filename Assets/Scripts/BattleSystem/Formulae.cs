using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formulae
{
    // This class can be used to customise skill-related values such as damage and casting time based on entity attributes, payload flags and other variables. 
    #region Damage
    public static float IncomingDamage(Payload payloadInfo, float rawDamage)
    {
        var defMultiplier = 1.0f - payloadInfo.Target.Attribute("def") / payloadInfo.Target.Attribute("def") + 5 * payloadInfo.Caster.Level + 500.0f;
        var incomingDamage = rawDamage * defMultiplier;

        return incomingDamage;
    }

    public static float IncomingRecovery(Payload payloadInfo, float recoveryAmount)
    {
        var incomingRecovery = recoveryAmount;

        return recoveryAmount;
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
    public static float ResourceMaxValue(EntityInfo entity, string resource)
    {
        if (BattleData.EntityResources.ContainsKey(resource))
        {
            var valueInfo = new ValueInfo(entity, null, null);
            return BattleData.EntityResources[resource].CalculateValue(valueInfo);
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

    public static float ResourceRecoveryRate(EntityInfo entity, EntityData.EntityResource resource)
    {
        if (entity.Entity.EntityBattle.InCombat)
        {
            if (resource != null && resource.ChangePerSecondInCombat != null && resource.ChangePerSecondInCombat.Components.Count > 0)
            {
                var valueInfo = new ValueInfo(entity, null, null);
                return resource.ChangePerSecondInCombat.CalculateValue(valueInfo);
            }
            else
            {
                return 0.0f;
            }
        }
        else
        {
            if (resource != null && resource.ChangePerSecondOutOfCombat != null && resource.ChangePerSecondOutOfCombat.Components.Count > 0)
            {
                var valueInfo = new ValueInfo(entity, null, null);
                return resource.ChangePerSecondOutOfCombat.CalculateValue(valueInfo);
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

    public static float StatusDurationTime(EntityInfo caster, EntityInfo targer, StatusEffectData statusEffect)
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

    public static float PayloadSuccessChance(ActionPayload action, Payload payload)
    {
        var valueInfo = new ValueInfo(payload);
        var successChance = action.SuccessChance.CalculateValue(valueInfo);

        return successChance;
    }

    #region Movement

    public static float EntityInterruptResistance(Entity entity)
    {
        var resistance = entity.EntityData.InterruptResistance;

        foreach (var multiplier in BattleData.Multipliers.InterruptResistanceMultipliers)
        {
            multiplier.ApplyMultiplier(entity, ref resistance);
        }

        return resistance;
    }

    public static float EntityMovementSpeed(Entity entity, bool running = false)
    {
        var speed = entity.EntityData.Movement.MovementSpeed;
        if (running)
        {
            speed *= entity.EntityData.Movement.MovementSpeedRunMultiplier;
        }

        foreach (var multiplier in BattleData.Multipliers.MovementSpeedMultipliers)
        {
            multiplier.ApplyMultiplier(entity, ref speed);
        }

        return speed;
    }

    public static float EntityRotateSpeed(Entity entity)
    {
        var speed = entity.EntityData.Movement.RotateSpeed;
        foreach (var multiplier in BattleData.Multipliers.RotationSpeedMultipliers)
        {
            multiplier.ApplyMultiplier(entity, ref speed);
        }

        return speed;
    }

    public static float EntityJumpHeight(Entity entity)
    {
        var height = entity.EntityData.Movement.JumpHeight;

        foreach (var multiplier in BattleData.Multipliers.JumpHeightMultipliers)
        {
            multiplier.ApplyMultiplier(entity, ref height);
        }

        return height;
    }
    #endregion
}
