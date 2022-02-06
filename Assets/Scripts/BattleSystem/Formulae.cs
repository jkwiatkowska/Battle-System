using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formulae
{
    // This class can be used to customise skill-related values such as damage and casting time based on entity attributes, payload flags and other variables. 
    public static float OutgoingDamage(Entity caster, float rawDamage, PayloadData payloadData)
    {
        var outgoingDamage = rawDamage;

        return outgoingDamage;
    }

    public static float IncomingDamage(Entity caster, Entity target, float rawDamage, PayloadData payloadData, ref List<string> resultFlags)
    {
        var flags = payloadData.Flags;

        var isCrit = flags.ContainsKey("canCrit") && flags["canCrit"] && caster.BaseAttributes["critChance"] >= Random.value;
        if (isCrit)
        {
            resultFlags.Add("critical");
        }
        var critMultiplier = isCrit ? (1.0f + caster.BaseAttributes["critDamage"]) : 1.0f;

        var defMultiplier = flags.ContainsKey("ignoreDef") && flags["ignoreDef"] ? 1.0f : 1.0f - target.BaseAttributes["def"] / (target.BaseAttributes["def"] + 5 * caster.Level + 500.0f);
        var incomingDamage = rawDamage * critMultiplier * defMultiplier;

        return incomingDamage;
    }

    public static float EntityBaseAttribute(Entity entity, string attribute)
    {
        var min = entity.EntityData.BaseAttributes[attribute].x;
        var max = entity.EntityData.BaseAttributes[attribute].y;
        var baseAttribute = Mathf.Lerp(min, max, (entity.Level) / 100.0f);

        return baseAttribute;
    }

    public static float DepletableMaxValue(Entity entity, string depletable)
    {
        var maxValue = 1.0f;
        if (entity.BaseAttributes.ContainsKey(depletable))
        {
            maxValue = entity.BaseAttributes[depletable];
        }

        return maxValue;
    }

    public static float DepletableStartValue(Entity entity, string depletable)
    {
        var startRatio = 1.0f;
        var startValue = DepletableMaxValue(entity, depletable) * startRatio;

        return startValue;
    }

    public static float DepletableRecoveryRate(Entity entity, string depletable)
    {
        var recoveryRate = 0.2f;

        if (entity.IsInCombat())
        {
            recoveryRate = 0.1f;
        }

        return recoveryRate;
    }

    public static float ActionTime(Entity entity, Action action)
    {
        var timeMultiplier = 1.0f;
        var actionTime = action.Timestamp * timeMultiplier;

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

    public static float CooldownTime(Entity entity, string skillID, ActionCooldownApplication action)
    {
        var cooldown = action.Cooldown;

        return cooldown;
    }

    public static float PayloadSuccessChance(PayloadData payloadData, Entity caster, Entity target)
    {
        var successChance = payloadData.SuccessChance;

        return successChance;
    }

    public static float EntityMovementSpeed(Entity entity)
    {
        var speed = entity.EntityData.MovementSpeed;

        return speed;
    }

    public static float EntityRotateSpeed(Entity entity)
    {
        var speed = entity.EntityData.RotateSpeed;

        return speed;
    }

    public static float EntityJumpHeight(Entity entity)
    {
        var height = entity.EntityData.JumpHeight;

        return height;
    }
}
