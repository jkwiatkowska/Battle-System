using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSummon : Action
{
    public string EntityID;

    public TransformData SummonAtPosition;

    public float SummonDuration;                        // 0 if infinite
    public Dictionary<string, float> SharedAttributes;  // Summoned entity can inherit the caster's attributes
                                                        // The float value is a multiplier
                                                        // (for example an entry of {atk, 0.5} means the entity has half of the caster's atk attribute)

    public bool LifeLink;                               // If true, the entity will disappear when the caster dies

    public override void Execute(Entity entity, out ActionResult actionResult, Entity target)
    {
        actionResult = new ActionResult();

        if (!ConditionsMet(entity))
        {
            return;
        }
    }
}
