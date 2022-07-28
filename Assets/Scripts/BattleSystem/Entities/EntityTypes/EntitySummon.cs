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

        foreach (var attribute in SummonAction.SharedAttributes)
        {
            BaseAttributes[attribute.Key] = attribute.Value * Summoner.Attribute(attribute.Key, payload: null, action, statusID: null);
        }

        SetupResourcesMax();
        SetupResourcesStart();
    }

    protected override void Update()
    {
        if (SetupComplete)
        {
            base.Update();

            if (SummonAction != null && SummonAction.SummonDuration > Constants.Epsilon && SummonTime + SummonAction.SummonDuration < BattleSystem.Time)
            {
                OnDeath();
                return;
            }

            CheckRange();
        }
    }

    public void CheckRange()
    {
        var dir = Summoner.Position - Position;
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
                var summonerPos = Summoner.Position;
                var dir = Position - summonerPos;
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
    protected override void OnTrigger(TriggerData.eTrigger trigger, Entity triggerSource = null, Payload payload = null,
                                      PayloadComponentResult payloadResult = null, ActionResult actionResult = null, Action action = null,
                                      Dictionary<string, ActionResult> actionResults = null, string statusID = "",
                                      TriggerData.eEntityAffected entityAffected = TriggerData.eEntityAffected.Self, string customIdentifier = "")
    {
        base.OnTrigger(trigger, triggerSource, payload, payloadResult, actionResult, action, actionResults, statusID, entityAffected, customIdentifier);

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
                    EntityBattle.Disengage(triggerSource.UID);
                }
            }
        }
    }

    public override void OnPayloadComponentApplied(PayloadComponentResult payloadResult)
    {
        base.OnPayloadComponentApplied(payloadResult);
        Summoner?.OnPayloadComponentApplied(payloadResult);
    }

    public override void OnStatusApplied(Entity target, string statusName)
    {
        base.OnStatusApplied(target, statusName);
        Summoner?.OnStatusApplied(target, statusName);
    }

    public override void OnStatusClearedOutgoing(Entity target, string statusName)
    {
        base.OnStatusClearedOutgoing(target, statusName);
        Summoner?.OnStatusClearedOutgoing(target, statusName);
    }

    public override void OnKill(PayloadComponentResult payloadResult = null, string statusID = "")
    {
        base.OnKill(payloadResult, statusID);
        Summoner?.OnKill(payloadResult, statusID);
    }

    public override void OnDeath(Entity source = null, PayloadComponentResult payloadResult = null)
    {
        base.OnDeath(source, payloadResult);
        Summoner?.RemoveSummonedEntity(this);
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
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
