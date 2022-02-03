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

    protected Entity SummonnedEntity;                   // Used by inheriting classes

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

        // Setup
        SummonnedEntity = BattleSystem.SpawnEntity(EntityID);
        SummonnedEntity.Setup(EntityID, entity.Level, new EntitySummonDetails(this, entity, SummonnedEntity));
        entity.AddSummonnedEntity(SummonnedEntity, this);

        // Set position and transform
        SummonnedEntity.transform.position = position;

        SummonnedEntity.transform.forward = forward;

        actionResults[ActionID].Success = true;
    }
}
