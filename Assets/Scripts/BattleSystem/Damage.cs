using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Damage
{
    public static Payload GetPayloadFromData(Entity caster, PayloadData payloadData, string skillID)
    {
        var payload = new Payload(caster, skillID);

        var rawDamage = 0.0f;

        // Go through the payload data and add up all values that can be derived from the caster.
        // Values dependent on the target are added when incoming damage is calculated.
        foreach (var component in payloadData.PayloadComponents)
        {
            switch (component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    rawDamage += component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterAttribute:
                {
                    if (!caster.Attributes.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{skillID}] payload. It should be an entity attribute name.");
                    }
                    rawDamage += component.Potency * caster.Attributes[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableMax:
                {
                    if (!caster.DepletablesMax.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{skillID}] payload. The attribute should be a name of a depletable value.");
                    }
                    rawDamage += component.Potency * caster.DepletablesMax[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableCurrent:
                {
                    if (!caster.DepletablesCurrent.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{skillID}] payload. The attribute should be a name of a depletable value.");
                    }
                    rawDamage += component.Potency * caster.DepletablesCurrent[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    payload.OutgoingDamage.Add(component);
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    payload.OutgoingDamage.Add(component);
                    break;
                }
            }
        }

        // Damage can be further adjusted here, for example by applying multipliers
        var totalDamage = rawDamage;

        payload.OutgoingDamage.Add(new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, totalDamage, ""));

        return payload;
    }

    public static float GetDamageFromPayload(Payload payload, Entity target)
    {
        float incomingDamage = 0.0f;

        // Go through the payload and calculate any damage that's dependent on target.
        foreach (var component in payload.OutgoingDamage)
        {
            switch (component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    incomingDamage += component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    incomingDamage += component.Potency * target.DepletablesCurrent[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    incomingDamage += component.Potency * target.DepletablesMax[component.Attribute];
                    break;
                }
                default:
                {
                    Debug.LogError($"Error when applying payload for skill {payload.SkillID}, payload component was not converted to flat damage in DamageFormulae.OutgoingPayload().");
                    break;
                }
            }
        }

        // Incoming damage can be calculated using target attributes and other variables here. 
        float totalDamage = incomingDamage;

        return totalDamage;
    }
}
