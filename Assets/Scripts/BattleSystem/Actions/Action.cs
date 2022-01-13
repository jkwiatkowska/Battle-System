using UnityEngine;

public abstract class Action
{
    public enum eActionType
    {
        ApplyCooldown,              // If a skill has a cooldown, this action is used to put it on cooldown at the desired time.
        CollectCost,                // If a skill has a cost to be used, this action is used to collect it at the desired time.
        DestroySelf,                // Typically used after an entity dies or expires.
        PayloadArea,                // Applies a defined payload to all entities in a given area.
        PayloadDirect,              // Applies a payload to specified entities. 
        SpawnProjectile,            // Spawns an entity that moves in a specific way and can execute skills on contact or timeout. 
        SpawnEntity,                // Spawns an entity that can execute skills. Can be used to implement area of effect skills. 
        TriggerAnimation            // Can be used to set animation triggers in the entity. 
    }

    public enum eActionCondition
    {
        AlwaysExecute,              // No condition
        OnActionSuccess,            // Only execute if the condition action was executed succesfully.
        OnActionFail,               // Only execute if the condition action failed to execute.
        OnMinValue,                 // Checks if a certain value is low enough.
    }

    // Used by OnMinValue condition
    public enum eConditionValueType
    {
        ChargeRatio,
        DepletableRatio,
        RandomValue
    }

    public string ActionID;         // Used to identify actions and their results.
    public string SkillID;          // For actions tied to skills. 
    public eActionType ActionType;
    public float Timestamp;

    public eActionCondition ExecuteCondition;
    public string ConditionActionID;
    public float ConditionMinValue;
    public eConditionValueType ConditionValueType;
    public string ConditionValueName;               // If depletable

    public abstract void Execute(Entity entity, out ActionResult actionResult);
    public abstract bool NeedsTarget();
    public virtual bool ConditionMet(Entity entity)
    {
        switch (ExecuteCondition)
        {
            case eActionCondition.AlwaysExecute:
            {
                return true;
            }
            case eActionCondition.OnActionSuccess:
            {
                if (entity.ActionResults.ContainsKey(ConditionActionID))
                {
                    return entity.ActionResults[ConditionActionID].Success;
                }
                else
                {
                    Debug.LogError($"Condition action for action {ActionID} has not been executed.");
                    return false;
                }
            }
            case eActionCondition.OnActionFail:
            {
                if (entity.ActionResults.ContainsKey(ConditionActionID))
                {
                    return !entity.ActionResults[ConditionActionID].Success;
                }
                else
                {
                    Debug.LogError($"Condition action for action {ActionID} has not been executed.");
                    return false;
                }
            }
            case eActionCondition.OnMinValue:
            {
                switch (ConditionValueType)
                {
                    case eConditionValueType.ChargeRatio:
                    {
                        return ConditionMinValue <= entity.SkillChargeRatio;
                    }
                    case eConditionValueType.DepletableRatio:
                    {
                        return ConditionMinValue <= entity.DepletableRatio(ConditionValueName);
                    }
                    case eConditionValueType.RandomValue:
                    {
                        return ConditionMinValue <= Random.value; 
                    }
                    default:
                    {
                        Debug.LogError($"Unsupported condition value type: {ConditionValueType}");
                        return false;
                    }
                }
            }
            default:
            {
                Debug.LogError($"Unsupported execute condition: {ExecuteCondition}");
                return false;
            }
        }
    }

    public float TimestampForEntity(Entity entity)
    {
        return Formulae.ActionTime(entity, this);
    }
}