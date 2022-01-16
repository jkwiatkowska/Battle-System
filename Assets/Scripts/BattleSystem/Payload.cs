using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload
{
    public Entity Source;
    public ActionPayload Action;
    public List<PayloadData.PayloadComponent> PayloadDamage;

    // To do: status effects

    public Payload(Entity caster, ActionPayload action)
    {
        Source = caster;
        Action = action;

        var change = 0.0f;

        // Go through the payload data and add up all values that can be derived from the caster.
        // Values dependent on the target are added when incoming damage is calculated.
        PayloadDamage = new List<PayloadData.PayloadComponent>();
        foreach (var component in Action.PayloadData.PayloadComponents)
        {
            switch (component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    change += component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterAttribute:
                {
                    if (!caster.BaseAttributes.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{Action.ActionID}] payload. It should be an entity attribute name.");
                    }
                    change += component.Potency * caster.BaseAttributes[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableMax:
                {
                    if (!caster.DepletablesMax.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{Action.ActionID}] payload. The attribute should be a name of a depletable value.");
                    }
                    change += component.Potency * caster.DepletablesMax[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableCurrent:
                {
                    if (!caster.DepletablesCurrent.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{Action.ActionID}] payload. The attribute should be a name of a depletable value.");
                    }
                    change += component.Potency * caster.DepletablesCurrent[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    PayloadDamage.Add(component);
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    PayloadDamage.Add(component);
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.ActionResultValue:
                {
                    if (caster.ActionResults.ContainsKey(component.Attribute))
                    {
                        change -= caster.ActionResults[component.Attribute].Value;
                    }
                    else
                    {
                        Debug.LogError($"Action {action.ActionID} requires an action result for action {component.Attribute}, but it can't be found.");
                    }
                    break;
                }
                default:
                {
                    Debug.LogError($"Unsupported payload component type: {component.ComponentType}");
                    break;
                }
            }
        }

        // Damage can be further adjusted here, for example by applying multipliers
        var totalDamage = Formulae.OutgoingDamage(caster, change, Action.PayloadData);

        PayloadDamage.Add(new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, totalDamage));
    }

    public void ApplyPayload(Entity caster, Entity target, PayloadResult result)
    {
        // Go through the payload and calculate any damage that's dependent on target.
        foreach (var component in PayloadDamage)
        {
            switch (component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    result.Change -= component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    result.Change -= component.Potency * target.DepletablesCurrent[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    result.Change -= component.Potency * target.DepletablesMax[component.Attribute];
                    break;
                }
                default:
                {
                    Debug.LogError($"Error when applying payload for action {Action.ActionID}, payload component was not converted to flat damage in DamageFormulae.OutgoingPayload().");
                    break;
                }
            }
        }

        // Incoming damage can be calculated using target attributes and other variables here. 
        result.Change = Formulae.IncomingDamage(caster, target, result.Change, Action.PayloadData, ref result.Flags);

        target.ApplyChangeToDepletable(Action.PayloadData.DepletableAffected, result);

        // Only perform other actions if the target is still alive
        if (target.Alive)
        {
            if (!string.IsNullOrEmpty(Action.PayloadData.Tag))
            {

            }
        }
    }
}
