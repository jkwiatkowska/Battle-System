using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A payload is made up of multiple components. Components can be applied to entities with various effects. 
public abstract class PayloadComponent
{
    public enum eComponentTarget
    {
        ResourceChange, // Change to an entity resource (damage, recovery or setting a value).
        StateChange,    // Simple state changes and interruptions.
        StatusEffect,   // Application or removal of a status effect or status effect stacks.
        Tag,            // Entities can be track of and targeted by payloads without being specifically selected by. 
        TransformChange,// Change to a target's position and/or rotation, it can be immediate or happen over time. 
    }

    public eComponentTarget ComponentTarget;    // What the component affects.
    public List<string> Flags;                  // Flags to customise the payload.

    public PayloadComponent(eComponentTarget target)
    {
        ComponentTarget = target;
        Flags = new List<string>();
    }

    public abstract PayloadComponentResult ApplyComponent(Payload payload);

    protected void SetTriggers(Payload payloadInfo, PayloadComponentResult result)
    {
        payloadInfo.Caster.Entity.OnPayloadComponentApplied(result);
        payloadInfo.Target.Entity.OnPayloadComponentReceived(result);
    }
}

#region Resource
public class PayloadResourceChange : PayloadComponent
{
    public enum eChangeType
    {
        Damage,     // Subtraction
        Recovery,   // Addition
        Set         // Resource is set to the evaluated value
    }

    public eChangeType ChangeType;

    public Value Value;                                         // The value is used when a change is applied and can be evaluated based on caster and target attributes.
    public string ResourceAffected;                             // Name of the entity resource affected by the change, such as hp, mp or energy.

    public Dictionary<string, float> EntityCategoryMult;        // Effectiveness of the damage/recovery against given entity categories.

    public Dictionary<string, float> CasterAttributeOverride;   // Caster attributes such as damage boosting multipliers can be overriden (0 to ignore).
    public Dictionary<string, float> TargetAttributeOverride;   // Target attributes such as defense can be overriden (0 to ignore).

    public bool IgnoreShield;                                   // True damage.

    public AggroData.AggroChange Aggro;                         // When applying payload, aggro can be generated. Entities can be set to select their targets based on aggro.
    public bool MultiplyAggroByPayloadValue;                    // If true, aggro will be multiplied by damage dealt.

    public bool DisplayPopupText = true;                        // If true, a popup text will appear, showing the calculated value. 

    public PayloadResourceChange() : base(eComponentTarget.ResourceChange)
    {
        Value = new Value(false);
        EntityCategoryMult = new Dictionary<string, float>();
        CasterAttributeOverride = new Dictionary<string, float>();
        TargetAttributeOverride = new Dictionary<string, float>();
    }

    public override PayloadComponentResult ApplyComponent(Payload payload)
    {
        // Result
        var result = new PayloadComponentResult(payload, this);

        // Ignored attributes
        payload.Caster.AttributeOverride = CasterAttributeOverride;
        payload.Target.AttributeOverride = TargetAttributeOverride;

        // Value
        var valueInfo = new ValueInfo(payload);
        var value = Value.CalculateValue(valueInfo);

        var target = payload.Target.Entity;
        var targetData = target.EntityData;

        if ((ChangeType == eChangeType.Damage || ChangeType == eChangeType.Recovery) && (value < Constants.Epsilon && value > -Constants.Epsilon))
        {
            return result; // No change. 
        }

        // Category multiplier
        var categoryMultiplier = 1.0f;
        if (EntityCategoryMult != null && targetData.Categories != null && targetData.Categories.Count > 0)
        {
            foreach (var cat in EntityCategoryMult)
            {
                if (targetData.Categories.Contains(cat.Key))
                {
                    categoryMultiplier *= cat.Value;
                }
            }
        }

        value *= categoryMultiplier;

        switch (ChangeType)
        {
            case eChangeType.Damage:
            {
                // Attribute multipliers
                var attributeMultiplier = BattleData.Multipliers.DamageMultipliers.GetMultiplier(payload, Flags, result.ResultFlags);
                value *= attributeMultiplier;

                // Keep track of value
                result.ResultValue = value;

                // Apply damage
                target.ApplyDamage(ResourceAffected, value, this, result);

                break;
            }
            case eChangeType.Recovery:
            {
                // Attribute multipliers
                var attributeMultiplier = BattleData.Multipliers.RecoveryMultipliers.GetMultiplier(payload, Flags, result.ResultFlags);
                value *= attributeMultiplier;

                // Keep track of value
                result.ResultValue = value;

                // Apply recovery
                target.ApplyRecovery(ResourceAffected, value, this, result);
                break;
            }
            case eChangeType.Set:
            {
                // Keep track of value
                result.ResultValue = value;

                // Set resource
                target.SetResource(ResourceAffected, value, this, result);
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented payload resource change type {ChangeType}");
                break;
            }
        }

        // Aggro
        if (Aggro != null && payload.Caster.Entity != null && payload.Caster.Entity.Alive)
        {
            var mult = MultiplyAggroByPayloadValue ? -value : 1.0f;
            target.EntityBattle.ChangeAggro(payload.Caster.Entity, Aggro.GetAggroChange(valueInfo, mult));
        }

        // Reset ignored attributes
        payload.Caster.AttributeOverride = null;
        payload.Target.AttributeOverride = null;

        // Triggers
        SetTriggers(payload, result);

        return result;
    }
}
#endregion

#region State
public class PayloadStateChange : PayloadComponent
{
    public enum eStateChange
    {
        Instakill,      // Kills a target without affecting its resources.
        Interrupt,      // If the target is casting an interruptible skill, the action is cancelled.
        Revive,         // Revives a dead target.
        CancelMovement, // Stop movement caused by a transform payload component.
        CancelRotation, // Stop rotation caused by a transform payload component.
        CustomTrigger,  // Sets off a custom trigger
    }

    public eStateChange StateChange;

    public string Trigger;              // ID of the custom trigger to set off.

    public PayloadStateChange() : base(eComponentTarget.StateChange)
    {

    }

    public override PayloadComponentResult ApplyComponent(Payload payload)
    {
        var result = new PayloadComponentResult(payload, this);
        var target = payload.Target.Entity;

        switch (StateChange)
        {
            case eStateChange.Instakill:
            {
                if (!target.Alive)
                {
                    return result;
                }
                target.OnDeath(payload.Caster?.Entity, result);
                break;
            }
            case eStateChange.Interrupt:
            {
                target.EntityBattle.CancelSkill(payload);
                break;
            }
            case eStateChange.Revive:
            {
                if (target.Alive)
                {
                    return result;
                }
                target.OnReviveIncoming(result);
                break;
            }
            case eStateChange.CancelMovement:
            {
                target.Movement?.CancelMovement();
                break;
            }
            case eStateChange.CancelRotation:
            {
                target.Movement?.CancelRotation();
                break;
            }
            case eStateChange.CustomTrigger:
            {
                target.OnCustomTrigger(Trigger, payload, result);
                break;
            }
            default:
            {
                Debug.LogError($"State change not implemented: {StateChange}");
                break;
            }
        }

        SetTriggers(payload, result);

        return result;
    }
}
#endregion

#region Status Effect
public class PayloadStatusEffect : PayloadComponent
{
    public enum eStatusAction
    {
        ApplyNewStatusEffect,   // Applies a new status effect. If the entity already has this status effect, it is replaced.
        ApplyStacks,            // Applies a new status effect, or increases the number of stacks if the caster has previously applied the effect to the target.
        RemoveStacks,           // If the entity has this status effect, its stacks are decreased. If multiple instances of the status effect exist, all can be affected up to the specified limit.
        ClearStatus,            // A status effect is fully cleared. if multiple instances exist, all can be affected, up to the limit specified.
        ClearStatusGroup,       // Same as above, but all status effects from a given group can be affected. 
        UpdateStatusDuration,   // Increase or decrease the duration of a status effect applied by the caster.
    }

    public eStatusAction StatusAction;
    public string StatusID;

    public int Stacks;                      // For apply and remove actions.
    public int MaxStatusEffectsAffected;    // For actions that can affect more than one status effect, such as stack removal and clear. If 0, there is no limit.

    public bool RefreshTimer;               // For apply action - if true, the timer will be refreshed when stacks are applied. 
    public bool RefreshPayloads;            // For apply action - instead of just increasing stacks, reapplies the status effect, updating the saved entity attributes.

    public Value DurationChange;            // Increases or decreases the duration of a status effect applied by the caster entity. 

    public PayloadStatusEffect() : base(eComponentTarget.StatusEffect)
    {
        Stacks = 1;
        RefreshTimer = true;
        RefreshPayloads = true;
        DurationChange = new Value();
    }

    public override PayloadComponentResult ApplyComponent(Payload payload)
    {
        var result = new PayloadComponentResult(payload, this);

        switch(StatusAction)
        {
            case eStatusAction.ApplyNewStatusEffect:
            {
                var immunity = payload.Target.Entity.HasImmunityAgainstStatus(StatusID);
                if (immunity != null)
                {
                    return result;
                }
                payload.Target.Entity.ApplyStatusEffect(payload.Caster.Entity, StatusID, Stacks, RefreshTimer, RefreshPayloads, isNew: true, payload);
                break;
            }
            case eStatusAction.ApplyStacks:
            {
                var immunity = payload.Target.Entity.HasImmunityAgainstStatus(StatusID);
                if (immunity != null)
                {
                    return result;
                }
                payload.Target.Entity.ApplyStatusEffect(payload.Caster.Entity, StatusID, Stacks, RefreshTimer, RefreshPayloads, isNew: false, payload);
                break;
            }
            case eStatusAction.RemoveStacks:
            {
                payload.Target.Entity.RemoveStatusEffectStacks(payload.Caster.Entity, StatusID, Stacks, MaxStatusEffectsAffected);
                break;
            }
            case eStatusAction.ClearStatus:
            {
                var limit = MaxStatusEffectsAffected;
                payload.Target.Entity.ClearStatusEffect(payload.Caster.Entity, StatusID, ref limit);
                break;
            }
            case eStatusAction.ClearStatusGroup:
            {
                if (BattleData.StatusEffectGroups.ContainsKey(StatusID))
                {
                    var limit = MaxStatusEffectsAffected;
                    foreach (var s in BattleData.StatusEffectGroups[StatusID])
                    {
                        payload.Target.Entity.ClearStatusEffect(payload.Caster.Entity, s, ref limit);
                        if (limit < 1 && MaxStatusEffectsAffected != 0)
                        {
                            break;
                        }
                    }
                }
                break;
            }
            case eStatusAction.UpdateStatusDuration:
            {
                if (!payload.Target.Entity.HasStatusEffect(StatusID, payload.Caster.UID))
                {
                    break;
                }

                var valueInfo = new ValueInfo(payload);
                var change = DurationChange.CalculateValue(valueInfo);

                payload.Target.Entity.ChangeStatusEffectDuration(StatusID, payload.Caster.UID, change);

                break;
            }
        }

        SetTriggers(payload, result);

        return result;
    }
}
#endregion

#region Tag
// An entity can be "tagged". This makes it possible for skills to affect this entity specifically without targeting it.
public class PayloadTag : PayloadComponent
{
    public enum eTagAction
    {
        Tag,
        RemoveTagFromSelf,
        RemoveTagFromAny,
        RemoveTagFromAll,
    }

    public eTagAction TagAction;
    public TagData Tag;

    public PayloadTag() : base(eComponentTarget.Tag)
    {
        Tag = new TagData();
    }

    public override PayloadComponentResult ApplyComponent(Payload payload)
    {
        var result = new PayloadComponentResult(payload, this);

        switch (TagAction)
        {
            case eTagAction.Tag:
            {
                payload.Caster.Entity.TagEntity(Tag.TagID, payload.Target.Entity, Tag);
                break;
            }
            case eTagAction.RemoveTagFromSelf:
            {
                payload.Caster.Entity.RemoveTagOnEntity(Tag.TagID, payload.Target.Entity, selfOnly: false);
                break;
            }
            case eTagAction.RemoveTagFromAny:
            {
                payload.Target.Entity.RemoveTagFromAny(Tag.TagID, all:false);
                break;
            }
            case eTagAction.RemoveTagFromAll:
            {
                payload.Target.Entity.RemoveTagFromAny(Tag.TagID, all: true);
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented tag action: {TagAction}");
                break;
            }
        }

        SetTriggers(payload, result);

        return result;
    }
}
#endregion

#region Transform
public class PayloadTransformChange : PayloadComponent
{
    public PayloadMovement Movement;
    public PayloadRotation Rotation;

    public PayloadTransformChange() : base(eComponentTarget.TransformChange)
    {

    }

    public override PayloadComponentResult ApplyComponent(Payload payload)
    {
        var result = new PayloadComponentResult(payload, this);

        var caster = payload.Caster.Entity;
        var target = payload.Target.Entity;

        var isSkill = caster == target && !string.IsNullOrEmpty(payload.Action.SkillID);

        if (Rotation != null)
        {
            target.Movement.InitiateRotation(Rotation, caster, target, isSkill);
        }

        if (Movement != null)
        {
            target.Movement.InitiateMovement(Movement, caster, target, isSkill);
        }

        SetTriggers(payload, result);

        return result;
    }
}
#endregion