using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public enum eActionType
    {
        ApplyCooldown,              // If a skill has a cooldown, this action is used to put it on cooldown at the desired time.
        CollectCost,                // If a skill has a cost to be used, this action is used to collect it at the desired time.
        DestroySelf,                // Typically used after an entity dies or expires.
        Message,                    // Displays a message on screen. Can be used to show warnings, explain mechanics, etc.
        PayloadArea,                // Applies a defined payload to all entities in a given area.
        PayloadDirect,              // Applies a payload to specified entities. 
        SpawnProjectile,            // Spawns an entity that moves in a specific way and can execute skills on contact or timeout. 
        SpawnEntity,                // Spawns an entity that can execute skills. Can be used to implement area of effect skills.
        TriggerAnimation,           // Can be used to set animation triggers in the entity. 
    }

    public string ActionID;         // Used to identify actions and their results.
    public string SkillID;          // For actions tied to skills. 
    public eActionType ActionType;
    public float Timestamp;

    public List<ActionCondition> ActionConditions;

    public abstract void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults);

    public float TimestampForEntity(Entity entity)
    {
        return Formulae.ActionTime(entity, this);
    }

    public bool ConditionsMet(Entity entity, Dictionary<string, ActionResult> actionResults)
    {
        if (ActionConditions == null)
        {
            return true;
        }

        foreach (var condition in ActionConditions)
        {
            if (!condition.ConditionMet(entity, ActionID, actionResults))
            {
                return false;
            }
        }
        return true;
    }
}