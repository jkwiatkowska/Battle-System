using System.Collections.Generic;
using UnityEngine;

// This class compiles information about a payload being applied, its source and the involved entities and uses it to apply the payload to target entity. 
public class Payload
{
    public EntityInfo Caster;                               // Entity that applied the payload. 
    public EntityInfo Target;                               // Entity affected by the payload.

    public PayloadData PayloadData;                         // Payload being applied
    public Payload AlternatePayload;                        // Alternate payload, if one exists.

    public ActionPayload Action;                            // (Optional) Action that triggered the payload.
    public Dictionary<string, ActionResult> ActionResults;  // (Optional) Results of previously executed actions.

    public string SourceStatusEffect;                       // (Optional) Status effect that applied the payload.

    public Payload()
    {

    }

    public Payload(Entity caster, PayloadData payloadData, Payload payload, string statusID)
                : this(caster, payloadData, payload?.Action, payload?.ActionResults, statusID)
    {
    }

    public Payload(Entity caster, PayloadData payloadData, ActionPayload action, Dictionary<string, ActionResult> actionResults, string statusID) 
        : this(new EntityInfo(caster, caster.EntityAttributes(payloadData, action, statusID)), payloadData, action, actionResults, statusID)
    {
    }

    public Payload(EntityInfo caster, PayloadData payloadData, ActionPayload action, Dictionary<string, ActionResult> actionResults, string statusID)
    {
        Caster = caster;

        Action = action;
        ActionResults = actionResults;

        PayloadData = payloadData;

        SourceStatusEffect = statusID;

        if (PayloadData.AlternatePayload != null)
        {
            AlternatePayload = new Payload(caster, PayloadData.AlternatePayload, action, actionResults, statusID);
        }
    }

    public bool ApplyPayload(Entity target, out List<PayloadComponentResult> payloadResults)
    {
        payloadResults = new List<PayloadComponentResult>();

        var caster = Caster.Entity;
        if (target == null || caster == null)
        {
            return false;
        }

        var payloadToApply = this;
        payloadToApply.Target = new EntityInfo(target, payloadToApply.PayloadData);

        // A payload can be applied if it has no conditions or conditions are met.
        var canApplyPayload = payloadToApply.PayloadData.PayloadCondition == null || payloadToApply.PayloadData.PayloadCondition.CheckCondition(Caster, Target);

        // If conditions aren't met, but an alternate payload exist, check the alternate payload's conditions.
        // Keep going until a payload that can be applied is found or all options are exhausted.
        while (!canApplyPayload)
        {
            payloadToApply = payloadToApply.AlternatePayload;
            if (payloadToApply == null)
            {
                return false;
            }

            payloadToApply.Target = new EntityInfo(target, payloadToApply.PayloadData);
            canApplyPayload = payloadToApply.PayloadData.PayloadCondition == null || payloadToApply.PayloadData.PayloadCondition.CheckCondition(Caster, Target);
        }

        // Immunities
        if (Action != null)
        {
            var immunity = target.HasImmunityAgainstAction(Action);
            if (immunity != null)
            {
                target.OnImmune(caster, this);
                return false;
            }
        }

        foreach (var category in payloadToApply.PayloadData.Categories)
        {
            var catImmunity = target.HasImmunityAgainstCategory(category);
            if (catImmunity != null)
            {
                target.OnImmune(caster, this);
                return false;
            }
        }

        // Apply payload components.
        foreach (var component in payloadToApply.PayloadData.Components)
        {
            payloadResults.Add(component.ApplyComponent(payloadToApply));
        }

        return true;
    }
}