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

    public Coroutine TryExecute(Entity entity, out bool usesLeft, Entity triggerSource = null, PayloadResult payloadResult = null, 
                                Action action = null, ActionResult actionResult = null, string statusName = "")
    {
        usesLeft = true;

        if (!ConditionsMet(entity, triggerSource, payloadResult, action, actionResult, statusName))
        {
            return null;
        }

        return Execute(entity, triggerSource, out usesLeft);
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

    public bool ConditionsMet(Entity entity, Entity triggerSource, PayloadResult payloadResult, Action action, ActionResult actionResult, string statusName)
    {
        if (ExpireTime != 0.0f && ExpireTime < BattleSystem.Time)
        {
            return false;
        }

        if (LastUsedTime + TriggerData.Cooldown > BattleSystem.Time)
        {
            return false;
        }

        if (Random.value > TriggerData.TriggerChance)
        {
            return false;
        }

        foreach (var condition in TriggerData.Conditions)
        {
            if (!condition.ConditionMet(entity, triggerSource, payloadResult, action, actionResult, statusName))
            {
                return false;
            }
        }

        return true;
    }
}
