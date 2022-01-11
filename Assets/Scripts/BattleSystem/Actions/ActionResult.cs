using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When a skill action is performed, it returns a result which can be used by other actions.
public class ActionResult
{
    public bool Success;
    public float Value;
    public int Count;

    public ActionResult()
    {
        Success = false;
        Value = 0.0f;
        Count = 0;
    }
}
