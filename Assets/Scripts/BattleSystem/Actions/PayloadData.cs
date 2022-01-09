using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadData
{
    public class PayloadComponent
    {
        public enum ePayloadComponentType
        {
            FlatValue,
            CasterAttribute,
            CasterDepletableMax,
            CasterDepletableCurrent,
            TargetDepletableMax,
            TargetDepletableCurrent,
            ActionResultValue
        }
        public ePayloadComponentType ComponentType;

        public float Potency;                           // Multiplier
        public string Attribute;                        // The payload can scale off different attributes, depletables or action results (for example damage dealt).

        public PayloadComponent(ePayloadComponentType type, float potency, string attribute = "")
        {
            ComponentType = type;
            Potency = potency;
            Attribute = attribute;
        }
    }

    public List<string> Affinities;                     // Can define what kind of payload this is, affected by damage bonuses and resistances

    public List<PayloadComponent> PayloadComponents;    // The components are added together. Example: 20% TargetMaxHP + 1000 (FlatValue)
    public string DepletableAffected;                   // Typically hp, but can be other things like energy

    public Dictionary<string, bool> Flags;              // Flags to customise the payload 

    public bool TagEntity;                              // An entity can be "tagged". This makes it possible for skills to affect this entity specifically without selecting it
    public string Tag;                                  // Identifier for tagged entities. 
    public int MaxTags;                                 // Replaces a tagged entity if this limit is exceeded 

    public float SuccessChance;

    // TO DO:
    // - Status effects (DoT, HoT, buff, debuff, passive effect, stun, apply skill, grant immunity), apply and remove lists by status name
    // - Force applied
    // - Consider conditional passives that apply to payload

    public PayloadData(List<string> affinities, List<PayloadComponent> payloadComponents, Dictionary<string, bool> flags, float successChance = 1.0f)
    {
        Affinities = affinities;
        PayloadComponents = payloadComponents;
        Flags = flags;
        SuccessChance = successChance;
    }
}
