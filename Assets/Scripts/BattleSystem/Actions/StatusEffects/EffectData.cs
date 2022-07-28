using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectData
{
    public enum eEffectType
    {
        AttributeChange,                                // Target's attributes are modified while the effect is active.
        Convert,                                        // Target's faction is temporarily overriden.
        //DamageResistance,                             // Resistance to damage from specific skills, effects and payload types.
        //DamageVulnerability,                          // Increased damage from specific skills, effects and payload types.
        Immunity,                                       // Full immunity to particular skills, payload types or effects.
        Lock,                                           // Prevents the target from using specific skills or moving.
        ResourceGuard,                                  // Prevents a resource from going below a specified amount.
        Shield,                                         // A resource can be substituted with another by a shield.
        //Taunt,                                        // Target can only attack the caster.
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
    public int StacksRequiredMin;                       // Effect is only applied if the current stack number is within this range.
    public int StacksRequiredMax;

    public bool EndStatusOnEffectEnd;                   // If an effect can run out before the status ends, it can cause it to end early.

    public abstract void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target);
    public abstract void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false);

    protected static string Key(string statusID, string casterUID, int effectIndex)
    {
        return statusID + effectIndex.ToString() + casterUID;
    }

    public static EffectData MakeNew(eEffectType type)
    {
        EffectData effect;

        switch(type)
        {
            case eEffectType.AttributeChange:
            {
                effect = new EffectAttributeChange();
                break;
            }
            case eEffectType.Convert:
            {
                effect = new EffectConvert();
                break;
            }
            case eEffectType.Immunity:
            {
                effect = new EffectImmunity();
                break;
            }
            case eEffectType.Lock:
            {
                effect = new EffectLock();
                break;
            }
            case eEffectType.ResourceGuard:
            {
                effect = new EffectResourceGuard();
                break;
            }
            case eEffectType.Shield:
            {
                effect = new EffectShield();
                break;
            }
            case eEffectType.Trigger:
            {
                effect = new EffectTrigger();
                break;
            }
            default:
            {
                effect = null;
                break;
            }
        }

        if (effect != null)
        {
            effect.EffectType = type;
            effect.StacksRequiredMin = 1;
            effect.StacksRequiredMax = 1;
            effect.EndStatusOnEffectEnd = true;
        }

        return effect;
    }
}

public class AppliedEffect<T> where T : EffectData
{
    public T Data;              // Effect data
    public EntityInfo Caster;   // Caster entity and stats at the time of applying the status effect
    public string StatusID;     // ID of the status effect that applied this effect.
    public string Key;          // Key in the effect dictionary

    public float CalculateValue(Value value, EntityInfo target)
    {
        var valueInfo = new ValueInfo(casterInfo: Caster, targetInfo: target, null);
        return value.CalculateValue(valueInfo);
    }
}

public class LimitedEffect<T> : AppliedEffect<T> where T : EffectData
{
    public int UsesLeft;
    public int EffectIndex;

    public virtual void Use(Entity entity)
    {
        if (UsesLeft > 0)
        {
            UsesLeft--;

            if (UsesLeft <= 0)
            {
                Data.Remove(StatusID, Caster.UID, EffectIndex, entity, true);
            }
        }
    }
}

#region Attribute Change
public class EffectAttributeChange : EffectData
{
    public string Attribute;                            // Affected attribute.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.

    public ePayloadFilter PayloadTargetType;            // An attribute can be affected directly or only when specific skills and actions are used.
    public string PayloadTarget;                        // Name or ID of the above.

    public EffectAttributeChange()
    {
        Value = new Value(false);
    }

    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        var attributeChange = new AttributeChange
        {
            Data = this,
            Caster = caster,
            StatusID = statusID,
            Key = Key(statusID, caster.UID, effectIndex),
            Attribute = Attribute,
            Value = Value,
            PayloadFilter = PayloadTargetType,
            Requirement = PayloadTarget
        };

        target.Entity.ApplyAttributeChange(attributeChange);
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveAttributeChange(Attribute, Key(statusID, casterUID, effectIndex));
    }
}
public class AttributeChange : AppliedEffect<EffectAttributeChange>
{
    public string Attribute;                            // Affected attribute.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.

    public EffectData.ePayloadFilter PayloadFilter;         // An attribute can be affected directly or only when specific skills and actions are used.
    public string Requirement;                          // Name or ID of the above.

    public float GetValue(EntityInfo target)
    {
        return CalculateValue(Value, target);
    }
}
#endregion

#region Convert
public class EffectConvert : EffectData
{
    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        target.Entity.Convert(caster.Entity.Faction);
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveConversion(casterUID);

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID, casterUID);
        }
    }
}
#endregion

#region Immunity
public class EffectImmunity : EffectData
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.

    public int Limit;                                   // If not zero, the effect will only work a set number of times.

    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        var immunity = new Immunity()
        {
            Data = this,
            StatusID = statusID,
            Caster = caster,
            EffectIndex = effectIndex,
            UsesLeft = Limit
        };

        target.Entity.ApplyImmunity(immunity, Key(statusID, caster.UID, effectIndex));
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveImmunity(PayloadFilter, Key(statusID, casterUID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID, casterUID);
        }
    }
}

public class Immunity : LimitedEffect<EffectImmunity>
{

}
#endregion

#region Lock
public class EffectLock : EffectData
{
    public enum eLockType
    {
        AutoAttack,
        SkillsAll,
        SkillsGroup,
        Skill,
        Movement,
        Jump
    }

    public eLockType LockType;
    public string Skill;        // Name of the skill or skill group locked.

    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        target.Entity.ApplyLock(this, Key(statusID, caster.UID, effectIndex));
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveLock(this, Key(statusID, casterUID, effectIndex));
    }
}

#endregion

#region Resistance
public class EffectResistance : EffectData
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.
    public float Resisted;                              // Amount of resisted damage. If 1 a skill or effect isn't applied at all. The amount stacks.

    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        target.Entity.ApplyResistance(this, Key(statusID, caster.UID, effectIndex));
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveResistance(PayloadFilter, Key(statusID, casterUID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.RemoveStatusEffect(statusID, casterUID);
        }
    }
}
#endregion

#region Resource Guard
public class EffectResourceGuard : EffectData
{
    public string Resource; // Guarded resource.

    public Value MinValue;  // If not null or empty, the resource cannot be lower than this value.
    public Value MaxValue;  // If not null or empty, the resource cannot be higher than this value.

    public int Limit;       // If not zero, the guard will disappear after it's used this number of times.

    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        var resourceGuard = new ResourceGuard()
        {
            Data = this,
            Caster = caster,
            MinValue = MinValue,
            MaxValue = MaxValue,
            UsesLeft = Limit,
            StatusID = statusID,
            EffectIndex = effectIndex
        };

        target.Entity.ApplyResourceGuard(resourceGuard, Key(statusID, caster.UID, effectIndex));
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveResourceGuard(Resource, Key(statusID, casterUID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.RemoveStatusEffect(statusID, casterUID);
        }
    }
}

public class ResourceGuard : LimitedEffect<EffectResourceGuard>
{
    public Value MinValue;
    public Value MaxValue;

    public bool Guard(EntityInfo entity, float resourceValue, out float resourceGuarded)
    {
        resourceGuarded = resourceValue;

        if (MinValue != null && MinValue.Components.Count > 0)
        {
            var min = CalculateValue(MinValue, entity);
            if (resourceValue < min)
            {
                resourceGuarded = min;
                return true;
            }
        }

        if (MaxValue != null && MaxValue.Components.Count > 0)
        {
            var max = CalculateValue(MaxValue, entity);

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
public class EffectShield : EffectData
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

    public EffectShield()
    {
        ShieldResourceToGrant = new Value(false);
        CategoryMultipliers = new Dictionary<string, float>();
    }
    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        var shield = new Shield()
        {
            Data = this,
            Caster = caster,
            EffectIndex = effectIndex,
            StatusID = statusID,
            UsesLeft = Limit,
        };
        shield.AbsorbtionLeft = MaxDamageAbsorbed != null ? shield.CalculateValue(MaxDamageAbsorbed, target) : 0.0f;

        var shieldResourceGranted = shield.CalculateValue(ShieldResourceToGrant, target);

        target.Entity.ApplyShield(shield, Key(statusID, caster.UID, effectIndex), shieldResourceGranted);
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveShield(this, Key(statusID, casterUID, effectIndex));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.DelayedStatusEffectRemoval(statusID, casterUID);
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
                Data.Remove(StatusID, Caster.UID, EffectIndex, entity, endStatus:true);
                return;
            }
        }

        base.Use(entity);
    }

    public void RemoveShield(Entity entity)
    {
        Data.Remove(StatusID, Caster.UID, EffectIndex, entity, endStatus:true);
    }
}
#endregion

#region Trigger
public class EffectTrigger : EffectData
{
    public TriggerData TriggerData;

    public EffectTrigger()
    {
        TriggerData = new TriggerData();
    }
    public override void Apply(string statusID, int effectIndex, EntityInfo caster, EntityInfo target)
    {
        target.Entity.AddTrigger(new Trigger(TriggerData), Key(statusID, caster.UID, effectIndex));
    }

    public override void Remove(string statusID, string casterUID, int effectIndex, Entity target, bool endStatus = false)
    {
        target.RemoveTrigger(TriggerData, Key(statusID, casterUID, effectIndex));
    }
}

#endregion