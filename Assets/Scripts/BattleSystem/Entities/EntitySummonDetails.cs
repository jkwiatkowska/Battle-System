using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySummonDetails
{
    public ActionSummon SummonAction { get; private set; }
    public Entity Summoner { get; private set; }
    Entity Entity;

    float SummonTime;

    public bool IsLinked
    {
        get
        {
            return SummonAction.LifeLink;
        }
    }

    public EntitySummonDetails(ActionSummon action, Entity summoner, Entity summonedEntity)
    {
        SummonAction = action;
        Summoner = summoner;
        Entity = summonedEntity;
        SummonTime = BattleSystem.Time;
    }

    public void Update()
    {
        if (SummonAction.SummonDuration > 0 && SummonTime + SummonAction.SummonDuration < BattleSystem.Time)
        {
            Entity.OnTrigger(TriggerData.eTrigger.OnDeath);
        }
    }
}
