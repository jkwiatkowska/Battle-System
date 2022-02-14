using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    StatusEffectData Data;
    int CurrentStacks;
    Entity Target;
    Entity Caster;
    float StartTime;
    Payload SourcePayload;

    public List<(Payload, float)> OnInterval;
    public List<(Payload, int)> OnStacksGained;
    public Payload OnRemoved;
    public Payload OnExpired;

    public StatusEffect (Entity target, Entity caster, StatusEffectData statusEffectData, Payload payload)
    {
        Data = statusEffectData;
        CurrentStacks = 0;
        Target = target;
        Caster = caster;
        StartTime = BattleSystem.Time;
        SourcePayload = payload;

        ApplyStacks();
    }

    void UpdatePayloads()
    {
        OnInterval = new List<(Payload, float)>();
        foreach (var payload in Data.OnInterval)
        {
            OnInterval.Add((new Payload(Caster, payload.Item1), StartTime + payload.Item2));
        }

        OnStacksGained = new List<(Payload, int)>();
        foreach (var payload in Data.OnStacksGained)
        {
            OnStacksGained.Add((new Payload(Caster, payload.Item1), payload.Item2));
        }

        if (Data.OnRemoved != null)
        {
            OnRemoved = new Payload(Caster, Data.OnRemoved);
        }

        if (Data.OnExpired != null)
        {
            OnRemoved = new Payload(Caster, Data.OnRemoved);
        }
    }

    public void Update()
    {
        var now = BattleSystem.Time;
        var end = StartTime + Data.Duration;

        if (end < now)
        {
            if (OnExpired != null)
            {
                var payloadResult = new PayloadResult(Data.OnExpired, Caster, Target);
                OnExpired.ApplyPayload(Caster, Target, payloadResult);
            }
            EndStatus();
        }
        else
        {
            for (int i = 0; i < OnInterval.Count; i++)
            {
                var actionTimeline = OnInterval[i];

                if (actionTimeline.Item2 < now)
                {
                    var payloadResult = new PayloadResult(Data.OnInterval[i].Item1, Caster, Target);
                    actionTimeline.Item1.ApplyPayload(Caster, Target, payloadResult);
                    actionTimeline.Item2 += Data.OnInterval[i].Item2;
                }
            }
        }
    }

    public void ApplyStacks(int stacks = 1)
    {
        StartTime = BattleSystem.Time;
        if (Caster != null)
        {
            UpdatePayloads();
        }

        if (CurrentStacks == Data.MaxStacks)
        {
            return;
        }

        UpdateEffects(stacks);
    }

    public void RemoveStacks(int stacks = 1)
    {

        UpdateEffects(-stacks);
    }

    void UpdateEffects(int change)
    {
        var stacksBefore = CurrentStacks;
        CurrentStacks = Mathf.Clamp(CurrentStacks + change, 0, Data.MaxStacks);

        foreach (var effect in Data.Effects)
        {
            if (effect.StacksRequired.x > stacksBefore && effect.StacksRequired.y <= stacksBefore)
            {
                effect.Remove(Data.StatusID);
            }

            if (effect.StacksRequired.x > CurrentStacks && effect.StacksRequired.y <= CurrentStacks)
            {
                effect.Apply(Data.StatusID, Target, Caster, SourcePayload);
            }
        }

        if (change > 0 && Data.OnStacksGained != null)
        {
            for (int i = 0; i < OnStacksGained.Count; i++)
            {
                var payload = OnStacksGained[i];
                if (payload.Item2 > stacksBefore && payload.Item2 <= CurrentStacks)
                {
                    var payloadResult = new PayloadResult(Data.OnStacksGained[i].Item1, Caster, Target);
                    payload.Item1.ApplyPayload(Caster, Target, payloadResult);
                }
            }
        }
    }

    public void RemoveStatus()
    {
        if (OnRemoved != null)
        {
            var payloadResult = new PayloadResult(Data.OnRemoved, Caster, Target);
            OnRemoved.ApplyPayload(Caster, Target, payloadResult);
        }
        EndStatus();
    }

    void EndStatus()
    {
        foreach (var effect in Data.Effects)
        {
            effect.Remove(Data.StatusID);
        }
    }
}