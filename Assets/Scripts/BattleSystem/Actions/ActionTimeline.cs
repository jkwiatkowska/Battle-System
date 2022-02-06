using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTimeline : List<Action>
{
    public IEnumerator ExecuteActions(Entity entity, Entity target)
    {
        Debug.Log("Boop");
        var startTime = BattleSystem.Time;
        var actionResults = new Dictionary<string, ActionResult>();

        foreach (var action in this)
        {
            Debug.Log($"Executing action {action.ActionID}, time: {BattleSystem.Time}");
            var timestamp = startTime + action.TimestampForEntity(entity);
            while (timestamp > BattleSystem.Time)
            {
                yield return null;
            }

            action.Execute(entity, target, ref actionResults);
        }
        yield return null;
    }
}
