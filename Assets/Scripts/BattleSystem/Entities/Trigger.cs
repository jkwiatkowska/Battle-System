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

    public Coroutine TryExecute(Entity entity, PayloadResult payloadResult, out bool usesLeft)
    {
        usesLeft = true;

        if (!ConditionsMet(payloadResult))
        {
            return null;
        }

        LastUsedTime = BattleSystem.Time;

        if (TriggerData.Limit != 0)
        {
            UsesLeft--;
            usesLeft = UsesLeft > 0;
        }

        return entity.StartCoroutine(TriggerData.Actions.ExecuteActions(entity, entity.Target));
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

        if (TriggerData.DepletablesAffected.Count > 0)
        {
            if (payloadResult == null || !TriggerData.DepletablesAffected.Contains(payloadResult.PayloadData.DepletableAffected))
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

        return true;
    }
}
