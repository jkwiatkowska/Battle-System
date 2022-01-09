using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionPayload : Action
{
    public enum eTarget
    {
        Self,
        EnemyEntities,
        FriendlyEntities
    }

    public eTarget Target;      // Which entities the payload affects
    public PayloadData Payload;

    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();

        if (!ConditionMet(entity))
        {
            return;
        }

        var targets = GetTargetsForAction(entity);

        if (targets.Count == 0)
        {
            return;
        }
    }

    public abstract List<Entity> GetTargetsForAction(Entity entity);
}
