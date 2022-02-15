using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySummon : Entity
{
    public ActionSummon SummonAction { get; private set; }
    public Entity Summoner { get; private set; }
    public override Entity SummoningEntity => Summoner.SummoningEntity;

    float SummonTime;

    public bool IsLinked
    {
        get
        {
            return SummonAction.LifeLink;
        }
    }

    public void SummonSetup(ActionSummon action, Entity summoner)
    {
        SummonAction = action;
        Summoner = summoner;
        SummonTime = BattleSystem.Time;

        if (SummonAction.InheritFaction)
        {
            EntityFaction = Summoner.Faction;
        }

        foreach (var attribute in GameData.EntityAttributes)
        {
            if (SummonAction.SharedAttributes.ContainsKey(attribute))
            {
                BaseAttributes[attribute] = SummonAction.SharedAttributes[attribute] *
                                            Summoner.Attribute(attribute, action.SkillID, action.ActionID, statusID: null, EntityData.Categories);
            }
        }

        SetupResourcesMax();
        SetupResourcesStart();
    }

    protected override void Update()
    {
        if (SetupComplete)
        {
            base.Update();

            if (SummonAction != null && SummonAction.SummonDuration > 0 && SummonTime + SummonAction.SummonDuration < BattleSystem.Time)
            {
                OnTrigger(TriggerData.eTrigger.OnDeath, this);
            }
        }
    }

    #region Triggers
    protected override void OnHitOutgoing(PayloadResult payloadResult)
    {
        base.OnHitOutgoing(payloadResult);
        if (Summoner != null)
        {
            Summoner.OnTrigger(TriggerData.eTrigger.OnHitOutgoing, this, payloadResult);
        }
    }

    protected override void OnHitMissed()
    {
        base.OnHitMissed();
        if (Summoner != null && Summoner.Alive)
        {
            Summoner.OnTrigger(TriggerData.eTrigger.OnDamageDealt, this);
        }
    }

    protected override void OnDamageDealt(PayloadResult payloadResult)
    {
        base.OnDamageDealt(payloadResult);
        if (Summoner != null && Summoner.Alive)
        {
            Summoner.OnTrigger(TriggerData.eTrigger.OnDamageDealt, this, payloadResult);
        }
    }

    protected override void OnRecoveryGiven(PayloadResult payloadResult)
    {
        base.OnRecoveryGiven(payloadResult);
        if (Summoner != null && Summoner.Alive)
        {
            Summoner.OnTrigger(TriggerData.eTrigger.OnRecoveryGiven, this, payloadResult);
        }
    }

    protected override void OnKill(PayloadResult payloadResult = null)
    {
        base.OnKill(payloadResult);
        if (Summoner != null && Summoner.Alive)
        {
            Summoner.OnTrigger(TriggerData.eTrigger.OnKill, this, payloadResult);
        }
    }

    protected override void OnDeath(Entity source = null, PayloadResult payloadResult = null)
    {
        base.OnDeath();
        if (Summoner != null)
        {
            Summoner.RemoveSummonedEntity(this);
        }
    }
    #endregion

    #region Tags
    public override List<Entity> GetEntitiesWithTag(string tag)
    {
        if (Summoner != null)
        {
            return Summoner.GetEntitiesWithTag(tag);
        }
        else
        {
            return base.GetEntitiesWithTag(tag);
        }
    }

    public override void TagEntity(TagData tagData, Entity entity)
    {
        base.TagEntity(tagData, entity);

        if (Summoner != null)
        {
            Summoner.TagEntity(tagData, entity);
        }
    }
    #endregion
}
