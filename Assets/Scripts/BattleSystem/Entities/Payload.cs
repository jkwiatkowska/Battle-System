using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Payload
{
    public Entity Source;
    public string SkillID;
    public List<PayloadData.PayloadComponent> OutgoingDamage;

    // To do: status effects

    public Payload(Entity caster, string skillID)
    {
        Source = caster;
        SkillID = skillID;
        OutgoingDamage = new List<PayloadData.PayloadComponent>();
    }
    public Payload(Entity caster, PayloadData payloadData, string skillID)
    {
        Source = caster;
        SkillID = skillID;

        OutgoingDamage = new List<PayloadData.PayloadComponent>();
        var damage = 0.0f;
        foreach (var component in payloadData.PayloadComponents)
        {
            switch(component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    damage += component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterAttribute:
                {
                    if (!caster.Attributes.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{skillID}] payload. It should be an entity attribute name.");
                    }
                    damage += component.Potency * caster.Attributes[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableMax:
                {
                    if (!caster.DepletablesMax.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{skillID}] payload. The attribute should be a name of a depletable value.");
                    }
                    damage += component.Potency * caster.DepletablesMax[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.CasterDepletableCurrent:
                {
                    if (!caster.DepletablesCurrent.ContainsKey(component.Attribute))
                    {
                        Debug.LogError($"Invalid attribute [{component.Attribute}] for [{skillID}] payload. The attribute should be a name of a depletable value.");
                    }
                    damage += component.Potency * caster.DepletablesCurrent[component.Attribute];
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    OutgoingDamage.Add(component);
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    OutgoingDamage.Add(component);
                    break;
                }
            }
            OutgoingDamage.Add(new PayloadData.PayloadComponent(PayloadData.PayloadComponent.ePayloadComponentType.FlatValue, damage, ""));
        }
    }
}
