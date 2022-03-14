using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectData
{
    public string StatusID;
    public List<Effect> Effects;                    // Effects applied when the required amount of stacks is aquired and removed when the status effect ends.
    public int MaxStacks;                           // Applying a status effect repeatedly can grant multiple stacks, up to this limit.
    public float Duration;                          // Infinite if 0.

    public List<(PayloadData PayloadData, int Stacks)> OnStacksGained;  // Payload applied when stacks are applied and the specified number of stacks is reached.
    public List<(PayloadData PayloadData, float Interval)> OnInterval;  // Payload applied every n seconds.
    public PayloadData OnCleared;                                       // Payload applied when a status effect is removed by an action.
    public PayloadData OnExpired;                                       // Payload applied when a status effect is removed because of its timer running out.

    public StatusEffectData()
    {
        StatusID = "";
        Effects = new List<Effect>();
        MaxStacks = 1;
        Duration = 0.0f;
        OnStacksGained = new List<(PayloadData PayloadData, int Stacks)>();
        OnInterval = new List<(PayloadData PayloadData, float Interval)>();
    }

    public StatusEffectData(string statusID):this()
    {
        StatusID = statusID;
    }
}
