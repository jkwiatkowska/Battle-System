using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectCostAction : SkillActionData
{
    public string Type;                     // One of the depletable attributes defined in game data.
    public float Value;                          // How much is depleted.
    public bool Optional;                       // If optional, the skill can be executed and continue without taking the cost. 
                                                // Can be used to change how skill works depending on whether the cost condition is met.

    public override bool NeedsTarget()
    {
        return false;
    }
}
