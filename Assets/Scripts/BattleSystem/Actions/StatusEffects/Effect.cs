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

    public abstract void Apply(string statusID, Entity target, Entity caster, Payload payload);
    public abstract void Remove(string statusID, Entity target, bool endStatus = false);
}

public class EffectAttributeChange : Effect
{
    public string Attribute;                            // Affected attribute.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.


    public ePayloadFilter PayloadTargetType;            // An attribute can be affected directly or only when specific skills and actions are used.
    public string PayloadTarget;                        // Name or ID of the above.

    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        var attributeChange = new AttributeChange
        {
            Attribute = Attribute,
            Key = Key(statusID),
            Value = Value.OutgoingValues(caster, caster.EntityAttributes(payload.Action.SkillID, 
                                        payload.Action.ActionID, statusID, payload.PayloadData.Categories), null),
            PayloadFilter = PayloadTargetType,
            Requirement = PayloadTarget
        };

        target.ApplyAttributeChange(attributeChange);
    }

    public override void Remove(string statusID, Entity target, bool endStatus = false)
    {
        target.RemoveAttributeChange(Attribute, Key(statusID));
    }

    public string Key(string statusID)
    {
        return statusID + StacksRequired.ToString() + PayloadTargetType + PayloadTarget;
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

public class EffectConvert : Effect
{
    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        target.Convert(caster.Faction);
    }

    public override void Remove(string statusID, Entity target, bool endStatus = false)
    {
        target.RemoveConversion();

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.RemoveStatusEffect(statusID);
        }
    }
}

public class EffectImmunity : Effect
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.

    public int Limit;                                   // If not zero, the effect will only work a set number of times.

    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        var immunity = new Immunity()
        {
            Data = this,
            StatusID = statusID,
            UsesLeft = Limit
        };

        target.ApplyImmunity(immunity, Key(statusID));
    }

    public override void Remove(string statusID, Entity target, bool endStatus = false)
    {
        target.RemoveImmunity(PayloadFilter, Key(statusID));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.RemoveStatusEffect(statusID);
        }
    }

    public string Key(string statusID)
    {
        return statusID + StacksRequired.ToString() + PayloadFilter + PayloadName;
    }
}

public class Immunity
{
    public EffectImmunity Data;
    public string StatusID;
    public int UsesLeft;

    public void Use(Entity entity)
    {
        if (UsesLeft > 0)
        {
            UsesLeft--;

            if (UsesLeft <= 0)
            {
                Data.Remove(StatusID, entity, true);
            }
        }
    }
}

public class EffectResistance : Effect
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.
    public float Resisted;                              // Amount of resisted damage. If 1 a skill or effect isn't applied at all. The amount stacks.

    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        target.ApplyResistance(this, Key(statusID));
    }

    public override void Remove(string statusID, Entity target, bool endStatus = false)
    {
        target.RemoveResistance(PayloadFilter, Key(statusID));

        if (endStatus & EndStatusOnEffectEnd)
        {
            target.RemoveStatusEffect(statusID);
        }
    }

    public string Key(string statusID)
    {
        return statusID + StacksRequired.ToString() + PayloadFilter + PayloadName;
    }
}
