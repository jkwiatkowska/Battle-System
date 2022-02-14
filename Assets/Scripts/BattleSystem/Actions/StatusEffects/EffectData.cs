using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EffectData
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
        Effect,
        EffectGroup,
        Skill,
        SkillGroup,
    }

    public eEffectType EffectType;
    public Vector2Int StacksRequired;                   // Effect is only applied if the current stack number is within this range.

    public abstract void Apply(string statusID, Entity target, Entity caster, Payload payload);
    public abstract void Remove(string statusID);
}

public class AttributeChangeData : EffectData
{
    public string Attribute;                            // Affected attribute.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.


    public ePayloadFilter PayloadTargetType;            // An attribute can be affected directly or only when specific skills and actions are used.
    public string PayloadTarget;                        // Name or ID of the above.

    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        var effect = new AttributeChange
        {
            Attribute = Attribute,
            Value = Value.OutgoingValues(caster, caster.EntityAttributes(payload.Action.SkillID, 
                                        payload.Action.ActionID, payload.PayloadData.Categories), null),
            PayloadFilter = PayloadTargetType,
            Requirement = PayloadTarget
        };

    }

    public override void Remove(string statusID)
    {
        throw new System.NotImplementedException();
    }
}

public class AttributeChange
{
    public string Attribute;                            // Affected attribute.
    public Value Value;                                 // Increase/decrease to the attribute. Entity that applies the status is the caster, while the entity that receives it is the target.

    public EffectData.ePayloadFilter PayloadFilter;   // An attribute can be affected directly or only when specific skills and actions are used.
    public string Requirement;                          // Name or ID of the above.
}

public class ImmunityData : EffectData
{
    public ePayloadFilter PayloadFilter;                // Resistance can be to all damage or to a specific type of skills or actions.
    public string PayloadName;                          // Name or ID of the above.

    public override void Apply(string statusID, Entity target, Entity caster, Payload payload)
    {
        throw new System.NotImplementedException();
    }

    public override void Remove(string statusID)
    {
        throw new System.NotImplementedException();
    }
}
