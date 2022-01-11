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

    public static float IncomingDamage(Entity caster, Entity target, float rawDamage, PayloadData payloadData)
    {
        var flags = payloadData.Flags;

        var isCrit = flags["canCrit"] && caster.Attributes["critChance"] >= Random.value;
        var critMultiplier = isCrit ? (1.0f + caster.Attributes["critDamage"]) : 1.0f;

        var defMultiplier = flags["ignoreDef"] ? 1.0f : 1.0f - target.Attributes["def"] / (target.Attributes["def"] + 5 * caster.Level + 500.0f);
        var incomingDamage = rawDamage * critMultiplier * defMultiplier;

        return incomingDamage;
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
}
