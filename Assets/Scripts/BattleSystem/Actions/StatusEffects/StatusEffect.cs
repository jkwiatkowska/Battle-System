using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect
{
    public StatusEffectData Data    { get; private set; }
    public int CurrentStacks        { get; private set; }
    EntityInfo Target;
    public EntityInfo Caster;
    float StartTime;
    public float EndTime;
    float DurationChange;
    public float ExpireTime => EndTime + DurationChange;

    Payload SourcePayload;

    public List<(Payload Payload, float NextTimestamp)> OnInterval;
    public Payload OnCleared;
    public Payload OnExpired;

    public bool RemoveEffect;   // This is set true when the effect is due to be removed.

    public StatusEffect(StatusEffectData statusEffectData, EntityInfo caster, EntityInfo target)
    {
        Setup(statusEffectData, caster, target, payload: null);
    }

    public StatusEffect(StatusEffectData statusEffectData, Payload payload)
    {
        Setup(statusEffectData, payload.Caster, payload.Target, payload);
    }

    public void Setup(StatusEffectData statusEffectData, EntityInfo caster, EntityInfo target, Payload payload)
    {
        Data = statusEffectData;
        Caster = caster;
        Target = target;
        SourcePayload = payload;
        CurrentStacks = 0;
        StartTime = BattleSystem.Time;
        EndTime = StartTime + Formulae.StatusDurationTime(Caster, Target, Data);
        RemoveEffect = false;

        RefreshPayloads();
    }

    void RefreshPayloads()
    {
        OnInterval = new List<(Payload Payload, float NextTimestamp)>();
        foreach (var p in Data.OnInterval)
        {
            OnInterval.Add((Payload: new Payload(Caster.Entity, p.Payload, SourcePayload, Data.StatusID), NextTimestamp: BattleSystem.Time + p.Delay));
        }

        if (Data.OnCleared != null)
        {
            OnCleared = new Payload(Caster.Entity, Data.OnCleared, SourcePayload, Data.StatusID);
        }

        if (Data.OnExpired != null)
        {
            OnExpired = new Payload(Caster.Entity, Data.OnExpired, SourcePayload, Data.StatusID);
        }
    }

    public bool Update()
    {
        if (RemoveEffect || (Caster.Entity == null && Data.RemoveOnCasterDeath))
        {
            return false;
        }

        var now = BattleSystem.Time;

        if (Data.Duration > Constants.Epsilon && ExpireTime < now)
        {
            if (OnExpired != null)
            {
                OnExpired.ApplyPayload(Target.Entity, out _);
            }
            return false;
        }
        
        for (int i = 0; i < OnInterval.Count; i++)
        {
            var payload = OnInterval[i];

            if (payload.NextTimestamp < now)
            {
                payload.Payload.ApplyPayload(Target.Entity, out _);
                payload.NextTimestamp += Data.OnInterval[i].Interval;

                OnInterval[i] = payload;
            }
        }
        return true;
    }

    public void ApplyStacks(int stacks = 1, bool refreshDuration = true, bool refreshPayloads = true)
    {
        StartTime = BattleSystem.Time;
        if (refreshDuration)
        {
            EndTime = StartTime + Formulae.StatusDurationTime(Caster, Target, Data);
        }

        if (refreshPayloads)
        {
            RefreshPayloads();
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
                    effect.Remove(Data.StatusID, Caster.UID, i, Target.Entity);
                }

                if (effect.StacksRequiredMin > stacksBefore && effect.StacksRequiredMin <= CurrentStacks && 
                    effect.StacksRequiredMax >= CurrentStacks)
                {
                    effect.Apply(Data.StatusID, i, Caster, Target);
                }
            }
            else if (change < 0)
            {
                if (effect.StacksRequiredMin > CurrentStacks)
                {
                    effect.Remove(Data.StatusID, Caster.UID, i, Target.Entity);
                }

                if (effect.StacksRequiredMax < stacksBefore && effect.StacksRequiredMin <= CurrentStacks &&
                    effect.StacksRequiredMax >= CurrentStacks)
                {
                    effect.Apply(Data.StatusID, i, Caster, Target);
                }
            }
        }
    }

    public void ClearStatus()
    {
        if (OnCleared != null)
        {
            OnCleared.ApplyPayload(Target.Entity, out _);
        }
        RemoveStatus();
    }

    public void RemoveStatus()
    {
        for (int i = 0; i < Data.Effects.Count; i++)
        {
            var effect = Data.Effects[i];
            effect.Remove(Data.StatusID, Caster.UID, i, Target.Entity);
        }
    }

    public void ChangeDuration(float change)
    {
        DurationChange += change;
        var max = Data.DurationIncreaseLimit;
        if (max > Constants.Epsilon && max < DurationChange)
        {
            DurationChange = max;
        }
    }
}