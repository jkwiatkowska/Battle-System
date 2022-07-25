using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger
{
    public TriggerData TriggerData;
    public float LastUsedTime;
    public float ExpireTime;        // If a trigger is only added temporarily, it will only work until this time. 
    public int UsesLeft;
    public string TriggerKey;       // For triggers from status effects.

    public Trigger(TriggerData triggerData, float expireTime = 0.0f)
    {
        TriggerData = triggerData;
        LastUsedTime = 0.0f - triggerData.Cooldown;
        UsesLeft = triggerData.Limit;
        ExpireTime = expireTime;
    }

    public bool TryExecute(Entity entity, out bool usesLeft, Entity triggerSource = null, Payload payload = null, 
                           PayloadComponentResult payloadResult = null, Action action = null, ActionResult actionResult = null, 
                           Dictionary<string, ActionResult> actionResults = null, string statusName = "", 
                           TriggerData.eEntityAffected entityAffected = TriggerData.eEntityAffected.Self, string customIdentifier = "")
    {
        usesLeft = true;
        if (triggerSource == null)
        {
            triggerSource = entity;
        }

        if (TriggerData.Trigger == TriggerData.eTrigger.Custom && TriggerData.CustomIdentifier != customIdentifier)
        {
            return false;
        }

        if (!ConditionsMet(entity, triggerSource, payload, payloadResult, action, actionResult, actionResults, statusName, entityAffected))
        {
            return false;
        }

        var valueInfo = payload != null ? new ValueInfo(payload) : new ValueInfo(entity?.EntityInfo, triggerSource?.EntityInfo, actionResults, action?.ActionID);
        if (TriggerData.ValuesToSave != null && TriggerData.ValuesToSave.Count > 0)
        {
            foreach (var value in TriggerData.ValuesToSave)
            {
                value.Save(valueInfo);
            }
        }

        Execute(entity, triggerSource, valueInfo, out usesLeft);

        return true;
    }

    public void Execute(Entity entity, Entity triggerSource, ValueInfo valueInfo, out bool usesLeft)
    {
        usesLeft = true;

        if (TriggerData.TriggerChance != null)
        {
            var chance = TriggerData.TriggerChance.CalculateValue(valueInfo);
            Debug.Log($"Chance: {chance}");
            if (Random.value > chance)
            {
                return;
            }
            Debug.Log("Success");
        }

        LastUsedTime = BattleSystem.Time;

        if (TriggerData.Limit != 0)
        {
            UsesLeft--;
            usesLeft = UsesLeft > 0;
        }

        foreach (var reaction in TriggerData.TriggerReactions)
        {
            reaction.React(entity, triggerSource);
        }
    }

    public bool ConditionsMet(Entity entity, Entity triggerSource, Payload payloadInfo, PayloadComponentResult payloadResult, Action action, 
                              ActionResult actionResult, Dictionary<string, ActionResult> actionResults, string statusName, TriggerData.eEntityAffected entityAffected)
    {
        if (entityAffected != TriggerData.EntityAffected)
        {
            return false;
        }

        if (ExpireTime != 0.0f && ExpireTime < BattleSystem.Time)
        {
            return false;
        }

        if (LastUsedTime + TriggerData.Cooldown > BattleSystem.Time)
        {
            return false;
        }

        foreach (var condition in TriggerData.Conditions)
        {
            if (!condition.ConditionMet(entity, triggerSource, payloadInfo, payloadResult, action, actionResult, actionResults, statusName))
            {
                return false;
            }
        }

        return true;
    }
}
