using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload
{
    public Entity Source;
    public ActionPayload Action;
    public PayloadData PayloadData;
    public Value PayloadValue;
    
    public Payload(Entity caster, PayloadData payloadData, ActionPayload action = null, Dictionary<string, ActionResult> actionResults = null)
    {
        Source = caster;
        Action = action;
        PayloadData = payloadData;

        var casterAttributes = caster.EntityAttributes(Action.SkillID, Action.ActionID, PayloadData.Categories);
        PayloadValue = payloadData.PayloadValue.OutgoingValues(caster, casterAttributes, actionResults);
    }

    public void ApplyPayload(Entity caster, Entity target, PayloadResult result)
    {
        result.Change = -PayloadValue.IncomingValue(target);

        // Incoming damage can be calculated using target attributes and other variables here.
        if (result.Change > Constants.Epsilon || result.Change < -Constants.Epsilon)
        {
            result.Change = Formulae.IncomingDamage(caster, target, result.Change, this, ref result.Flags);
            target.ApplyChangeToResource(PayloadData.ResourceAffected, result);
        }

        // Only perform other actions if the target is still alive
        if (target.Alive)
        {
            if (PayloadData.Tag != null)
            {
                caster.TagEntity(PayloadData.Tag, target);
            }

            if (PayloadData.Triggers != null)
            {
                foreach (var trigger in PayloadData.Triggers)
                {
                    target.OnTrigger(trigger, caster, result);
                }
            }
        }
    }
}
