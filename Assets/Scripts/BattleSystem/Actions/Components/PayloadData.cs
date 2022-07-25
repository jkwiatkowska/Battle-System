using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadData
{
    public List<string> Categories;                     // Can define what kind of payload this is, affected by damage bonuses and resistances
    public List<PayloadComponent> Components;           // Effects of applying a payload.

    public PayloadCondition PayloadCondition;           // Condition for the payload to be applied.
    public PayloadData AlternatePayload;                // If the condition isn't met, an alternate payload can be executed instead.

    public PayloadData()
    {
        Categories = new List<string>();
        Components = new List<PayloadComponent>();
    }
}