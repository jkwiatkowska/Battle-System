using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectData
{
    public string StatusID;
    public List<EffectData> Effects;                // Effects applied when the required amount of stacks is aquired and removed when the status effect ends.
    public int MaxStacks;                           // Applying a status effect repeatedly can grant multiple stacks, up to this limit.

    public float Duration;                          // Infinite if 0.
    public float DurationIncreaseLimit;             // Status effect duration can be increased up to this limit. 0 means there is no limit.

    public bool MultipleInstances;                  // If true, multiple entities can apply the status effect without it getting overriden.

    public bool RemoveOnCasterDeath;                // If true, the status effect will remove itself when the caster dies.
                                                    // This should be set to true for effects that require present knowledge about the caster.

    public List<IntervalPayload> OnInterval;        // Payload applied at intervals.
    public PayloadData OnCleared;                   // Payload applied when a status effect is removed by an action.
    public PayloadData OnExpired;                   // Payload applied when a status effect is removed because of its timer running out.

    public StatusEffectData()
    {
        StatusID = "";
        Effects = new List<EffectData>();
        MaxStacks = 1;
        Duration = 0.0f;
        OnInterval = new List<IntervalPayload>();
        RemoveOnCasterDeath = true;
    }

    public StatusEffectData(string statusID):this()
    {
        StatusID = statusID;
    }
}

public class IntervalPayload
{
    public PayloadData Payload;
    public float Interval;
    public float Delay;

    public IntervalPayload()
    {
        Payload = new PayloadData();
        Interval = 1.0f;
        Delay = 1.0f;
    }
}
