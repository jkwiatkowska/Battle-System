using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TriggerData
{
    #region Conditions
    public class TriggerCondition
    {
        public enum eConditionType
        {
            CausedBySkill,                      // For a trigger caused by a payload to work, it may be required to have been caused by a specific skill,
            CausedBySkillGroup,                 // or a skill from a specific skill group,
            CausedByAction,                     // or an action.
            PayloadCategory,                    // Custom string that typically refers to the type of damage.
            PayloadFlag,                        // Payload flags are custom string values that can be set when a resource change is applied. 
            ResultFlag,                         // Payload result flags are custom string values that can be set as a result of applying a payload.
            CausedByStatus,                     // For status triggers, used to specify the status.
            CausedByStatusGroup,                // A status belonging to this group.
            ResourceAffected,                   // Particular resource was affected by a payload.
            EntityResourceMin,                  // Resource of this entity must be bigger than the float value.
            TriggerSourceResourceMin,           // Resource of the entity that caused the trigger must be bigger than the float value.
            EntityResourceRatioMin,             // Ratio of current resource to max resource.
            TriggerSourceResourceRatioMin,      // Ratio of current resource to max resource of the entity that caused the trigger.
            EntityHasTag,                       // Entity has a specific tag applied.
            TriggerSourceHasTag,                // Trigger source entity has a specific tag applied.
            EntityHasTagFromTriggerSource,      // Entity has a specific tag applied by the entity that caused the trigger.
            TriggerSourceHasTagFromEntity,      // Trigger source has a specific tag applied by the entity. 
            PayloadResultMin,                   // How much a resource has been depleted as a result of a payload being applied.
            ActionResultMin,                    // How much resources of all targets have been affected as a result of a payload being applied.
            NumTargetsAffectedMin,              // For triggers caused by outgoing payload actions. 
            EntityHasStatus,                    // Trigger can only go off if the entity has this status. Min stacks can be specified.
            TriggerSourceHasStatus,             // Trigger can go off if the entity that caused it has this status.
            TriggerSourceIsEnemy,               // Condition succeeds if the entity that triggered it is an enemy.
            TriggerSourceIsFriend,              // Condition succeeds if the entity that triggered it is a friend.
            EntitiesEngagedMin,                 // The entity is in battle with this number of entities.
        }

        public eConditionType ConditionType;
        public bool DesiredOutcome;             // If false the check must fail for the condition to be met.
        
        public string StringValue;              // Name or ID.            
        public float FloatValue;                // Resource value/change.
        public int IntValue;                    // Count or status stacks.

        public TriggerCondition AndCondition;   // Both this condition and the and condition must pass for the condition to be met. 
        public TriggerCondition OrCondition;    // Alternate conditions can be chained this way. If this condition fails, the next condition will be tried.

        public TriggerCondition()
        {
            DesiredOutcome = true;

            StringValue = "";
            FloatValue = 1.0f;
            IntValue = 1;
        }

        public bool ConditionMet(Entity entity, Entity triggerSource, PayloadResult payloadResult, Action action, ActionResult actionResult, string statusID)
        {
            bool conditionMet;
            switch (ConditionType)
            {
                case eConditionType.CausedBySkill:
                {
                    if (payloadResult == null && action == null)
                    {
                        conditionMet = false;
                    }
                    else
                    {
                        var skillName = payloadResult != null ? payloadResult.SkillID : action.SkillID;
                        conditionMet = StringValue == skillName;
                    }
                    break;
                }
                case eConditionType.CausedBySkillGroup:
                {
                    if (payloadResult == null && action == null)
                    {
                        conditionMet = false;
                    }
                    else
                    {
                        var skillName = payloadResult != null ? payloadResult.SkillID : action.SkillID;
                        conditionMet = BattleData.SkillGroups.ContainsKey(StringValue) &&
                                       BattleData.SkillGroups[StringValue].Contains(skillName);
                    }
                    break;
                }
                case eConditionType.CausedByAction:
                {
                    if (payloadResult == null && action == null)
                    {
                        conditionMet = false;
                    }
                    else
                    {
                        var actionName = payloadResult != null ? payloadResult.ActionID : action.ActionID;
                        conditionMet = actionName == StringValue;
                    }
                    break;
                }
                case eConditionType.PayloadCategory:
                {
                    conditionMet = payloadResult != null && payloadResult.PayloadData.Categories.Contains(StringValue);
                    break;
                }
                case eConditionType.PayloadFlag:
                {
                    conditionMet = payloadResult != null && payloadResult.PayloadData.Flags.Contains(StringValue);
                    break;
                }
                case eConditionType.ResultFlag:
                {
                    conditionMet = payloadResult != null && payloadResult.Flags.Contains(StringValue);
                    break;
                }
                case eConditionType.CausedByStatus:
                {
                    conditionMet = !string.IsNullOrEmpty(statusID) && statusID == StringValue;
                    break;
                }
                case eConditionType.CausedByStatusGroup:
                {
                    conditionMet = !string.IsNullOrEmpty(statusID) && BattleData.StatusEffectGroups.ContainsKey(StringValue) &&
                                   BattleData.StatusEffectGroups[StringValue].Contains(statusID);
                    break;
                }
                case eConditionType.ResourceAffected:
                {
                    conditionMet = payloadResult != null && payloadResult.PayloadData.ResourceAffected == StringValue;
                    break;
                }
                case eConditionType.EntityResourceMin:
                {
                    conditionMet = entity != null && entity.ResourcesCurrent.ContainsKey(StringValue) &&
                                   entity.ResourcesCurrent[StringValue] >= FloatValue;
                    break;
                }
                case eConditionType.TriggerSourceResourceMin:
                {
                    conditionMet = triggerSource != null && triggerSource.ResourcesCurrent.ContainsKey(StringValue) &&
                                   triggerSource.ResourcesCurrent[StringValue] >= FloatValue;
                    break;
                }
                case eConditionType.EntityResourceRatioMin:
                {
                    conditionMet = entity != null && entity.ResourcesCurrent.ContainsKey(StringValue) &&
                                   entity.ResourceRatio(StringValue) >= FloatValue;
                    break;
                }
                case eConditionType.TriggerSourceResourceRatioMin:
                {
                    conditionMet = triggerSource != null && triggerSource.ResourcesCurrent.ContainsKey(StringValue) &&
                                   triggerSource.ResourceRatio(StringValue) >= FloatValue;
                    break;
                }
                case eConditionType.EntityHasTag:
                {
                    conditionMet = entity != null && entity.HasTag(StringValue);
                    break;
                }
                case eConditionType.TriggerSourceHasTag:
                {
                    conditionMet = triggerSource != null && triggerSource.HasTag(StringValue);
                    break;
                }
                case eConditionType.EntityHasTagFromTriggerSource:
                {
                    conditionMet = entity != null && entity.HasTagFromEntity(StringValue, triggerSource.EntityUID);
                    break;
                }
                case eConditionType.TriggerSourceHasTagFromEntity:
                {
                    conditionMet = triggerSource != null && triggerSource.HasTagFromEntity(StringValue, entity.EntityUID);
                    break;
                }
                case eConditionType.PayloadResultMin:
                {
                    conditionMet = payloadResult != null && payloadResult.Change >= FloatValue;
                    break;
                }
                case eConditionType.ActionResultMin:
                {
                    conditionMet = actionResult != null && actionResult.Value >= FloatValue;
                    break;
                }
                case eConditionType.NumTargetsAffectedMin:
                {
                    conditionMet = actionResult != null && actionResult.Count >= IntValue;
                    break;
                }
                case eConditionType.EntityHasStatus:
                {
                    var stacks = entity != null ? entity.GetStatusEffectStacks(StringValue) : 0;
                    conditionMet = stacks >= IntValue;
                    break;
                }
                case eConditionType.TriggerSourceHasStatus:
                {
                    var stacks = triggerSource != null ? triggerSource.GetStatusEffectStacks(StringValue) : 0;
                    conditionMet = stacks >= IntValue;
                    break;
                }
                case eConditionType.TriggerSourceIsEnemy:
                {
                    conditionMet = entity != null && triggerSource != null && entity.IsEnemy(triggerSource.Faction);
                    break;
                }
                case eConditionType.TriggerSourceIsFriend:
                {
                    conditionMet = entity != null && triggerSource != null && entity.IsFriendly(triggerSource.Faction);
                    break;
                }
                case eConditionType.EntitiesEngagedMin:
                {
                    conditionMet = entity != null && entity.EntityBattle.EngagedEntities.Count >= IntValue;
                    break;
                }
                default:
                {
                    Debug.LogError($"Unimplemented trigger condition type: {ConditionType}");
                    conditionMet = false;
                    break;
                }
            }

            if (conditionMet == DesiredOutcome && AndCondition != null)
            {
                conditionMet = AndCondition.ConditionMet(entity, triggerSource, payloadResult, action, actionResult, statusID);
            }

            if (conditionMet != DesiredOutcome && OrCondition != null)
            {
                return OrCondition.ConditionMet(entity, triggerSource, payloadResult, action, actionResult, statusID);
            }

            return conditionMet == DesiredOutcome;
        }

        public List<eConditionType> AvailableConditions(eTrigger triggerType)
        {
            var list = new List<eConditionType>();

            list.Add(eConditionType.EntityResourceMin);
            list.Add(eConditionType.EntityResourceRatioMin);
            list.Add(eConditionType.EntityHasStatus);
            list.Add(eConditionType.EntitiesEngagedMin);
            list.Add(eConditionType.EntityHasTag);

            // These apply to triggers caused by another entity, If another entity didn't cause the trigger, the entity itself is treated as a source entity. 
            list.Add(eConditionType.TriggerSourceResourceMin);
            list.Add(eConditionType.TriggerSourceResourceRatioMin);
            list.Add(eConditionType.TriggerSourceHasStatus);
            list.Add(eConditionType.TriggerSourceIsEnemy);
            list.Add(eConditionType.TriggerSourceIsFriend);
            list.Add(eConditionType.TriggerSourceHasTag);
            list.Add(eConditionType.EntityHasTagFromTriggerSource);
            list.Add(eConditionType.TriggerSourceHasTagFromEntity);

            bool isPayloadTrigger = triggerType == eTrigger.OnPayloadApplied || triggerType == eTrigger.OnPayloadReceived || 
                                    triggerType == eTrigger.OnResourceChanged || triggerType == eTrigger.OnDeath || 
                                    triggerType == eTrigger.OnKill || triggerType == eTrigger.OnHitMissed || triggerType == eTrigger.OnImmune || 
                                    triggerType == eTrigger.OnReviveIncoming || triggerType == eTrigger.OnReviveOutgoing;

            bool isActionTrigger = triggerType == eTrigger.OnActionUsed;

            bool isStatusTrigger = triggerType == eTrigger.OnStatusApplied || triggerType == eTrigger.OnStatusReceived ||
                                   triggerType == eTrigger.OnStatusClearedOutgoing || triggerType == eTrigger.OnStatusClearedIncoming ||
                                   triggerType == eTrigger.OnStatusExpired;

            if (isPayloadTrigger || isActionTrigger)
            {
                list.Add(eConditionType.CausedBySkill);
                list.Add(eConditionType.CausedBySkillGroup);
                list.Add(eConditionType.CausedByAction);
            }

            if (isPayloadTrigger)
            {
                list.Add(eConditionType.PayloadResultMin);
                list.Add(eConditionType.ResourceAffected);
                list.Add(eConditionType.PayloadCategory);
                list.Add(eConditionType.ResultFlag);
            }

            if (isActionTrigger)
            {
                list.Add(eConditionType.ActionResultMin);
                list.Add(eConditionType.NumTargetsAffectedMin);
            }

            if (isStatusTrigger)
            {
                list.Add(eConditionType.CausedByStatus);
                list.Add(eConditionType.CausedByStatusGroup);
            }

            return list;
        }
    }
    #endregion
    public enum eTrigger
    {
        EveryFrame,                     // Fires every frame.
        Custom,                         // Trigger fires if the custom identifier matches.
        OnPayloadApplied,               // Succesfully using a payload action.
        OnPayloadReceived,              // Having payload applied.
        OnHitMissed,                    // Failing to apply a payload action.
        OnImmune,                       // Immunity status effect.
        OnResourceChanged,              // Damage or recovery of a resource.
        OnActionUsed,                   // Use of an action.
        OnStatusApplied,                // Status applied to another entity.
        OnStatusReceived,               // Status applied to this entity.
        OnStatusClearedOutgoing,        // Another entity's status cleared.
        OnStatusClearedIncoming,        // This entity's status cleared.
        OnStatusExpired,                // This entity's status expired.
        OnDeath,                        // Life resource reaches 0.
        OnKill,                         // Killing another entity.
        OnSpawn,                        // Fires after setup.
        OnReviveOutgoing,               // Fires if an entity revives another entity.
        OnReviveIncoming,               // Fires if an entity is revived by another entity.
        OnEngage,                       // Entity enters battle.
        OnDisengage,                    // Entity leaves battle.
        OnCollisionEntity,              // Fires on trigger collision with any entity.
        OnCollisionTargetableEntity,    // Fires on trigger collision with an entity that's targetable.
        OnCollisionTerrain,             // Fires on trigger collision with an object on terrain layer.
        OnEntityMoved,                  // Triggered by an entity moving (non-action movement).
        OnEntityJumped,                 // Triggered when an entity jumps.
        OnEntityLanded,                 // Triggered when an entity lands after a jump or fall. 
    }

    public enum eEntityAffected
    {
        Self,                       // The trigger fired because something happened to the entity it is attached to.
        Summoner,                   // The trigger fired because something happened to the entity that summoned this entity.
        TaggedEntity,               // Trigger fired because something happened to a tagged entity. Summonned entities are automatically tagged with their action ID. 
        EngagedEntity,              // Something happened to an entity that's being battled.
    }

    public eTrigger Trigger;                    // Type of trigger.
    public eEntityAffected EntityAffected;      // Entity affected that the trigger - another entity can cause a trigger to go off for another entity.
    public List<TriggerCondition> Conditions;   // All these conditions have to be met for the trigger to go off.
    public float Cooldown;                      // A trigger can have a cooldown applied whenever it activates to limit its effects
    public int Limit;                           // A trigger can have a limit set. It will be removed from an entity when that limit is reached. Unlimited if 0. 
    public float TriggerChance = 1.0f;          // The odds of an effect triggering. 
    public string CustomIdentifier;             // For custom triggers.

    public ActionTimeline Actions;

    public TriggerData()
    {
        Conditions = new List<TriggerCondition>();
        Actions = new ActionTimeline();
    }

    public TriggerData(eTrigger trigger) : this()
    {
        Trigger = trigger;
    }
}
