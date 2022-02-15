using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Effect
{
    public enum eEffectType
    {
        AttributeChange,                                // Target's attributes are modified while the effect is active.
        Convert,                                        // Target's faction is temporarily overriden.
        Immunity,                                       // Resistance to specific payload types.
        Lock,                                           // Prevents the target from using specific skills or moving.                     
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

    public abstract void Apply(string statusID, Entity target, Entity caster, Payload payload);
    public abstract void Remove(string statusID, Entity target);
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

    public override void Remove(string statusID, Entity target)
    {
        target.RemoveAttributeChange(Attribute, Key(statusID));
    }

    public string Key(string statusID)
    {
        return statusID + StacksRequired.ToString() + PayloadTargetType + PayloadTarget;
    }
}

public class EffectConvert : Effect
{
    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        target.Convert(caster.Faction);
    }

    public override void Remove(string statusID, Entity target)
    {
        target.RemoveConversion();
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

public class EffectImmunity : Effect
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.

    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        target.ApplyImmunity(this, Key(statusID));
    }

    public override void Remove(string statusID, Entity target)
    {
        target.RemoveImmunity(PayloadFilter, Key(statusID));
    }

    public string Key(string statusID)
    {
        return statusID + StacksRequired.ToString() + PayloadFilter + PayloadName;
    }
}
