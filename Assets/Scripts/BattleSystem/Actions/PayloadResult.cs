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
    public string ResourceChanged;
    public List<string> Flags;

    public PayloadResult(PayloadData payloadData, Entity caster, Entity target, string skillID = "", string actionID = "")
    {
        PayloadData = payloadData;

        Caster = caster;
        Target = target;
        Change = 0.0f;
        Flags = new List<string>();

        SkillID = skillID;
        ActionID = actionID;
    }
}
