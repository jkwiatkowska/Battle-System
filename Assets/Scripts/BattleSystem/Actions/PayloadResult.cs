using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadResult
{
    public PayloadData PayloadData;
    public Entity Caster;
    public Entity Target;

    public string SkillID;
    public string ActionID;
    public float Change;
    public List<string> Flags;

    public PayloadResult(PayloadData payloadData, Entity caster, Entity target, string skillID, string actionID, float change, List<string> flags)
    {
        PayloadData = payloadData;
        Caster = caster;
        Target = target;
        SkillID = skillID;
        ActionID = actionID;
        Change = change;
        Flags = flags;
    }
}
