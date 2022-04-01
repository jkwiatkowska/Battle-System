using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload
{
    public Entity Source;
    public Action Action;
    public PayloadData PayloadData;
    public Value PayloadValue;
    public Value PayloadValueMax;
    public Dictionary<string, float> CasterAttributes;
    public string StatusID;
    
    public Payload(Entity caster, PayloadData payloadData, Action action, string statusID = null, Dictionary<string, ActionResult> actionResults = null)
    {
        Source = caster;
        Action = action;
        PayloadData = payloadData;
        StatusID = statusID;

        CasterAttributes = Action != null ? caster.EntityAttributes(Action.SkillID, Action.ActionID, statusID, PayloadData.Categories) :
                           caster.EntityAttributes();
        PayloadValue = payloadData.PayloadValue.OutgoingValues(caster, CasterAttributes, actionResults);
        if (payloadData.PayloadValueMax != null && payloadData.PayloadValueMax.Count > 0)
        {
            PayloadValueMax = payloadData.PayloadValueMax.OutgoingValues(caster, CasterAttributes, actionResults);
        }
    }

    public bool ApplyPayload(Entity caster, Entity target, PayloadResult result)
    {
        if (PayloadData.Revive && !target.Alive)
        {
            target.OnReviveIncoming(result);
        }

        // Chance
        var chance = Formulae.PayloadSuccessChance(PayloadData, caster, target);
        if (Random.value > chance)
        {
            caster.OnHitMissed(target, result);
            return false;
        }

        // Immunity
        foreach (var category in PayloadData.Categories)
        {
            var immunity = target.HasImmunityAgainstCategory(category);
            if (immunity != null)
            {
                target.OnImmune(caster, result);
                return false;
            }
        }

        // Movement
        if (PayloadData.Rotation != null)
        {

        }

        if (PayloadData.Movement != null)
        {
            target.Movement.InitiateMovement(PayloadData.Movement, caster, target);
        }

        // Instant death
        if (PayloadData.Instakill)
        {
            target.OnDeath(caster, result);
        }

        // Category multiplier
        var targetData = target.EntityData;
        var categoryMultiplier = 1.0f;
        if (PayloadData.CategoryMult != null && targetData.Categories != null)
        {
            foreach (var cat in PayloadData.CategoryMult)
            {
                if (targetData.Categories.Contains(cat.Key))
                {
                    categoryMultiplier *= cat.Value;
                }
            }
        }

        // Reverse the change
        result.Change = -PayloadValue.IncomingValue(target, target.EntityAttributes(), PayloadValueMax);

        // Incoming damage can be calculated using target attributes and other variables here.
        if (result.Change > Constants.Epsilon || result.Change < -Constants.Epsilon)
        {
            result.Change = Formulae.IncomingDamage(caster, target, result.Change, this, ref result.Flags) * categoryMultiplier;
            target.ApplyChangeToResource(PayloadData.ResourceAffected, result);
        }

        caster.OnPayloadApplied(result);
        target.OnPayloadReceived(result);

        // Only continue if the target is still alive
        if (!target.Alive)
        {
            return true;
        }

        // Aggro
        if (PayloadData.Aggro != null)
        {
            var mult = PayloadData.MultiplyAggroByPayloadValue ? result.Change : 1.0f;
            target.EntityBattle.ChangeAggro(caster, PayloadData.Aggro.GetAggroChange(caster, caster.EntityUID, target, mult));
        }

        // Tag
        if (PayloadData.Tag != null)
        {
            caster.TagEntity(PayloadData.Tag.TagID, target, PayloadData.Tag);
        }

        // Status effects
        if (PayloadData.ApplyStatus != null)
        {
            foreach (var status in PayloadData.ApplyStatus)
            {
                var immunity = target.HasImmunityAgainstStatus(status.StatusID);
                if (immunity != null)
                {
                    continue;
                }
                target.ApplyStatusEffect(caster, status.StatusID, status.Stacks, Action, this);
            }
        }

        if (PayloadData.ClearStatus != null)
        {
            foreach (var status in PayloadData.ClearStatus)
            {
                if (status.StatusGroup)
                {
                    if (BattleData.StatusEffectGroups.ContainsKey(status.StatusID))
                    {
                        foreach (var s in BattleData.StatusEffectGroups[status.StatusID])
                        {
                            target.ClearStatusEffect(caster, s);
                        }
                    }
                }
                else
                {
                    target.ClearStatusEffect(caster, status.StatusID);
                }
            }
        }

        if (PayloadData.RemoveStatusStacks != null)
        {
            foreach (var status in PayloadData.RemoveStatusStacks)
            {
                target.RemoveStatusEffectStacks(caster, status.StatusID, status.Stacks);
            }
        }

        return true;
    }
}
