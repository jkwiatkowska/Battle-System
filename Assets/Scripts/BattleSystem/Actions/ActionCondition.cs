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
        ChargeRatio,
        ResourceRatio,
        ResourceCurrent,
        RandomValue
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

    public virtual bool ConditionMet(Entity entity, string actionID, Dictionary<string, ActionResult> actionResults)
    {
        switch (Condition)
        {
            case eActionCondition.ActionSuccess:
            {
                if (actionResults.ContainsKey(ConditionTarget))
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
                if (actionResults.ContainsKey(ConditionTarget))
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
                switch (ConditionValueType)
                {
                    case eConditionValueType.ChargeRatio:
                    {
                        return ConditionValueBoundary <= entity.Skills.SkillChargeRatio;
                    }
                    case eConditionValueType.ResourceRatio:
                    {
                        return ConditionValueBoundary <= entity.ResourceRatio(ConditionTarget);
                    }
                    case eConditionValueType.ResourceCurrent:
                    {
                        return ConditionValueBoundary <= entity.ResourcesCurrent[ConditionTarget];
                    }
                    case eConditionValueType.RandomValue:
                    {
                        return ConditionValueBoundary <= Random.value;
                    }
                    default:
                    {
                        Debug.LogError($"Unimplemented condition value type: {ConditionValueType}");
                        return false;
                    }
                }
            }
            case eActionCondition.ValueBelow:
            {
                switch (ConditionValueType)
                {
                    case eConditionValueType.ChargeRatio:
                    {
                        return ConditionValueBoundary >= entity.Skills.SkillChargeRatio;
                    }
                    case eConditionValueType.ResourceRatio:
                    {
                        return ConditionValueBoundary >= entity.ResourceRatio(ConditionTarget);
                    }
                    case eConditionValueType.ResourceCurrent:
                    {
                        return ConditionValueBoundary >= entity.ResourcesCurrent[ConditionTarget];
                    }
                    case eConditionValueType.RandomValue:
                    {
                        return ConditionValueBoundary >= Random.value;
                    }
                    default:
                    {
                        Debug.LogError($"Unimplemented condition value type: {ConditionValueType}");
                        return false;
                    }
                }
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
}
