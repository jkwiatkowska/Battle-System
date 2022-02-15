using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger
{
    public TriggerData TriggerData;
    public float LastUsedTime;
    public float ExpireTime;        // If a trigger is only added temporarily, it will only work until this time. 
    public int UsesLeft;

    public Trigger(TriggerData triggerData, float expireTime = 0.0f)
    {
        TriggerData = triggerData;
        LastUsedTime = 0.0f - triggerData.Cooldown;
        UsesLeft = triggerData.Limit;
        ExpireTime = expireTime;
    }

    public Coroutine TryExecute(Entity entity, Entity target, PayloadResult payloadResult, out bool usesLeft)
    {
        usesLeft = true;

        if (!ConditionsMet(payloadResult))
        {
            return null;
        }

        return Execute(entity, target, out usesLeft);
    }

    public Coroutine Execute(Entity entity, Entity target, out bool usesLeft)
    {
        usesLeft = true;

        LastUsedTime = BattleSystem.Time;

        if (TriggerData.Limit != 0)
        {
            UsesLeft--;
            usesLeft = UsesLeft > 0;
        }

        return entity.StartCoroutine(TriggerData.Actions.ExecuteActions(entity, target));
    }

    public bool ConditionsMet(PayloadResult payloadResult)
    {
        if (ExpireTime != 0.0f && ExpireTime < BattleSystem.Time)
        {
            return false;
        }

        if (LastUsedTime + TriggerData.Cooldown > BattleSystem.Time)
        {
            return false;
        }

        if (TriggerData.SkillIDs.Count > 0)
        {
            if (!TriggerData.SkillIDs.Contains(payloadResult.SkillID))
            {
                return false;
            }
        }

        if (TriggerData.ResourcesAffected.Count > 0)
        {
            if (payloadResult == null || !TriggerData.ResourcesAffected.Contains(payloadResult.PayloadData.ResourceAffected))
            {
                return false;
            }
        }

        if (TriggerData.Flags.Count > 0)
        {
            if (payloadResult == null)
            {
                return false; 
            }

            foreach (var flag in TriggerData.Flags)
            {
                if (!payloadResult.Flags.Contains(flag))
                {
                    return false;
                }
            }
        }

        if (Random.value > TriggerData.TriggerChance)
        {
            return false;
        }

        return true;
    }
}
