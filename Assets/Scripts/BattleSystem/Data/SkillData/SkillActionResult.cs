using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When a skill action is performed, it returns a result which can be used by other actions.
public class SkillActionResult
{
    public SkillActionData ActionData;
    public bool Success;
    public float Value;

    public SkillActionResult(SkillActionData actionData)
    {
        ActionData = actionData;
        Success = false;
        Value = 0.0f;
    }
}
