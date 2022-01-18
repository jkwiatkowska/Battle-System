using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionTimeline : List<Action>
{
    public IEnumerator ExecuteActions(Entity entity, Entity target)
    {
        var startTime = BattleSystem.Time;
        entity.ActionResults.Clear();

        foreach (var action in this)
        {
            var timestamp = startTime + action.TimestampForEntity(entity);
            while (timestamp > BattleSystem.Time)
            {
                yield return null;
            }

            action.Execute(entity, out var actionResult, target);
            entity.ActionResults[action.ActionID] = actionResult;
        }
        yield return null;
    }
}
