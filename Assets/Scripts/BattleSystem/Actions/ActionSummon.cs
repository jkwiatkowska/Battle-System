using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSummon : Action
{
    public string EntityID;

    public TransformData SummonAtPosition;

    public float SummonDuration;                        // 0 if infinite
    public int SummonLimit;                             // The number of summoned entities can be limited
    public Dictionary<string, float> SharedAttributes;  // Summoned entity can inherit the caster's attributes
                                                        // The float value is a multiplier
                                                        // (for example an entry of {atk, 0.5} means the entity has half of the caster's atk attribute)

    public bool LifeLink;                               // If true, the entity will disappear when the caster dies
    public bool InheritFaction;                         // The summoned entity will have its faction overriden with summoner's if true.

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        actionResults[ActionID] = new ActionResult();

        if (!ConditionsMet(entity, actionResults))
        {
            return;
        }

        // Find position and transform
        var foundPosition = SummonAtPosition.TryGetTransformFromData(entity, target, out var position, out var forward);

        if (!foundPosition)
        {
            return;
        }

        // Spawn and setup
        var summon = BattleSystem.SpawnEntity(EntityID);
        actionResults[ActionID].Success = SetupSummon(summon, entity.SummoningEntity, target, position, forward);
    }

    protected virtual bool SetupSummon(Entity summon, Entity summoner, Entity target, Vector3 position, Vector3 forward)
    {
        var summonedEntity = summon as EntitySummon;
        if (summonedEntity == null)
        {
            Debug.LogError($"Summoned entity {EntityID} is missing an EntitySummon component.");
            return false;
        }

        // Set position and transform
        summonedEntity.transform.position = position;
        forward.y = 0.0f;
        summonedEntity.transform.forward = forward;

        //Setup
        summonedEntity.Setup(EntityID, summoner.Level, summoner);
        summonedEntity.SummonSetup(this, summoner);

        summoner.AddSummonedEntity(summonedEntity, this);
        summoner.TagEntity(ActionID, summonedEntity);
        return true;
    }

    public override void SetTypeDefaults()
    {
        EntityID = "";

        SummonAtPosition = new TransformData();

        SummonDuration = 60.0f;
        SummonLimit = 0;
        SharedAttributes = new Dictionary<string, float>();

        LifeLink = true;
        InheritFaction = true;
    }
}
