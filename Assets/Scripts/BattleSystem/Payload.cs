using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload
{
    public Entity Source;
    public ActionPayload Action;
    public Value PayloadValue;

    // To do: status effects

    public Payload(Entity caster, ActionPayload action, Dictionary<string, ActionResult> actionResults)
    {
        Source = caster;
        Action = action;

        PayloadValue = action.PayloadData.PayloadValue.OutgoingValues(caster, actionResults);
    }

    public void ApplyPayload(Entity caster, Entity target, PayloadResult result)
    {
        result.Change = -PayloadValue.IncomingValue(target);

        // Incoming damage can be calculated using target attributes and other variables here.
        if (result.Change > Constants.Epsilon || result.Change < -Constants.Epsilon)
        {
            result.Change = Formulae.IncomingDamage(caster, target, result.Change, Action.PayloadData, ref result.Flags);
            target.ApplyChangeToDepletable(Action.PayloadData.DepletableAffected, result);
        }

        // Only perform other actions if the target is still alive
        if (target.Alive)
        {
            if (Action.PayloadData.Tag != null)
            {
                caster.TagEntity(Action.PayloadData.Tag, target);
            }

            if (Action.PayloadData.Triggers != null)
            {
                foreach (var trigger in Action.PayloadData.Triggers)
                {
                    target.OnTrigger(trigger, caster, result);
                }
            }
        }
    }
}
