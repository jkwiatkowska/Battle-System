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
    float EndTime;
    Payload SourcePayload;

    public List<(Payload Payload, float NextTimestamp)> OnInterval;
    public List<(Payload Payload, int Stacks)> OnStacksGained;
    public Payload OnCleared;
    public Payload OnExpired;

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

        OnStacksGained = new List<(Payload, int)>();
        if (Data.OnStacksGained != null)
        {
            foreach (var payload in Data.OnStacksGained)
            {
                OnStacksGained.Add((Payload: new Payload(Caster, payloadData: payload.PayloadData, Action, Data.StatusID), Stacks: payload.Stacks));
            }
        }

        if (Data.OnCleared != null)
        {
            OnCleared = new Payload(Caster, Data.OnCleared, Action, Data.StatusID);
        }

        if (Data.OnExpired != null)
        {
            OnCleared = new Payload(Caster, Data.OnCleared, Action, Data.StatusID);
        }
    }

    public bool Update()
    {
        if (RemoveEffect)
        {
            return false;
        }

        var now = BattleSystem.Time;

        if (Data.Duration > Constants.Epsilon)
        {
            if (EndTime < now)
            {
                if (OnExpired != null)
                {
                    var payloadResult = new PayloadResult(Data.OnExpired, Caster, Target);
                    OnExpired.ApplyPayload(Caster, Target, payloadResult);
                }
                return false;
            }
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
                if (effect.StacksRequired.y < CurrentStacks)
                {
                    effect.Remove(Data.StatusID, i, Target);
                }

                if (effect.StacksRequired.x > stacksBefore && effect.StacksRequired.x <= CurrentStacks && 
                    effect.StacksRequired.y >= CurrentStacks)
                {
                    effect.Apply(Data.StatusID, i, Target, Caster, SourcePayload);
                }
            }
            else if (change < 0)
            {
                if (effect.StacksRequired.x > CurrentStacks)
                {
                    effect.Remove(Data.StatusID, i, Target);
                }

                if (effect.StacksRequired.y < stacksBefore && effect.StacksRequired.x <= CurrentStacks &&
                    effect.StacksRequired.y >= CurrentStacks)
                {
                    effect.Apply(Data.StatusID, i, Target, Caster, SourcePayload);
                }
            }
        }

        if (change > 0 && Data.OnStacksGained != null)
        {
            for (int i = 0; i < OnStacksGained.Count; i++)
            {
                var payload = OnStacksGained[i];
                if (payload.Stacks > stacksBefore && payload.Stacks <= CurrentStacks)
                {
                    var payloadResult = new PayloadResult(Data.OnStacksGained[i].PayloadData, Caster, Target);
                    payload.Payload.ApplyPayload(Caster, Target, payloadResult);
                }
            }
        }
    }

    public void ClearStatus()
    {
        if (OnCleared != null)
        {
            var payloadResult = new PayloadResult(Data.OnCleared, Caster, Target);
            OnCleared.ApplyPayload(Caster, Target, payloadResult);
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