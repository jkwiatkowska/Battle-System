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
        foreach (var component in Action.Payload.PayloadComponents)
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
            }
        }

        // Damage can be further adjusted here, for example by applying multipliers
        var totalDamage = Formulae.OutgoingDamage(caster, change, Action.Payload);

        PayloadDamage.Add(new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, totalDamage));
    }

    public float ApplyPayload(Entity caster, Entity target)
    {
        float change = 0.0f;

        // Go through the payload and calculate any damage that's dependent on target.
        foreach (var component in PayloadDamage)
        {
            switch (component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    change -= component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    change -= component.Potency * target.DepletablesCurrent[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    change -= component.Potency * target.DepletablesMax[component.Attribute];
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
        float totalChange = Formulae.IncomingDamage(caster, target, change, Action.Payload);

        target.ApplyChangeToDepletable(Action.Payload.DepletableAffected, totalChange);

        return totalChange;
    }
}
