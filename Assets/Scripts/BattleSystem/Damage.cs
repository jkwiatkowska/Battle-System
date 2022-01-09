using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Damage
{
    public static float GetOutgoingDamage(Entity caster, float rawDamage)
    {
        var outgoingDamage = rawDamage;

        return outgoingDamage;
    }

    public static float GetIncomingDamage(Entity target, float rawDamage)
    {
        var incomingDamage = rawDamage;

        return incomingDamage;
    }
}
