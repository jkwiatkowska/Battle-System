using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadCondition
{
    public enum ePayloadConditionType
    {
        AngleBetweenDirections,
        Chance,
        CasterCategory,
        TargetCategory,
        TargetHasStatus,
        CasterHasStatus,
        TargetWithinDistance,
        TargetResourceRatioWithinRange,
        CasterResourceRatioWithinRange,
    }

    public ePayloadConditionType ConditionType;

    // Chance
    public Value ChanceValue;

    // Status
    public string StatusID;
    public int MinStatusStacks;
    public int MaxStatusStacks;

    // Everything but status
    public Vector2 Range;

    // Resource
    public string Resource;

    // Direction
    public enum eDirection
    {
        CasterToTarget,
        TargetToCaster,
        CasterForward,
        TargetForward,
        CasterRight,
        TargetRight,
    }

    public eDirection Direction1;
    public eDirection Direction2;

    // Entity category
    public string Category;

    // Condition
    public bool ExpectedResult = true;      // If false, the check must fail for the condition to be met.
    public PayloadCondition AndCondition;   // Both conditions must succeed for the condtion to be met.
    public PayloadCondition OrCondition;    // If this check fails, another can be made with a different condition.

    public PayloadCondition(ePayloadConditionType condition = ePayloadConditionType.AngleBetweenDirections)
    {
        SetCondition(condition);
    }

    public void SetCondition(ePayloadConditionType condition)
    {
        ConditionType = condition;

        switch (condition)
        {
            case ePayloadConditionType.AngleBetweenDirections:
            {
                Range = new Vector2(0.0f, 45.0f);

                Direction1 = eDirection.CasterToTarget;
                Direction2 = eDirection.TargetForward;
                break;
            }
            case ePayloadConditionType.Chance:
            {
                ChanceValue = new Value(false);
                ChanceValue.Components.Add(new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 0.5f));
                break;
            }
            case ePayloadConditionType.TargetHasStatus:
            {
                StatusID = "";
                MinStatusStacks = 1;
                MaxStatusStacks = 10000;
                break;
            }
            case ePayloadConditionType.TargetWithinDistance:
            {
                Range = new Vector2(0.0f, 1.0f);
                break;
            }
            case ePayloadConditionType.TargetResourceRatioWithinRange:
            {
                Resource = "";
                Range = new Vector2(0.0f, 0.5f);
                break;
            }
        }
    }

    public bool CheckCondition(EntityInfo caster, EntityInfo target)
    {
        if (!CanCheck(caster, target))
        {
            return false;
        }

        if (ConditionMet(caster, target) == ExpectedResult)
        {
            if (AndCondition != null)
            {
                return AndCondition.CheckCondition(caster, target);
            }

            return true;
        }
        else
        {
            return OrCondition != null && OrCondition.CheckCondition(caster, target);
        }
    }

    bool ConditionMet(EntityInfo caster, EntityInfo target)
    {
        switch (ConditionType)
        {
            case ePayloadConditionType.AngleBetweenDirections:
            {
                var dir1 = Direction(Direction1, caster.Entity, target.Entity);
                var dir2 = Direction(Direction2, caster.Entity, target.Entity);

                var value = Vector3.Angle(dir1, dir2);

                return CheckRange(value, Range.x, Range.y);
            }
            case ePayloadConditionType.Chance:
            {
                var valueInfo = new ValueInfo(caster, target, actionResults: null);
                return Random.value < ChanceValue.CalculateValue(valueInfo);
            }
            case ePayloadConditionType.CasterCategory:
            {
                return caster.Entity.EntityData.Categories.Contains(Category);
            }
            case ePayloadConditionType.TargetCategory:
            {
                return target.Entity.EntityData.Categories.Contains(Category);
            }
            case ePayloadConditionType.CasterHasStatus:
            {
                var stacks = caster.Entity.GetTotalStatusEffectStacks(StatusID);
                return stacks >= MinStatusStacks && stacks <= MaxStatusStacks;
            }
            case ePayloadConditionType.TargetHasStatus:
            {
                var stacks = target.Entity.GetTotalStatusEffectStacks(StatusID);
                return stacks >= MinStatusStacks && stacks <= MaxStatusStacks;
            }
            case ePayloadConditionType.TargetWithinDistance:
            {
                var value = (target.Entity.Position - caster.Entity.Position).sqrMagnitude;
                return CheckRange(value, (Range.x * Range.x), (Range.y * Range.y));
            }
            case ePayloadConditionType.CasterResourceRatioWithinRange:
            {
                var value = caster.Entity.ResourcesCurrent.ContainsKey(Resource) ? caster.Entity.ResourceRatio(Resource) : 0.0f;
                return CheckRange(value, Range.x, Range.y);
            }
            case ePayloadConditionType.TargetResourceRatioWithinRange:
            {
                var value = target.Entity.ResourcesCurrent.ContainsKey(Resource) ? target.Entity.ResourceRatio(Resource) : 0.0f;
                return CheckRange(value, Range.x, Range.y);
            }
        }

        return false;
    }

    bool CheckRange(float value, float min, float max)
    {
        return value >= min - Constants.Epsilon && value <= max + Constants.Epsilon;
    }

    Vector3 Direction(eDirection direction, Entity caster, Entity target)
    {
        switch (direction)
        {
            case eDirection.CasterToTarget:
            {
                return (target.Position - caster.Position).normalized;
            }
            case eDirection.TargetToCaster:
            {
                return (caster.Position - target.Position).normalized;
            }
            case eDirection.CasterForward:
            {
                return caster.transform.forward;
            }
            case eDirection.TargetForward:
            {
                return target.transform.forward;
            }
            case eDirection.CasterRight:
            {
                return caster.transform.right;
            }
            case eDirection.TargetRight:
            {
                return target.transform.right;
            }
            default:
            {
                Debug.LogError($"Invalid direction type: {direction}");
                return Vector3.one;
            }
        }
    }

    bool CanCheck(EntityInfo caster, EntityInfo target)
    {
        if (target == null || target.Entity == null)
        {
            Debug.LogWarning("Target is null");
            return false;
        }

        if (caster?.Entity != null)
        {
            return true;
        }

        var casterNeeded = ConditionType == ePayloadConditionType.AngleBetweenDirections ||
                           ConditionType == ePayloadConditionType.CasterResourceRatioWithinRange ||
                           ConditionType == ePayloadConditionType.CasterHasStatus ||
                           ConditionType == ePayloadConditionType.CasterCategory;

        return !casterNeeded;
    }
}