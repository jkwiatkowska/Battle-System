using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCondition
{
    public enum eActionCondition
    {
        ActionSuccess,                              // Only execute if the condition action was executed succesfully.
        ValueCompare,                               // Compares two values.
        CasterHasStatusEffect,
        TargetHasStatusEffect,
    }

    public eActionCondition Condition;
    public bool RequiredResult;

    public string StringValue;                      // For the action success condition.
    public Value Value;                             // For value comparison.
    public Value ComparisonValue;
    public int MinStacks;
    public int MaxStacks;

    public ActionCondition AndCondition;            // If multiple conditions are required.
    public ActionCondition OrCondition;             // Alternate condition.

    public ActionCondition()
    {
        RequiredResult = true;
        Value = new Value();
        ComparisonValue = new Value();
    }

    public ActionCondition(eActionCondition condition):this()
    {
        Condition = condition;
    }

    public virtual bool ConditionsMet(string actionID, ValueInfo valueInfo)
    {
        var success = ConditionMet(actionID, valueInfo);

        if (success && AndCondition != null)
        {
            success = AndCondition.ConditionsMet(actionID, valueInfo);
        }

        if (!success && OrCondition != null)
        {
            success = OrCondition.ConditionsMet(actionID, valueInfo);
        }

        return success;
    }

    public virtual bool ConditionMet(string actionID, ValueInfo valueInfo)
    {
        if (valueInfo == null)
        {
            Debug.LogError($"Value info is null.");
            return false;
        }

        var result = false;
        switch (Condition)
        {
            case eActionCondition.ActionSuccess:
            {
                if (valueInfo?.ActionResults != null && valueInfo.ActionResults.ContainsKey(StringValue))
                {
                    result = valueInfo.ActionResults[StringValue].Success;
                    break;
                }
                else
                {
                    Debug.LogError($"Condition action {StringValue} for action {actionID} has not been executed.");
                    return false;
                }
            }
            case eActionCondition.ValueCompare:
            {
                if (Value == null || ComparisonValue == null)
                {
                    Debug.LogError($"Attempted comparing two values, but one of them is null.");
                    return false;
                }
                var v1 = Value.CalculateValue(valueInfo);
                var v2 = Value.CalculateValue(valueInfo);
                result = v1 + Constants.Epsilon > v2;
                break;
            }
            case eActionCondition.CasterHasStatusEffect:
            {
                if (valueInfo?.Caster?.Entity != null)
                {
                    var stacks = valueInfo.Caster.Entity.GetHighestStatusEffectStacks(StringValue);
                    result = stacks >= MinStacks && stacks <= MaxStacks;
                }
                else
                {
                    Debug.LogError($"Caster is null.");
                }

                break;
            }
            case eActionCondition.TargetHasStatusEffect:
            {
                if (valueInfo?.Target?.Entity != null)
                {
                    var stacks = valueInfo.Target.Entity.GetHighestStatusEffectStacks(StringValue);
                    result = stacks >= MinStacks && stacks <= MaxStacks;
                }
                else
                {
                    Debug.LogError($"Caster is null.");
                }

                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented execute condition: {Condition}");
                return false;
            }
        }

        return result == RequiredResult;
    }
}
