using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMessage : Action
{
    public string MessageString;
    public Color MessageColor;

    public override void Execute(Entity entity, out ActionResult actionResult, Entity target)
    {
        actionResult = new ActionResult();

        if (!ConditionsMet(entity))
        {
            return;
        }

        var message = NamesAndText.MessageFromString(MessageString);
        MessageHUD.Instance.DisplayMessage(message, MessageColor);

        actionResult.Success = true;
        return;
    }
}
