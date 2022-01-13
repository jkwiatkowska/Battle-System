using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDestroySelf : Action
{
    public override void Execute(Entity entity, out ActionResult actionResult)
    {
        actionResult = new ActionResult();
        entity.DestroyEntity();
        actionResult.Success = true;
    }

    public override bool NeedsTarget()
    {
        return false;
    }
}
