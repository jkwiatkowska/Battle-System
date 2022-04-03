using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public enum eActionType
    {
        ApplyCooldown,              // If a skill has a cooldown, this action is used to put it on cooldown at the desired time.
        CollectCost,                // If a skill has a cost to be used, this action is used to collect it at the desired time.
        Destroy,                    // Typically used after an entity dies or expires.
        DoNothing,                  // Empty action. Can be used for condition checks or to delay end of action timeline.
        LoopBack,                   // This action can be used to go back in the timeline and repeat previous actions.
        Message,                    // Displays a message on screen. Can be used to show warnings, explain mechanics, etc.
        PayloadArea,                // Applies a defined payload to all entities in a given area.
        PayloadDirect,              // Applies a payload to specified entities. 
        SetAnimation,               // Can be used to set animation triggers in the entity. 
        SpawnProjectile,            // Spawns an entity that moves in a specific way and can execute skills on contact or timeout. 
        SpawnEntity,                // Spawns an entity that can execute skills. Can be used to implement area of effect skills.
        Cancel,                     // This will stop the timeline from continuing. 
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
            {
                action = new ActionCooldown();
                break;
            }
            case eActionType.CollectCost:
            {
                action = new ActionCostCollection();
                break;
            }
            case eActionType.Destroy:
            {
                action = new ActionDestroy();
                break;
            }
            case eActionType.LoopBack:
            {
                action = new ActionLoopBack();
                break;
            }
            case eActionType.Message:
            {
                action = new ActionMessage();
                break;
            }
            case eActionType.PayloadArea:
            {
                action = new ActionPayloadArea();
                break;
            }
            case eActionType.PayloadDirect:
            {
                action = new ActionPayloadDirect();
                break;
            }
            case eActionType.SpawnProjectile:
            {
                action = new ActionProjectile();
                break;
            }
            case eActionType.SpawnEntity:
            {
                action = new ActionSummon();
                break;
            }
            case eActionType.SetAnimation:
            {
                action = new ActionAnimationSet();
                break;
            }
            case eActionType.Cancel:
            {
                action = new ActionCancel();
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented action type: {type}");
                return null;
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