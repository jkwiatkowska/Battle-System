using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public string SkillID;                                  // Used to obtain data about a skill.

    public bool Interruptible;                              // If true, a skill can be interrupted by other entities. 

    public SkillChargeData SkillChargeData;                 // A charge time before skill execution can be added. Additional actions can be executed at that point.
    public ActionTimeline SkillTimeline;                    // Actions executed during skill cast.

    public enum eTargetPreferrence
    {
        Any,                                                // Skills that affect the player and don't concern a particular target.
        Enemy,                                              // Skills that affect an enemy entity.
        Friendly,                                           // Skills that affect a friendly entity.
        None                                                // Skills where it doesn't matter whether there is a target or not. 
    }

    public enum eTargetStatePreferrence
    {
        Alive,
        Dead,
        Any
    }

    public bool NeedsTarget;                                // If a skill requires a target, it can only be executed if a preferred target is selected. 
                                                            // This must be set to true if any of the actions require a selected entity.
                                                            // Friendly actions will always default to the caster if a suitable target is not selected.

    public eTargetPreferrence PreferredTarget;              // If a correct target is selected, the skill will only execute when it's in range.
    public eTargetStatePreferrence PreferredTargetState;    // Normally targets are required to be alive. 
    public float Range;                                     // Maximum range from target required for the skill to be effective.
    public float MaxAngleFromTarget;                        // The target has to be within this angle from the entity's forward for a skill to work. 

    public enum eCasterState
    {
        Grounded,
        Jumping,
        Any
    }

    public eCasterState CasterState;            // Skill can only be executed in the state specified and is cancelled when state changes.
    public bool MovementCancelsSkill;           // Skill is cancelled if the entity moves while casting.

    public SkillData():this("")
    {

    }

    public SkillData(string skillID)
    {
        SkillID = skillID;
        Interruptible = false;
        SkillChargeData = null;
        SkillTimeline = new ActionTimeline();
        NeedsTarget = false;
        PreferredTarget = eTargetPreferrence.Enemy;
        Range = 20.0f;
    }

    public float Cooldown
    {
        get
        {
            var cooldownAction = SkillTimeline.Find(a => a.ActionType == Action.eActionType.ApplyCooldown) as ActionCooldown;
            if (cooldownAction != null)
            {
                return cooldownAction.Cooldown;
            }
            return 0.0f;
        }
    }

    public List<ActionCostCollection> SkillCost
    {
        get
        {
            var cost = new List<ActionCostCollection>();

            foreach (var action in SkillTimeline)
            {
                if (action.ActionType == Action.eActionType.CollectCost)
                {
                    var costAction = action as ActionCostCollection;
                    if (costAction != null && !costAction.Optional)
                    {
                        cost.Add(costAction);
                    }
                }
            }

            return cost;
        }
    }

    public bool HasChargeTime
    {
        get
        {
            return SkillChargeData != null && SkillChargeData.FullChargeTime > 0.0f;
        }
    }
}