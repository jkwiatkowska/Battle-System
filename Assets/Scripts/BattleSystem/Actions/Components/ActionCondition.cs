using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCondition
{
    public enum eActionCondition
    {
        ActionSuccess,              // Only execute if the condition action was executed succesfully.
        ActionFail,                 // Only execute if the condition action failed to execute.
        ValueBelow,                 // Checks if a certain value is low enough.
        ValueAbove,                 // Checks if a value is high enough
        HasStatus,                  // Checks if the caster has the specified status.
    }

    // Used by OnMinValue condition
    public enum eConditionValueType
    {
        ActionResult,
        ChargeRatio,
        DistanceFromTarget,
        ResourceRatio,
        ResourceCurrent,
        RandomValue,
    }

    public eActionCondition Condition;
    public eConditionValueType ConditionValueType;
    public float ConditionValueBoundary;            // Minimum or maximum value to compare against
    public string ConditionTarget;                  // Name of resource if using resource ratio, action if ActionSuccess/Fail or status ID
    public int MinStatusStacks;                     // For the HasStatus condition.

    public ActionCondition()
    {
        MinStatusStacks = 1;
    }

    public ActionCondition(eActionCondition condition):this()
    {
        Condition = condition;
    }

    public virtual bool ConditionMet(Entity entity, Entity target, string actionID, Dictionary<string, ActionResult> actionResults)
    {
        switch (Condition)
        {
            case eActionCondition.ActionSuccess:
            {
                if (actionResults != null && actionResults.ContainsKey(ConditionTarget))
                {
                    return actionResults[ConditionTarget].Success;
                }
                else
                {
                    Debug.LogError($"Condition action {ConditionTarget} for action {actionID} has not been executed.");
                    return false;
                }
            }
            case eActionCondition.ActionFail:
            {
                if (actionResults != null && actionResults.ContainsKey(ConditionTarget))
                {
                    return !actionResults[ConditionTarget].Success;
                }
                else
                {
                    Debug.LogError($"Condition action {ConditionTarget} for action {actionID} has not been executed.");
                    return false;
                }
            }
            case eActionCondition.ValueAbove:
            {
                return ConditionValueBoundary <= ConditionValue(entity, target, actionID, actionResults);
            }
            case eActionCondition.ValueBelow:
            {
                return ConditionValueBoundary >= ConditionValue(entity, target, actionID, actionResults);
            }
            case eActionCondition.HasStatus:
            {
                return entity.GetStatusEffectStacks(ConditionTarget) > MinStatusStacks;
            }
            default:
            {
                Debug.LogError($"Unimplemented execute condition: {Condition}");
                return false;
            }
        }
    }

    float ConditionValue(Entity entity, Entity target, string actionID, Dictionary<string, ActionResult> actionResults)
    {
        switch (ConditionValueType)
        {
            case eConditionValueType.ActionResult:
            {
                return actionResults.ContainsKey(ConditionTarget) ? actionResults[ConditionTarget].Value : 0.0f;
            }
            case eConditionValueType.ChargeRatio:
            {
                return entity.EntityBattle.SkillChargeRatio;
            }
            case eConditionValueType.DistanceFromTarget:
            {
                return target != null ? VectorUtility.Distance2D(entity, target) : 0.0f; 
            }
            case eConditionValueType.ResourceRatio:
            {
                return entity.ResourceRatio(ConditionTarget);
            }
            case eConditionValueType.ResourceCurrent:
            {
                return entity.ResourcesCurrent[ConditionTarget];
            }
            case eConditionValueType.RandomValue:
            {
                return Random.value;
            }
            default:
            {
                Debug.LogError($"Unimplemented condition value type: {ConditionValueType}");
                return 0.0f;
            }
        }
    }
}
