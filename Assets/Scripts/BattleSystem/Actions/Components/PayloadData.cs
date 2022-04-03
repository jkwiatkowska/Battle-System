using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadData
{
    public List<string> Categories;                     // Can define what kind of payload this is, affected by damage bonuses and resistances

    public Value PayloadValue;                          // The value components are added together. Example: 20% Target Max HP + 1000 (FlatValue)
    public Value PayloadValueMax;                       // A cap can be set on the payload value so that it doesn't go over a certain amount.
    public string ResourceAffected;                     // Typically hp, but can be other things like energy

    public AggroData.AggroChange Aggro;                 // Entities can select targets based on aggro. 
    public bool MultiplyAggroByPayloadValue;            // If true, aggro will be multiplied by damage dealt.

    public Dictionary<string, float> CategoryMult;      // Effectiveness of the payload damage/recovery against given entity categories.
    public List<string> TargetAttributesIgnored;        // Target attributes such as defense can be ignored. 
    public bool IgnoreShield;                           // True damage.

    public List<string> Flags;                          // Flags to customise the payload 

    public TagData Tag;                                 // An entity can be "tagged". This makes it possible for skills to affect this entity specifically without selecting it

    public List<(string StatusID, int Stacks)> ApplyStatus;             // Effects applied to the target entity along with the payload.
    public List<(string StatusID, int Stacks)> RemoveStatusStacks;      // Status stacks cleared from the target entity
    public List<(bool StatusGroup, string StatusID)> ClearStatus;       // Effects fully cleared from the target entity.

    public bool Instakill = false;                      // Used to set an entity's OnDeath trigger.
    public bool Revive = false;                         // Revives dead entities.
    public bool Interrupt = false;                      // If true, stops entity from casting skills. 

    public ActionMovement Movement;                     // Causes the entity to move in a given direction or toward a specified position.
    public bool StopCurrentMovement;                    // Stops previously applied movement.
    public ActionRotation Rotation;                     // Causes the entity to rotate toward a given direction.
    public bool StopCurrentRotation;                    // Stops previously applied rotation.

    public float SuccessChance;                         // Chance for a payload to be applied.

    public PayloadCondition PayloadCondition;           // Condition for the payload to be applied.
    public PayloadData AlternatePayload;                // If the condition isn't met, an alternate payload can be executed instead.

    public PayloadData()
    {
        Categories = new List<string>();
        PayloadValue = new Value();
        Flags = new List<string>();
        SuccessChance = 1.0f;
        CategoryMult = new Dictionary<string, float>();
        TargetAttributesIgnored = new List<string>();

        ApplyStatus = new List<(string StatusID, int Stacks)>();
        ClearStatus = new List<(bool StatusGroup, string StatusID)>();
        RemoveStatusStacks = new List<(string StatusID, int Stacks)>();
    }
}

public class PayloadCondition
{
    public enum ePayloadConditionType
    {
        AngleBetweenDirections,
        TargetHasStatus,
        TargetWithinDistance,
        TargetResourceRatioWithinRange,
    }

    public ePayloadConditionType ConditionType;

    // Status
    public string StatusID;
    public int MinStatusStacks;

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
            case ePayloadConditionType.TargetHasStatus:
            {
                StatusID = "";
                MinStatusStacks = 1;
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

    bool ConditionMet(Entity caster, Entity target)
    {
        switch (ConditionType)
        {
            case ePayloadConditionType.AngleBetweenDirections:
            {
                var dir1 = Direction(Direction1, caster, target);
                var dir2 = Direction(Direction2, caster, target);

                var value = Vector3.Angle(dir1, dir2);

                return CheckRange(value, Range.x, Range.y);
            }
            case ePayloadConditionType.TargetHasStatus:
            {
                return target.GetStatusEffectStacks(StatusID) > MinStatusStacks;
            }
            case ePayloadConditionType.TargetWithinDistance:
            {
                var value = (target.transform.position - caster.transform.position).sqrMagnitude;
                return CheckRange(value, (Range.x * Range.x), (Range.y * Range.y));
            }
            case ePayloadConditionType.TargetResourceRatioWithinRange:
            {
                var value = target.ResourcesCurrent.ContainsKey(Resource) ? target.ResourceRatio(Resource) : 0.0f;
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