using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMessage : Action
{
    public string MessageString;
    public Color MessageColor;

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, target, actionResults))
        {
            return;
        }

        var message = NamesAndText.MessageFromString(MessageString);
        MessageHUD.Instance.DisplayMessage(message, MessageColor);

        actionResults[ActionID].Success = true;
        return;
    }

    public override void SetTypeDefaults()
    {
        MessageString = "";
        MessageColor = Color.white;
    }
}
