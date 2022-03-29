using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySummon : Entity
{
    public ActionSummon SummonAction { get; private set; }
    public Entity Summoner           { get; private set; }
    string SummonerFaction;
    public override Entity SummoningEntity => Summoner.SummoningEntity;
    public override string Faction => SummonAction.InheritFaction ? (Summoner != null ? Summoner.Faction : SummonerFaction) : EntityData.Faction;

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
        SummonerFaction = summoner.Faction;
        SummonTime = BattleSystem.Time;

        foreach (var attribute in BattleData.EntityAttributes)
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
                OnDeath();
            }
        }
    }

    #region Triggers
    public override void OnPayloadApplied(PayloadResult payloadResult)
    {
        base.OnPayloadApplied(payloadResult);
        Summoner.OnPayloadApplied(payloadResult);
    }

    public override void OnStatusApplied(Entity target, string statusName)
    {
        base.OnStatusApplied(target, statusName);
        Summoner.OnStatusApplied(target, statusName);
    }

    public override void OnStatusClearedOutgoing(Entity target, string statusName)
    {
        base.OnStatusClearedOutgoing(target, statusName);
        Summoner.OnStatusClearedOutgoing(target, statusName);
    }

    public override void OnKill(PayloadResult payloadResult = null, string statusID = "")
    {
        base.OnKill(payloadResult, statusID);
        Summoner.OnKill(payloadResult, statusID);
    }

    public override void OnDeath(Entity source = null, PayloadResult payloadResult = null)
    {
        base.OnDeath(source, payloadResult);
        if (Summoner != null)
        {
            Summoner.RemoveSummonedEntity(this);
        }
    }

    protected override void OnSpawn()
    {
        OnTrigger(TriggerData.eTrigger.OnSpawn, triggerSource: Summoner);
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

    public override void TagEntity(string tag, Entity entity, TagData tagData)
    {
        base.TagEntity(tag, entity, tagData);

        if (Summoner != null)
        {
            Summoner.TagEntity(tag, entity, tagData);
        }
    }
    #endregion
}
