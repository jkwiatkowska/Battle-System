using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionDestroy : Action
{
    public enum eTarget
    {
        Self,
        SelectedEntity
    }

    public eTarget EntityToDestroy;

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, target, actionResults))
        {
            return;
        }

        switch(EntityToDestroy)
        {
            case eTarget.Self:
            {
                entity.DestroyEntity();
                break;
            }
            case eTarget.SelectedEntity:
            {
                target.DestroyEntity();
                break;
            }
        }
        
        actionResults[ActionID].Success = true;
    }

    public override void SetTypeDefaults()
    {
        
    }
}
