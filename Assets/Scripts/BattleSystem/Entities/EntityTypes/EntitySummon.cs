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
                return;
            }

            CheckRange();
        }
    }

    public void CheckRange()
    {
        var dir = Summoner.transform.position - transform.position;
        var dist = dir.sqrMagnitude;
        if (SummonAction.MaxDistanceFromSummoner > Constants.Epsilon && 
            dist > SummonAction.MaxDistanceFromSummoner * SummonAction.MaxDistanceFromSummoner)
        {
            OnSummonerOutOfRange();
            return;
        }

        if (SummonAction.PreferredDistanceFromSummoner > Constants.Epsilon && Movement != null)
        {
            if (dist > SummonAction.PreferredDistanceFromSummoner * SummonAction.PreferredDistanceFromSummoner)
            {
                Movement.Move(dir.normalized, true);
            }
        }
    }

    public void OnSummonerOutOfRange()
    {
        switch(SummonAction.OnSummonerOutOfRange)
        {
            case ActionSummon.eOutOfRangeReaction.Destroy:
            {
                DestroyEntity();
                break;
            }
            case ActionSummon.eOutOfRangeReaction.Death:
            {
                OnDeath();
                break;
            }
            case ActionSummon.eOutOfRangeReaction.TeleportInRange:
            {
                var summonerPos = Summoner.transform.position;
                var dir = transform.position - summonerPos;
                var newPos = summonerPos + SummonAction.PreferredDistanceFromSummoner * dir.normalized;
                newPos.y = summonerPos.y;

                transform.position = newPos;
                break;
            }
        }
    }

    public override bool IsEnemy(string targetFaction)
    {
        var summonerFaction = Summoner != null ? Summoner.Faction : SummonerFaction;
        return targetFaction != summonerFaction && base.IsEnemy(targetFaction);
    }

    #region Triggers
    protected override void OnTrigger(TriggerData.eTrigger trigger, Entity triggerSource = null, PayloadResult payloadResult = null, 
                                      ActionResult actionResult = null, Action action = null, string statusID = "", 
                                      TriggerData.eEntityAffected entityAffected = TriggerData.eEntityAffected.Self, string customIdentifier = "")
    {
        base.OnTrigger(trigger, triggerSource, payloadResult, actionResult, action, statusID, entityAffected, customIdentifier);

        if (entityAffected == TriggerData.eEntityAffected.Summoner)
        {
            if (trigger == TriggerData.eTrigger.OnEngage)
            {
                EntityBattle.Engage(triggerSource);
            }
            else if (trigger == TriggerData.eTrigger.OnDisengage)
            {
                if (triggerSource != null)
                {
                    EntityBattle.Disengage(triggerSource.EntityUID);
                }
            }
        }
    }

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
