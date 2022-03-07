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
            Skill,                              // For a trigger caused by a payload to work, it may be required to have been caused by a specific skill,
            SkillGroup,                         // or a skill from a specific skill group,
            Action,                             // or an action.
            Category,                           // Custom string that typically refers to the type of damage.
            PayloadFlag,                        // Payload flags are custom string values that can be set when a resource change is applied. 
            ResultFlag,                         // Payload result flags are custom string values that can be set as a result of applying a payload.
            Status,                             // For status triggers, used to specify the status.
            StatusGroup,                        // A status belonging to this group.
            ResourceAffected,                   // Particular resource was affected by a payload.
            EntityResource,                     // Resource of this entity must be bigger than the float value.
            TriggerSourceResource,              // Resource of the entity that caused the trigger must be bigger than the float value.
            EntityResourceRatio,                // Ratio of current resource to max resource.
            TriggerSourceResourceRatio,         // Ratio of current resource to max resource of the entity that caused the trigger.
            PayloadResult,                      // How much a resource has been depleted as a result of a payload being applied.
            ActionResult,                       // How much resources of all targets have been affected as a result of a payload being applied.
            NumTargetsAffected,                 // For triggers caused by outgoing payload actions. 
            HasStatus,                          // Trigger can only go off if the entity has this status. Min stacks can be specified.
            TriggerSourceHasStatus,             // Trigger can go off if the entity that caused it has this status.
            TriggerSourceIsEnemy,               // Condition succeeds if the entity that triggered it is an enemy.
            TriggerSourceIsFriend,              // Condition succeeds if the entity that triggered it is a friend.
        }

        public eConditionType ConditionType;
        public bool DesiredOutcome;             // If false the check must fail for the condition to be met.
        
        public string StringValue;              // Name or ID.            
        public float FloatValue;                // Resource value/change.
        public float IntValue;                  // Count or status stacks.

        public TriggerCondition AndCondition;   // Both this condition and the and condition must pass for the condition to be met. 
        public TriggerCondition OrCondition;    // Alternate conditions can be chained this way. If this condition fails, the next condition will be tried.

        public bool ConditionMet(Entity entity, Entity triggerSource, PayloadResult payloadResult, Action action, ActionResult actionResult, string statusID)
        {
            bool conditionMet;
            switch (ConditionType)
            {
                case eConditionType.Skill:
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
                case eConditionType.SkillGroup:
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
                case eConditionType.Action:
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
                case eConditionType.Category:
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
                case eConditionType.Status:
                {
                    conditionMet = !string.IsNullOrEmpty(statusID) && statusID == StringValue;
                    break;
                }
                case eConditionType.StatusGroup:
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
                case eConditionType.EntityResource:
                {
                    conditionMet = entity != null && entity.ResourcesCurrent.ContainsKey(StringValue) &&
                                   entity.ResourcesCurrent[StringValue] >= FloatValue;
                    break;
                }
                case eConditionType.TriggerSourceResource:
                {
                    conditionMet = triggerSource != null && triggerSource.ResourcesCurrent.ContainsKey(StringValue) &&
                                   triggerSource.ResourcesCurrent[StringValue] >= FloatValue;
                    break;
                }
                case eConditionType.EntityResourceRatio:
                {
                    conditionMet = entity != null && entity.ResourcesCurrent.ContainsKey(StringValue) &&
                                   entity.ResourceRatio(StringValue) >= FloatValue;
                    break;
                }
                case eConditionType.TriggerSourceResourceRatio:
                {
                    conditionMet = triggerSource != null && triggerSource.ResourcesCurrent.ContainsKey(StringValue) &&
                                   triggerSource.ResourceRatio(StringValue) >= FloatValue;
                    break;
                }
                case eConditionType.PayloadResult:
                {
                    conditionMet = payloadResult != null && payloadResult.Change >= FloatValue;
                    break;
                }
                case eConditionType.ActionResult:
                {
                    conditionMet = actionResult != null && actionResult.Value >= FloatValue;
                    break;
                }
                case eConditionType.NumTargetsAffected:
                {
                    conditionMet = actionResult != null && actionResult.Count >= IntValue;
                    break;
                }
                case eConditionType.HasStatus:
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

            list.Add(eConditionType.EntityResource);
            list.Add(eConditionType.EntityResourceRatio);
            list.Add(eConditionType.HasStatus);

            bool isPayloadTrigger = triggerType == eTrigger.OnPayloadApplied || triggerType == eTrigger.OnPayloadReceived || 
                                    triggerType == eTrigger.OnResourceChanged || triggerType == eTrigger.OnDeath || 
                                    triggerType == eTrigger.OnKill || triggerType == eTrigger.OnHitMissed || triggerType == eTrigger.OnImmune;

            bool isActionTrigger = triggerType == eTrigger.OnActionUsed;

            bool isStatusTrigger = triggerType == eTrigger.OnStatusApplied || triggerType == eTrigger.OnStatusReceived ||
                                   triggerType == eTrigger.OnStatusClearedOutgoing || triggerType == eTrigger.OnStatusClearedIncoming ||
                                   triggerType == eTrigger.OnStatusExpired;

            bool hasSourceEntity = isPayloadTrigger || isStatusTrigger || triggerType == eTrigger.OnSpawn || 
                                   triggerType == eTrigger.OnCollisionEnemy || triggerType == eTrigger.OnCollisionFriend;

            if (isPayloadTrigger || isActionTrigger)
            {
                list.Add(eConditionType.Skill);
                list.Add(eConditionType.SkillGroup);
                list.Add(eConditionType.Action);
            }

            if (hasSourceEntity)
            {
                list.Add(eConditionType.TriggerSourceResource);
                list.Add(eConditionType.TriggerSourceResourceRatio);
                list.Add(eConditionType.TriggerSourceHasStatus);
                list.Add(eConditionType.TriggerSourceIsEnemy);
                list.Add(eConditionType.TriggerSourceIsFriend);
            }

            if (isPayloadTrigger)
            {
                list.Add(eConditionType.PayloadResult);
                list.Add(eConditionType.ResourceAffected);
                list.Add(eConditionType.Category);
                list.Add(eConditionType.ResultFlag);
            }

            if (isActionTrigger)
            {
                list.Add(eConditionType.ActionResult);
                list.Add(eConditionType.NumTargetsAffected);
            }

            if (isStatusTrigger)
            {
                list.Add(eConditionType.Status);
                list.Add(eConditionType.StatusGroup);
            }

            return list;
        }
    }
    #endregion
    public enum eTrigger
    {
        OnPayloadApplied,           // Succesfully using a payload action.
        OnPayloadReceived,          // Having payload applied.
        OnHitMissed,                // Failing to apply a payload action.
        OnImmune,              // Immunity status effect.
        OnResourceChanged,          // Damage or recovery of a resource.
        OnActionUsed,               // Use of an action.
        OnStatusApplied,            // Status applied to another entity.
        OnStatusReceived,           // Status applied to this entity.
        OnStatusClearedOutgoing,    // Another entity's status cleared.
        OnStatusClearedIncoming,    // This entity's status cleared.
        OnStatusExpired,            // This entity's status expired.
        OnDeath,                    // Life resource reaches 0.
        OnKill,                     // Killing another entity.
        OnSpawn,                    // Fires after setup.
        OnCollisionEnemy,           // Fires on trigger collision with an enemy.
        OnCollisionFriend,          // Fireso on trigger collision with a friend.
        OnCollisionTerrain,         // Fires on trigger collision with an object on terrain layer.
    }

    public eTrigger Trigger;                    // Type of trigger.
    public List<TriggerCondition> Conditions;   // All these conditions have to be met for the trigger to go off.
    public float Cooldown;                      // A trigger can have a cooldown applied whenever it activates to limit its effects
    public int Limit;                           // A trigger can have a limit set. It will be removed from an entity when that limit is reached. Unlimited if 0. 
    public float TriggerChance = 1.0f;          // The odds of an effect triggering. 

    public ActionTimeline Actions;
}
