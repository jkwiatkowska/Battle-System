using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public enum eActionType
    {
        ApplyCooldown,              // If a skill has a cooldown, this action is used to put it on cooldown at the desired time.
        CollectCost,                // If a skill has a cost to be used, this action is used to collect it at the desired time.
        DestroySelf,                // Typically used after an entity dies or expires.
        LoopBack,                   // This action can be used to go back in the timeline and repeat previous actions.
        Message,                    // Displays a message on screen. Can be used to show warnings, explain mechanics, etc.
        PayloadArea,                // Applies a defined payload to all entities in a given area.
        PayloadDirect,              // Applies a payload to specified entities. 
        SetAnimation,           // Can be used to set animation triggers in the entity. 
        SpawnProjectile,            // Spawns an entity that moves in a specific way and can execute skills on contact or timeout. 
        SpawnEntity,                // Spawns an entity that can execute skills. Can be used to implement area of effect skills.
    }

    public enum eTargetState
    {
        Alive,
        Dead,
        Any
    }

    public string ActionID;         // Used to identify actions and their results.
    public string SkillID;          // For actions tied to skills. 
    public eActionType ActionType;
    public float Timestamp;

    public List<ActionCondition> ActionConditions;

    public abstract void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults);

    public float TimestampForEntity(Entity entity)
    {
        return Formulae.ActionTime(entity, Timestamp);
    }

    public virtual bool ConditionsMet(Entity entity, Entity target, Dictionary<string, ActionResult> actionResults)
    {
        if (ActionConditions == null)
        {
            return true;
        }

        foreach (var condition in ActionConditions)
        {
            if (!condition.ConditionMet(entity, target, ActionID, actionResults))
            {
                return false;
            }
        }
        return true;
    }

    public static Action MakeAction(eActionType type, string skillID = "")
    {
        Action action = null;

        switch (type)
        {
            case eActionType.ApplyCooldown:
            {return action as ActionCooldown;
            }
            case eActionType.CollectCost:
            {return action as ActionCostCollection;
            }
            case eActionType.DestroySelf:
            {return action as ActionDestroySelf;
            }
            case eActionType.LoopBack:
            {return action as ActionLoopBack;
            }
            case eActionType.Message:
            {return action as ActionMessage;
            }
            case eActionType.PayloadArea:
            {return action as ActionPayloadArea;
            }
            case eActionType.PayloadDirect:
            {return action as ActionPayloadDirect;
            }
            case eActionType.SpawnProjectile:
            {return action as ActionProjectile;
            }
            case eActionType.SpawnEntity:
            {return action as ActionSummon;
            }
            case eActionType.SetAnimation:
            {return action as ActionAnimationSet;
            }
        }

        if (action != null)
        {
            action.ActionType = type;
            action.SkillID = skillID;
            action.SetTypeDefaults();
        }
        return action;
    }

    public abstract void SetTypeDefaults();
}