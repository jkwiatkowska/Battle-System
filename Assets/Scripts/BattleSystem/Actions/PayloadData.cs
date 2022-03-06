using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadData
{
    public List<string> Categories;                     // Can define what kind of payload this is, affected by damage bonuses and resistances

    public Value PayloadValue;                          // The value components are added together. Example: 20% TargetMaxHP + 1000 (FlatValue)
    public string ResourceAffected;                     // Typically hp, but can be other things like energy

    public List<string> Flags;                          // Flags to customise the payload 

    public TagData Tag;                                 // An entity can be "tagged". This makes it possible for skills to affect this entity specifically without selecting it

    public List<(string StatusID, int Stacks)> ApplyStatus;             // Effects applied to the target entity along with the payload.
    public List<(bool StatusGroup, string StatusID)> ClearStatus;       // Effects cleared from the target entity.
    public List<(string StatusID, int Stacks)> RemoveStatus;            // Status stacks cleared from the target entity

    public bool Instakill = false;                      // Used to set an entity's OnDeath trigger.

    public float SuccessChance;

    public PayloadCondition PayloadCondition;           // Condition for the payload to be applied.

    // TO DO:
    // - Force applied to target.

    public PayloadData()
    {
        Categories = new List<string>();
        PayloadValue = new Value();
        Flags = new List<string>();
        SuccessChance = 1.0f;
    }
}

public abstract class PayloadCondition
{
    public enum ePayloadConditionType
    {
        TargetHasStatus,
        TargetWithinDistance,
        TargetResourceInRange,
        AngleBetweenDirections,
    }

    public ePayloadConditionType ConditionType;

    public bool ExpectedResult;             // If false, the check must fail for the condition to be met.
    public PayloadCondition AndCondition;   // Both conditions must succeed for the condtion to be met.
    public PayloadCondition OrCondition;    // If this check fails, another can be made with a different condition.

    public bool CheckCondition(Entity caster, Entity target)
    {
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

    protected abstract bool ConditionMet(Entity caster, Entity target);
}

public class PayloadConditionStatus : PayloadCondition
{
    public string StatusID;
    public int MinStatusStacks;

    protected override bool ConditionMet(Entity caster, Entity target)
    {
        return target.GetStatusEffectStacks(StatusID) > MinStatusStacks;
    }
}

public abstract class PayloadConditionRange : PayloadCondition
{
    public Vector2 Range;

    protected override bool ConditionMet(Entity caster, Entity target)
    {
        var value = GetValue(caster, target);

        return value >= Range.x - Constants.Epsilon && value <= Range.y + Constants.Epsilon;
    }

    protected abstract float GetValue(Entity caster, Entity target);
}

public class PayloadConditionDistance : PayloadConditionRange
{
    protected override bool ConditionMet(Entity caster, Entity target)
    {
        var value = GetValue(caster, target);

        return value >= (Range.x * Range.x) - Constants.Epsilon && value <= (Range.y * Range.y) + Constants.Epsilon;
    }

    protected override float GetValue(Entity caster, Entity target)
    {
        return (target.transform.position - caster.transform.position).sqrMagnitude;
    }
}

public class PayloadConditionResource : PayloadConditionRange
{
    public string Resource;

    protected override float GetValue(Entity caster, Entity target)
    {
        return (target.ResourcesCurrent.ContainsKey(Resource) ? target.ResourcesCurrent[Resource] : 0.0f);
    }
}

public class PayloadConditionAngle : PayloadConditionRange
{
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

    protected override float GetValue(Entity caster, Entity target)
    {
        var dir1 = Direction(Direction1, caster, target);
        var dir2 = Direction(Direction2, caster, target);

        return Vector3.Angle(dir1, dir2);
    }

    Vector3 Direction(eDirection direction, Entity caster, Entity target)
    {
        switch (direction)
        {
            case eDirection.CasterToTarget:
            {
                return (target.transform.position - caster.transform.position).normalized;
            }
            case eDirection.TargetToCaster:
            {
                return (caster.transform.position - target.transform.position).normalized;
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
}