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

    public enum eTargetState
    {
        Alive,
        Dead,
        Any
    }

    public eTarget Target;              // Which entities the payload affects
    public eTargetState TargetState;    // What state the target has to be in to be affected
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

        var payload = new Payload(entity, this);
        foreach (var target in targets)
        {
            // If payload isn't guaranteed to trigger.
            var chance = Formulae.PayloadSuccessChance(Payload, entity, target);
            if (Random.value > chance)
            {
                continue;
            }

            // Apply payload and update result.
            var change = payload.ApplyPayload(entity, target);
            actionResult.Value += change;
            actionResult.Count += 1;

            // Show damage number on HUD
            if (change != 0.0f)
            {
                HUDPopupTextDisplay.Instance.DisplayDamage(target, this, -change);
            }
        }

        actionResult.Success = actionResult.Count > 0;
    }

    public abstract List<Entity> GetTargetsForAction(Entity entity);

    protected bool CheckTargetableState(Entity target)
    {
        if (TargetState == eTargetState.Any)
        {
            return true;
        }

        if (TargetState == eTargetState.Alive)
        {
            return target.Alive;
        }

        if (TargetState == eTargetState.Dead)
        {
            return !target.Alive;
        }

        Debug.LogError($"Unsupported target state: {TargetState}");
        return false;
    }
}
