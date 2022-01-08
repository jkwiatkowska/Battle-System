using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    public enum eActionType
    {
        CollectCost,            // If a skill has a cost to be used, this action is used to collect it at the desired time.
        ApplyCooldown,          // If a skill has a cooldown, this action is used to put it on cooldown at the desired time.
        PayloadArea,            // Applies a defined payload to all entities in a given area.
        PayloadDirect,          // Applies a payload to specified entities. 
        SpawnProjectile,        // Spawns an entity that moves in a specific way and can execute skills on contact or timeout. 
        SpawnEntity,            // Spawns an entity that can execute skills. Can be used to implement area of effect skills. 
        TriggerAnimation        // Can be used to set animation triggers in the entity.  
    }

    public enum eActionCondition
    {
        AlwaysExecute,          // No condition
        OnActionSuccess,        // Only execute if the condition action was executed succesfully.
        OnActionFail,           // Only execute if the condition action failed to execute.
        OnMinChargeRatio,       // If the skill has been charged for a long enough time, the action can execute.
    }

    public string ActionID;
    public eActionType ActionType;
    public float Timestamp;

    public eActionCondition ExecuteCondition;
    public string ConditionActionID;
    public float MinChargeRatio;

    public abstract bool NeedsTarget();
}