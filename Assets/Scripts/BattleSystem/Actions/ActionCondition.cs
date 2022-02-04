using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCondition
{
    public enum eActionCondition
    {
        OnActionSuccess,            // Only execute if the condition action was executed succesfully.
        OnActionFail,               // Only execute if the condition action failed to execute.
        OnValueBelow,               // Checks if a certain value is low enough.
        OnValueAbove                // Checks if a calue is high enough
    }

    // Used by OnMinValue condition
    public enum eConditionValueType
    {
        ChargeRatio,
        DepletableRatio,
        DepletableCurrent,
        RandomValue
    }

    public eActionCondition Condition;
    public eConditionValueType ConditionValueType;
    public float ConditionValueBoundary;            // Minimum or maximum value to compare against
    public string ConditionTarget;                  // Name of depletable if using depletable ratio, or action if onActionSuccess/Fail

    public virtual bool ConditionMet(Entity entity, string actionID, Dictionary<string, ActionResult> actionResults)
    {
        switch (Condition)
        {
            case eActionCondition.OnActionSuccess:
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
            case eActionCondition.OnActionFail:
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
            case eActionCondition.OnValueAbove:
            {
                switch (ConditionValueType)
                {
                    case eConditionValueType.ChargeRatio:
                    {
                        return ConditionValueBoundary <= entity.SkillChargeRatio;
                    }
                    case eConditionValueType.DepletableRatio:
                    {
                        return ConditionValueBoundary <= entity.DepletableRatio(ConditionTarget);
                    }
                    case eConditionValueType.DepletableCurrent:
                    {
                        return ConditionValueBoundary <= entity.DepletablesCurrent[ConditionTarget];
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
            case eActionCondition.OnValueBelow:
            {
                switch (ConditionValueType)
                {
                    case eConditionValueType.ChargeRatio:
                    {
                        return ConditionValueBoundary >= entity.SkillChargeRatio;
                    }
                    case eConditionValueType.DepletableRatio:
                    {
                        return ConditionValueBoundary >= entity.DepletableRatio(ConditionTarget);
                    }
                    case eConditionValueType.DepletableCurrent:
                    {
                        return ConditionValueBoundary >= entity.DepletablesCurrent[ConditionTarget];
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
            default:
            {
                Debug.LogError($"Unimplemented execute condition: {Condition}");
                return false;
            }
        }
    }
}
