using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public StatusEffectData Data    { get; private set; }
    Action Action;
    public int CurrentStacks        { get; private set; }
    Entity Target;
    public Entity Caster            { get; private set; }
    float StartTime;
    public float EndTime            { get; private set; }
    Payload SourcePayload;

    public List<(Payload Payload, float NextTimestamp)> OnInterval;
    public List<Payload> OnCleared;
    public List<Payload> OnExpired;

    public bool RemoveEffect;

    public StatusEffect (Entity target, Entity caster, StatusEffectData statusEffectData, Action action, Payload payload)
    {
        Setup(target, caster, statusEffectData, action, payload);
    }

    public void Setup(Entity target, Entity caster, StatusEffectData statusEffectData, Action action, Payload payload)
    {
        Data = statusEffectData;
        Action = action;
        CurrentStacks = 0;
        Target = target;
        Caster = caster;
        StartTime = BattleSystem.Time;
        EndTime = StartTime + Formulae.StatusDurationTime(Caster, Target, Data);
        SourcePayload = payload;
        RemoveEffect = false;
        OnInterval = new List<(Payload Payload, float NextTimestamp)>();
        OnCleared = new List<Payload>();
        OnExpired = new List<Payload>();
    }

    void UpdatePayloads()
    {
        OnInterval = new List<(Payload Payload, float NextTimestamp)>();
        if (Data.OnInterval != null)
        {
            foreach (var payload in Data.OnInterval)
            {
                OnInterval.Add((Payload: new Payload(Caster, payloadData: payload.PayloadData, Action, Data.StatusID), NextTimestamp: BattleSystem.Time + payload.Interval));
            }
        }

        if (Data.OnCleared != null && Data.OnCleared.Count > 0)
        {
            OnCleared = new List<Payload>();
            foreach (var payload in Data.OnCleared)
            {
                OnCleared.Add(new Payload(Caster, payload, Action, Data.StatusID));
            }
        }

        if (Data.OnExpired != null && Data.OnExpired.Count > 0)
        {
            OnExpired = new List<Payload>();
            foreach (var payload in Data.OnExpired)
            {
                OnExpired.Add(new Payload(Caster, payload, Action, Data.StatusID));
            }
        }
    }

    public bool Update()
    {
        if (RemoveEffect)
        {
            return false;
        }

        var now = BattleSystem.Time;

        if (Data.Duration > Constants.Epsilon && EndTime < now)
        {
            if (OnExpired != null)
            {
                for (int i = 0; i < OnExpired.Count; i++)
                {
                    var payloadResult = new PayloadResult(Data.OnExpired[i], Caster, Target);
                    OnExpired[i].ApplyPayload(Caster, Target, payloadResult);
                }
            }
            return false;
        }
        
        for (int i = 0; i < OnInterval.Count; i++)
        {
            var payload = OnInterval[i];

            if (payload.NextTimestamp < now)
            {
                var payloadResult = new PayloadResult(Data.OnInterval[i].PayloadData, Caster, Target);
                payload.Payload.ApplyPayload(Caster, Target, payloadResult);
                payload.NextTimestamp += Data.OnInterval[i].Interval;

                OnInterval[i] = payload;
            }
        }
        return true;
    }

    public void ApplyStacks(int stacks = 1)
    {
        StartTime = BattleSystem.Time;
        EndTime = StartTime + Formulae.StatusDurationTime(Caster, Target, Data);

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

        for (int i = 0; i < Data.Effects.Count; i++)
        {
            var effect = Data.Effects[i];
            if (change > 0)
            {
                if (effect.StacksRequiredMax < CurrentStacks)
                {
                    effect.Remove(Data.StatusID, i, Target);
                }

                if (effect.StacksRequiredMin > stacksBefore && effect.StacksRequiredMin <= CurrentStacks && 
                    effect.StacksRequiredMax >= CurrentStacks)
                {
                    effect.Apply(Data.StatusID, i, Target, Caster, SourcePayload);
                }
            }
            else if (change < 0)
            {
                if (effect.StacksRequiredMin > CurrentStacks)
                {
                    effect.Remove(Data.StatusID, i, Target);
                }

                if (effect.StacksRequiredMax < stacksBefore && effect.StacksRequiredMin <= CurrentStacks &&
                    effect.StacksRequiredMax >= CurrentStacks)
                {
                    effect.Apply(Data.StatusID, i, Target, Caster, SourcePayload);
                }
            }
        }
    }

    public void ClearStatus()
    {
        if (OnCleared != null)
        {
            for (int i = 0; i < OnCleared.Count; i++)
            {
                var payloadResult = new PayloadResult(Data.OnCleared[i], Caster, Target);
                OnCleared[i].ApplyPayload(Caster, Target, payloadResult);
            }
        }
        RemoveStatus();
    }

    public void RemoveStatus()
    {
        for (int i = 0; i < Data.Effects.Count; i++)
        {
            var effect = Data.Effects[i];
            effect.Remove(Data.StatusID, i, Target);
        }
    }
}