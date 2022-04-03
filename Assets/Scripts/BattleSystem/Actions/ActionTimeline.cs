using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTimeline : List<Action>
{
    public IEnumerator ExecuteActions(Entity entity, Entity target)
    {
        var startTime = BattleSystem.Time;
        var actionResults = new Dictionary<string, ActionResult>();

        for (int i = 0; i < Count; i++)
        {
            // Wait until the action timestamp, then execute it.
            var action = this[i];

            var timestamp = startTime + action.TimestampForEntity(entity);
            while (timestamp > BattleSystem.Time)
            {
                yield return null;
            }

            action.Execute(entity, target, ref actionResults);

            // Some actions affect the timeline on success.
            if (actionResults[action.ActionID].Success)
            {
                if (action.ActionType == Action.eActionType.LoopBack)
                {
                    if (action is ActionLoopBack loopBackAction)
                    {
                        var goToTimestamp = startTime + Formulae.ActionTime(entity, loopBackAction.GoToTimestamp);
                        var difference = BattleSystem.Time - goToTimestamp;

                        startTime += difference;

                        while (i >= 0 && this[i].Timestamp >= loopBackAction.GoToTimestamp)
                        {
                            i--;
                        }
                    }
                    else
                    {
                        Debug.LogError($"Action {action.ActionID} is not a loop back action.");
                    }
                }
                else if (action.ActionType == Action.eActionType.Cancel)
                {
                    yield break;
                }
            }
        }
    }
}
