using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSaveTransform : Action
{
    public string TransformID;
    public TransformData Transform;

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, target, actionResults))
        {
            return;
        }

        actionResults[ActionID].Success = Transform.TryGetTransformFromData(entity, target, out var pos, out var forward);

        if (actionResults[ActionID].Success)
        {
            entity.SaveTransform(TransformID, pos, forward);
        }
        entity.OnActionUsed(this, actionResults[ActionID], actionResults);
    }

    public override void SetTypeDefaults()
    {
        throw new System.NotImplementedException();
    }
}
