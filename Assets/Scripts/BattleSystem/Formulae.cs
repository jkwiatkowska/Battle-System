using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formulae
{
    public static float OutgoingDamage(Entity caster, float rawDamage, PayloadData payloadData)
    {
        // Caster attributes and payload may affect the outgoing damage
        var outgoingDamage = rawDamage;

        return outgoingDamage;
    }

    public static float IncomingDamage(Entity caster, Entity target, float rawDamage, PayloadData payloadData)
    {
        // Caster and target attributes as well as payload flags may affect the damage it receives.
        var flags = payloadData.Flags;

        var isCrit = flags["canCrit"] && caster.Attributes["critChance"] >= Random.value;
        var critMultiplier = isCrit ? (1.0f + caster.Attributes["critDamage"]) : 1.0f;

        var defMultiplier = flags["ignoreDef"] ? 1.0f : 1.0f - target.Attributes["def"] / (target.Attributes["def"] + 5 * caster.EntityLevel + 500.0f);
        var incomingDamage = rawDamage * critMultiplier * defMultiplier;

        return incomingDamage;
    }

    public static float CooldownTime(Entity entity, string skillID, float cooldownTime)
    {
        // Entity attributes can affect the skill cooldown time.
        var cooldown = cooldownTime;

        return cooldown;
    }

    public static float PayloadSuccessChance(PayloadData payloadData, Entity caster, Entity target)
    {
        // Entity attributes can affect the chance of a payload being successfully applied.
        var successChance = payloadData.SuccessChance;

        return successChance;
    }
}
