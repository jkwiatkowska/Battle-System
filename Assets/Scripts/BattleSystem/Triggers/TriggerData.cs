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
            CausedByActionType,                 // Action of a specified type.
            PayloadCategory,                    // Custom string that typically refers to the type of damage.
            PayloadFlag,                        // Payload flags are custom string values that can be set when a resource change is applied. 
            ResultFlag,                         // Payload result flags are custom string values that can be set as a result of applying a payload.
            CausedByStatus,                     // For status triggers, used to specify the status.
            CausedByStatusGroup,                // A status belonging to this group.
            ResourceAffected,                   // Particular resource was affected by a payload.
            CompareValues,                      // Two values are compared, for example entity attributes, resources and level. If the first value is bigger, the condition is met.
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

        public Action.eActionType ActionType;   // For action type check.
        public string StringValue;              // Name or ID.            
        public Value Value;                     // Value being compared.
        public Value ComparisonValue;           // Value being compared to.
        public int IntValue;                    // Count or status stacks.
        public int IntValue2;

        public TriggerCondition AndCondition;   // Both this condition and the and condition must pass for the condition to be met. 
        public TriggerCondition OrCondition;    // Alternate conditions can be chained this way. If this condition fails, the next condition will be tried.

        public TriggerCondition()
        {
            DesiredOutcome = true;

            StringValue = "";
            Value = new Value();
            ComparisonValue = new Value();
            IntValue = 1;
        }

        public bool ConditionMet(Entity entity, Entity triggerSource, Payload payload, PayloadComponentResult payloadResult, Action action, 
                                 ActionResult actionResult, Dictionary<string, ActionResult> actionResults, string statusID)
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
                        var skillName = payloadResult != null ? payload?.Action?.SkillID : action.SkillID;
                        conditionMet = !string.IsNullOrEmpty(skillName) && StringValue == skillName;
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
                        var skillName = payloadResult != null ? payload?.Action?.SkillID : action.SkillID;
                        conditionMet = !string.IsNullOrEmpty(skillName) && BattleData.SkillGroups.ContainsKey(StringValue) &&
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
                        var actionName = payloadResult != null ? payload?.Action?.ActionID : action.ActionID;
                        conditionMet = !string.IsNullOrEmpty(actionName) && actionName == StringValue;
                    }
                    break;
                }
                case eConditionType.CausedByActionType:
                {
                    if (payloadResult == null && action == null)
                    {
                        conditionMet = false;
                    }
                    else
                    {
                        var actionType = payloadResult != null ? payload?.Action?.ActionType : action.ActionType;
                        conditionMet =  actionType == ActionType;
                    }
                    break;
                }
                case eConditionType.PayloadCategory:
                {
                    var categories = payload?.PayloadData?.Categories;
                    conditionMet = categories != null && categories.Contains(StringValue);
                    break;
                }
                case eConditionType.PayloadFlag:
                {
                    var flags = payloadResult?.PayloadComponent?.Flags;
                    conditionMet = flags != null && flags.Contains(StringValue);
                    break;
                }
                case eConditionType.ResultFlag:
                {
                    var flags = payloadResult?.ResultFlags;
                    conditionMet = flags != null && flags.Contains(StringValue);
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
                    var payloadComponent = payloadResult?.PayloadComponent as PayloadResourceChange;
                    var resource = payloadComponent?.ResourceAffected;
                    conditionMet = !string.IsNullOrEmpty(resource) && resource == StringValue;
                    break;
                }
                case eConditionType.CompareValues:
                {
                    var results = actionResults != null ? actionResults : payload?.ActionResults;
                    var actionID = action != null ? action.ActionID : payload?.Action?.ActionID;
                    var valueInfo = new ValueInfo(entity?.EntityInfo, triggerSource?.EntityInfo, results, actionID);

                    var v1 = Value.CalculateValue(valueInfo);
                    var v2 = ComparisonValue.CalculateValue(valueInfo);
                    conditionMet = v1 > v2;
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
                    conditionMet = entity != null && entity.HasTagFromEntity(StringValue, triggerSource.UID);
                    break;
                }
                case eConditionType.TriggerSourceHasTagFromEntity:
                {
                    conditionMet = triggerSource != null && triggerSource.HasTagFromEntity(StringValue, entity.UID);
                    break;
                }
                case eConditionType.PayloadResultMin:
                {
                    var valueInfo = new ValueInfo(entity?.EntityInfo, triggerSource?.EntityInfo, payload?.ActionResults, payload?.Action?.ActionID);
                    var v = ComparisonValue.CalculateValue(valueInfo);

                    conditionMet = payloadResult != null && payloadResult.ResultValue >= v;
                    break;
                }
                case eConditionType.ActionResultMin:
                {
                    var results = actionResults != null ? actionResults : payload?.ActionResults;
                    var actionID = action != null ? action.ActionID : payload?.Action?.ActionID;

                    var valueInfo = new ValueInfo(entity?.EntityInfo, triggerSource?.EntityInfo, results, actionID);
                    var v = ComparisonValue.CalculateValue(valueInfo);

                    conditionMet = actionResult != null && actionResult.Values[StringValue] >= v;
                    break;
                }
                case eConditionType.NumTargetsAffectedMin:
                {
                    conditionMet = actionResult != null && actionResult.Count >= IntValue;
                    break;
                }
                case eConditionType.EntityHasStatus:
                {
                    var stacks = entity != null ? entity.GetTotalStatusEffectStacks(StringValue) : 0;
                    conditionMet = stacks >= IntValue && stacks <= IntValue2;
                    break;
                }
                case eConditionType.TriggerSourceHasStatus:
                {
                    var stacks = triggerSource != null ? triggerSource.GetTotalStatusEffectStacks(StringValue) : 0;
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

            var success = conditionMet == DesiredOutcome;
            if (success && AndCondition != null)
            {
                success = AndCondition.ConditionMet(entity, triggerSource, payload, payloadResult, action, actionResult, actionResults, statusID);
            }

            if (!success && OrCondition != null)
            {
                return OrCondition.ConditionMet(entity, triggerSource, payload, payloadResult, action, actionResult, actionResults, statusID);
            }

            return success;
        }

        public List<eConditionType> AvailableConditions(eTrigger triggerType)
        {
            var list = new List<eConditionType>();

            list.Add(eConditionType.CompareValues);
            list.Add(eConditionType.EntityHasStatus);
            list.Add(eConditionType.EntitiesEngagedMin);
            list.Add(eConditionType.EntityHasTag);

            // These apply to triggers caused by another entity, If another entity didn't cause the trigger, the entity itself is treated as a source entity. 
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
                                   triggerType == eTrigger.OnStatusExpired || triggerType == eTrigger.OnStatusEnded;

            if (isPayloadTrigger || isActionTrigger)
            {
                list.Add(eConditionType.CausedBySkill);
                list.Add(eConditionType.CausedBySkillGroup);
                list.Add(eConditionType.CausedByAction);
                list.Add(eConditionType.CausedByActionType);
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
        OnStatusEnded,                  // Status ends either by being cleared or expiring.
        OnStatusClearedOutgoing,        // Another entity's status cleared.
        OnStatusClearedIncoming,        // This entity's status cleared.
        OnStatusExpired,                // This entity's status expired.
        OnDeath,                        // Life resource reaches 0.
        OnKill,                         // Killing another entity.
        OnSpawn,                        // Fires after setup.
        OnReviveOutgoing,               // Fires if an entity revives another entity.
        OnReviveIncoming,               // Fires if an entity is revived by another entity.
        OnEngage,                       // Entity engages another in battle.
        OnDisengage,                    // Battle with an entity ends.
        OnBattleStart,                  // Entity enters battle.
        OnBattleEnd,                    // Entity leaves battle.
        OnCollisionEntity,              // Fires on trigger collision with any entity. Both entities need colliders for this to work.
        OnCollisionTargetableEntity,    // Fires on trigger collision with an entity that's targetable. Both entities need colliders for this to work.
        OnEntityInTriggerCollider,      // Fires if any entity is inside the trigger collider. Both entities need trigger colliders for this to work.
        OnTargetableInTriggerCollider,  // Fires if a targetable entity is inside the trigger collider. Both entities need trigger colliders for this to work.
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

    public eTrigger Trigger;                        // Type of trigger.
    public eEntityAffected EntityAffected;          // Entity affected that the trigger - another entity can cause a trigger to go off for another entity.
    public List<TriggerCondition> Conditions;       // All these conditions have to be met for the trigger to go off.
    public float Cooldown;                          // A trigger can have a cooldown applied whenever it activates to limit its effects
    public int Limit;                               // A trigger can have a limit set. It will be removed from an entity when that limit is reached. Unlimited if 0. 
    public Value TriggerChance;                     // The odds of an effect triggering. 
    public string CustomIdentifier;                 // For custom triggers.

    public List<TriggerReaction> TriggerReactions;  // Skills used by the entity when hit by the trigger.
    public List<SaveValue> ValuesToSave;            // Values such as action and payload result can be saved before the trigger reaction is executed, allowing the latter to make use of these values.

    public TriggerData()
    {
        Conditions = new List<TriggerCondition>();
        TriggerReactions = new List<TriggerReaction>();
        ValuesToSave = new List<SaveValue>();
    }

    public TriggerData(eTrigger trigger) : this()
    {
        Trigger = trigger;
    }

    public override string ToString()
    {
        var l = "[" + Trigger.ToString() + "]";
        if (Conditions != null && Conditions.Count > 0)
        {
            foreach (var c in Conditions)
            {
                if (c.ConditionType == TriggerCondition.eConditionType.CausedBySkill)
                {
                    l += " Skill: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.CausedBySkillGroup)
                {
                    l += " Skill Group: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.CausedByAction ||
                         c.ConditionType == TriggerCondition.eConditionType.ActionResultMin)
                {
                    l += " Action: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.CausedByStatus ||
                         c.ConditionType == TriggerCondition.eConditionType.TriggerSourceHasStatus)
                {
                    l += " Status: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.CausedByStatusGroup)
                {
                    l += " Status Group: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.EntityHasTag ||
                         c.ConditionType == TriggerCondition.eConditionType.EntityHasTagFromTriggerSource)
                {
                    l += " Tag: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.ResultFlag)
                {
                    l += " Result Flag: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.ResourceAffected)
                {
                    l += " Resource: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerCondition.eConditionType.PayloadFlag)
                {
                    l += " Payload Flag: ";
                    l += c.StringValue;
                }
                else if (c.ConditionType == TriggerData.TriggerCondition.eConditionType.PayloadCategory)
                {
                    l += " Payload Category: ";
                    l += c.StringValue;
                }
            }
        }

        if (TriggerReactions != null && TriggerReactions.Count > 0)
        {
            for (int i = 0; i < TriggerReactions.Count; i++)
            {
                l += $" -> {TriggerReactions[i].SkillID}";
                if (i > 0)
                {
                    l += " + ";
                }
            }
        }

        return l;
    }
}

public class TriggerReaction
{
    public enum eTriggerReactionTarget
    {
        TriggerSource,
        SelectedEntity,
        Self,
    }

    public string SkillID;
    public eTriggerReactionTarget ReactionTarget;

    public TriggerReaction()
    {

    }

    public TriggerReaction(string skillID)
    {
        SkillID = skillID;
        ReactionTarget = eTriggerReactionTarget.TriggerSource;
    }

    public void React(Entity affectedEntity, Entity triggerSource)
    {
        var target = triggerSource;

        if (ReactionTarget == eTriggerReactionTarget.SelectedEntity)
        {
            target = affectedEntity.TargetingSystem?.SelectedTarget;
        }
        else if (ReactionTarget == eTriggerReactionTarget.Self)
        {
            target = affectedEntity;
        }

        affectedEntity.EntityBattle.TryUseSkill(SkillID, target);
    }
}