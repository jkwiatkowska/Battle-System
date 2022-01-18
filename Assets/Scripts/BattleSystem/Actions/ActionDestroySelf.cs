using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDestroySelf : Action
{
    public override void Execute(Entity entity, out ActionResult actionResult, Entity target)
    {
        actionResult = new ActionResult();
        entity.DestroyEntity();
        actionResult.Success = true;
    }
}
