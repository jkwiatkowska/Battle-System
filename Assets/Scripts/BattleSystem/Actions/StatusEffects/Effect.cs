using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public enum eEffectType
    {
        AttributeChange,                                // Target's attributes are modified while the effect is active.
        Convert,                                        // Target's faction is temporarily overriden.
        DamageResistance,                               // Resistance to damage from specific skills, effects and payload types.
        DamageVulnerability,                            // Increased damage from specific skills, effects and payload types.
        Immunity,                                       // Full immunity to particular skills, payload types or effects.
        Lock,                                           // Prevents the target from using specific skills or moving.
        ResourceGuard,                                  // Prevents a resource from going below a specified amount.
        Shield,                                         // A resource can be substituted with another by a shield.
        Trigger,                                        // New triggers are added to an entity for the effect's duration.
    }

    public enum ePayloadFilter                          // Used by some status effects to specify which kinds of payloads are affected.
    {
        All,
        Action,
        Category,
        Skill,
        SkillGroup,
        Status,
        StatusGroup
    }

    public eEffectType EffectType;
    public Vector2Int StacksRequired;                   // Effect is only applied if the current stack number is within this range.

    public bool EndStatusOnEffectEnd;                   // If an effect can run out before the status ends, it can cause it to end early.

    public abstract void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload);
    public abstract void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false);

    protected string Key(string statusID, int effectIndex)
    {
        return statusID + effectIndex.ToString();
    }
}

public class LimitedEffect<T> where T:Effect
{
    public T Data;
    public string StatusID;
    public int EffectIndex;
    public int UsesLeft;

    public virtual void Use(Entity entity)
    {
        if (UsesLeft > 0)
        {
            UsesLeft--;

            if (UsesLeft <= 0)
            {
                Data.Remove(StatusID, EffectIndex, entity, true);
            }
        }
    }
}

#region Attribute Change
public class EffectAttributeChange : Effect
{
    public string Attribute;                            // Affected attribute.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.


    public ePayloadFilter PayloadTargetType;            // An attribute can be affected directly or only when specific skills and actions are used.
    public string PayloadTarget;                        // Name or ID of the above.

    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        var attributeChange = new AttributeChange
        {
            Attribute = Attribute,
            Key = Key(statusID, effectIndex),
            Value = Value.OutgoingValues(caster, caster.EntityAttributes(payload.Action.SkillID, 
                                        payload.Action.ActionID, statusID, payload.PayloadData.Categories), null),
            PayloadFilter = PayloadTargetType,
            Requirement = PayloadTarget
        };

        target.ApplyAttributeChange(attributeChange);
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveAttributeChange(Attribute, Key(statusID, effectIndex));
    }
}
public class AttributeChange
{
    public string Attribute;                            // Affected attribute.
    public string Key;                                  // Used to identify the attribute change.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.

    public Effect.ePayloadFilter PayloadFilter;         // An attribute can be affected directly or only when specific skills and actions are used.
    public string Requirement;                          // Name or ID of the above.
}
#endregion

#region Convert
public class EffectConvert : Effect
{
    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        target.Convert(caster.Faction);
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveConversion();

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID);
        }
    }
}
#endregion

#region Immunity
public class EffectImmunity : Effect
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.

    public int Limit;                                   // If not zero, the effect will only work a set number of times.

    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        var immunity = new Immunity()
        {
            Data = this,
            StatusID = statusID,
            UsesLeft = Limit
        };

        target.ApplyImmunity(immunity, Key(statusID, effectIndex));
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveImmunity(PayloadFilter, Key(statusID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID);
        }
    }
}

public class Immunity : LimitedEffect<EffectImmunity>
{

}
#endregion

#region Lock
public class EffectLock : Effect
{
    public enum eLockType
    {
        SkillsAll,
        SkillsGroup,
        Skill,
        Movement,
        Jump
    }

    public eLockType LockType;
    public string Skill;        // Name of the skill or skill group locked.

    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        target.ApplyLock(this, Key(statusID, effectIndex));
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveLock(this, Key(statusID, effectIndex));
    }
}

#endregion

#region Resistance
public class EffectResistance : Effect
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.
    public float Resisted;                              // Amount of resisted damage. If 1 a skill or effect isn't applied at all. The amount stacks.

    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        target.ApplyResistance(this, Key(statusID, effectIndex));
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveResistance(PayloadFilter, Key(statusID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.RemoveStatusEffect(statusID);
        }
    }
}
#endregion

#region Resource Guard
public class EffectResourceGuard : Effect
{
    public string Resource; // Guarded resource.

    public Value MinValue;  // If not null or empty, the resource cannot be lower than this value.
    public Value MaxValue;  // If not null or empty, the resource cannot be higher than this value.

    public int Limit;       // If not zero, the guard will disappear after it's used this number of times.

    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        var casterAttributes = caster.EntityAttributes(payload.Action.SkillID,
                               payload.Action.ActionID, statusID, payload.PayloadData.Categories);

        var resourceGuard = new ResourceGuard()
        {
            Data = this,
            MinValue = MinValue.OutgoingValues(caster, casterAttributes, null),
            MaxValue = MaxValue.OutgoingValues(caster, casterAttributes, null),
            UsesLeft = Limit,
            StatusID = statusID,
            EffectIndex = effectIndex
        };

        target.ApplyResourceGuard(resourceGuard, Key(statusID, effectIndex));
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveResourceGuard(Resource, Key(statusID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID);
        }
    }
}

public class ResourceGuard : LimitedEffect<EffectResourceGuard>
{
    public Value MinValue;
    public Value MaxValue;

    public bool Guard(Entity entity, float resourceValue, out float resourceGuarded)
    {
        resourceGuarded = resourceValue;

        if (MinValue != null && MinValue.Count > 0)
        {
            var min = MinValue.IncomingValue(entity);
            if (resourceValue < min)
            {
                resourceGuarded = min;
                return true;
            }
        }

        if (MaxValue != null && MaxValue.Count > 0)
        {
            var max = MaxValue.IncomingValue(entity);

            if (resourceValue > max)
            {
                resourceGuarded = max;
                return true;
            }
        }

        return false;
    }
}

#endregion

#region Shield
public class EffectShield : Effect
{
    public string ShieldResource;                           // Damage is funneled into this resource.
    public string ShieldedResource;                         // This resource does not take damage while a shield is active.

    public Value ShieldResourceToGrant;                     // When the effect is granted, this much of the shield resource is granted to the entity.   
    public Value MaxDamageAbsorbed;                         // Limit to how much damage the shield can absorb. 0 for no limit.
    public bool SetMaxShieldResource;                       // If the shield resource is unique to this shield, it needs a max value set for any UI display. 

    public float DamageMultiplier;                          // The damage to the shield resource can be lowered (less than 1) or increased (more than 1) through this.
    public Dictionary<string, float> CategoryMultipliers;   // The multiplier can be different for specified categories.

    public int Priority;                                    // If multiple shields are protecting the same resource, the shield with the higher priority will take damage.

    public int Limit;                                       // How many times a shield can be struck before being removed. 0 for no limit. 
    public bool RemoveShieldResourceOnEffectEnd;            // If true, all of the resource used to shield will be drained when the effect ends.

    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        var casterAttributes = caster.EntityAttributes(payload.Action.SkillID,
                               payload.Action.ActionID, statusID, payload.PayloadData.Categories);
        var shield = new Shield()
        {
            Data = this,
            EffectIndex = effectIndex,
            StatusID = statusID,
            UsesLeft = Limit,
            AbsorbtionLeft = MaxDamageAbsorbed.OutgoingValues(caster, casterAttributes, null).IncomingValue(target),
        };

        var shieldResourceGranted = ShieldResourceToGrant.OutgoingValues(caster, casterAttributes, null).IncomingValue(target);

        target.ApplyShield(shield, Key(statusID, effectIndex), shieldResourceGranted);
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveShield(this, Key(statusID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID);
        }
    }
}

public class Shield : LimitedEffect<EffectShield>
{
    public float AbsorbtionLeft;

    public void UseShield(Entity entity, float damageAbsorbed)
    {
        if (AbsorbtionLeft > Constants.Epsilon)
        {
            AbsorbtionLeft -= damageAbsorbed;

            if (AbsorbtionLeft <= -Constants.Epsilon)
            {
                Data.Remove(StatusID, EffectIndex, entity, true);
                return;
            }
        }

        base.Use(entity);
    }

    public void RemoveShield(Entity entity)
    {
        Data.Remove(StatusID, EffectIndex, entity, true);
    }
}
#endregion

#region Trigger
public class EffectTrigger : Effect
{
    public TriggerData TriggerData;
    public override void Apply(string statusID, int effectIndex, Entity target, Entity caster, Payload payload)
    {
        target.AddTrigger(new Trigger(TriggerData), Key(statusID, effectIndex));
    }

    public override void Remove(string statusID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveTrigger(TriggerData, Key(statusID, effectIndex));
    }
}

#endregion