using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload
{
    public Entity Source;
    public Action Action;
    public PayloadData PayloadData;
    public Value PayloadValue;
    public Dictionary<string, float> CasterAttributes;
    public string StatusID;
    
    public Payload(Entity caster, PayloadData payloadData, Action action, string statusID = null, Dictionary<string, ActionResult> actionResults = null)
    {
        Source = caster;
        Action = action;
        PayloadData = payloadData;
        StatusID = statusID;

        CasterAttributes = caster.EntityAttributes(Action.SkillID, Action.ActionID, statusID, PayloadData.Categories);
        PayloadValue = payloadData.PayloadValue.OutgoingValues(caster, CasterAttributes, actionResults);
    }

    public void ApplyPayload(Entity caster, Entity target, PayloadResult result)
    {
        if (PayloadData.Instakill)
        {
            target.OnDeath(caster, result);
            caster.OnKill(result);
        }
        
        result.Change = -PayloadValue.IncomingValue(target);

        // Incoming damage can be calculated using target attributes and other variables here.
        if (result.Change > Constants.Epsilon || result.Change < -Constants.Epsilon)
        {
            result.Change = Formulae.IncomingDamage(caster, target, result.Change, this, ref result.Flags);
            target.ApplyChangeToResource(PayloadData.ResourceAffected, result);
        }

        caster.OnPayloadApplied(result);
        target.OnPayloadReceived(result);

        // Only continue if the target is still alive
        if (!target.Alive)
        {
            return;
        }

        // Tag
        if (PayloadData.Tag != null)
        {
            caster.TagEntity(PayloadData.Tag.TagID, target, PayloadData.Tag);
        }

        // Status effects
        if (PayloadData.StatusEffects != null)
        {
            foreach (var status in PayloadData.StatusEffects)
            {
                target.ApplyStatusEffect(Action, status.Status, status.Stacks, this);
                caster.OnPayloadApplied(result);
            }
        }
    }
}
