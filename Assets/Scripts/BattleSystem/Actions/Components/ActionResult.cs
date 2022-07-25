using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// When a skill action is performed, it returns a result which can be used by other actions.
public class ActionResult
{
    public bool Success;
    public Dictionary<string, float> Values; // Recorded action result values, such as damage or recovery applied.
                                             // For payload results, the keys correspond to payload categories.
                                             // For cost collection results, the key is "cost". 
                                             // For other actiont types, the key is blank.
    public int Count;

    public ActionResult()
    {
        Success = false;
        Values = new Dictionary<string, float>();
        Count = 0;
    }
}
