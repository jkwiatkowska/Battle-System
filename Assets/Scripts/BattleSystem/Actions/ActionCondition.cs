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
        RandomValue
    }

    public eActionCondition Condition;
    public eConditionValueType ConditionValueType;
    public float ConditionValueBoundary;            // Minimum or maximum value to compare against
    public string ConditionTarget;                  // Name of depletable if using depletable ratio, or action if onActionSuccess/Fail

    public virtual bool ConditionMet(Entity entity, string actionID)
    {
        switch (Condition)
        {
            case eActionCondition.OnActionSuccess:
            {
                if (entity.ActionResults.ContainsKey(ConditionTarget))
                {
                    return entity.ActionResults[ConditionTarget].Success;
                }
                else
                {
                    Debug.LogError($"Condition action {ConditionTarget} for action {actionID} has not been executed.");
                    return false;
                }
            }
            case eActionCondition.OnActionFail:
            {
                if (entity.ActionResults.ContainsKey(ConditionTarget))
                {
                    return !entity.ActionResults[ConditionTarget].Success;
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
                    case eConditionValueType.RandomValue:
                    {
                        return ConditionValueBoundary <= Random.value;
                    }
                    default:
                    {
                        Debug.LogError($"Unsupported condition value type: {ConditionValueType}");
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
                    case eConditionValueType.RandomValue:
                    {
                        return ConditionValueBoundary >= Random.value;
                    }
                    default:
                    {
                        Debug.LogError($"Unsupported condition value type: {ConditionValueType}");
                        return false;
                    }
                }
            }
            default:
            {
                Debug.LogError($"Unsupported execute condition: {Condition}");
                return false;
            }
        }
    }
}
