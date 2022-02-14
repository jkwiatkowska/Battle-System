using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadData
{
    public List<string> Categories;                     // Can define what kind of payload this is, affected by damage bonuses and resistances

    public Value PayloadValue;                          // The value components are added together. Example: 20% TargetMaxHP + 1000 (FlatValue)
    public string ResourceAffected;                   // Typically hp, but can be other things like energy

    public Dictionary<string, bool> Flags;              // Flags to customise the payload 

    public List<StatusEffectData> StatusEffects;        // Effects applied to the target entity along with the payload.
    public TagData Tag;                                 // An entity can be "tagged". This makes it possible for skills to affect this entity specifically without selecting it

    public List<TriggerData.eTrigger> Triggers;         // Can be used to force trigger reactions in target, such as death without taking damage.

    public float SuccessChance;

    // TO DO:
    // - Status effects (DoT, HoT, buff, debuff, passive effect, stun, apply skill, grant immunity), apply and remove lists by status name
    // - Force applied
    // - Consider conditional passives that apply to payload

    public PayloadData()
    {
        Categories = new List<string>();
        PayloadValue = new Value();
        Flags = new Dictionary<string, bool>();
        SuccessChance = 1.0f;
        Triggers = new List<TriggerData.eTrigger>();
    }
}
