using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public string SkillID;                                  // Used to obtain data about a skill.

    public bool IsActive;                                   // Active skills may have requirements such as specific target and range. Only one active skill can be executed at a time.
                                                            // Passive skills can be triggered independendly and cannot be interrupted. 
    public bool Interruptible;                              // If true, a skill can be interrupted by other entities. 

    public int SkillPriority;                               //Skills with higher priority cannot be cancelled by skills with lower priority.

    public SkillChargeData SkillChargeData;                 // A charge time before skill execution can be added. Additional actions can be executed at that point.
    public ActionTimeline SkillTimeline;                    // Actions executed during skill cast.

    public enum eTargetPreferrence
    {
        Any,                                                // Skills that affect the player and don't concern a particular target.
        Enemy,                                              // Skills that affect an enemy entity.
        Friendly,                                           // Skills that affect a friendly entity.
        None                                                // Skills where it doesn't matter whether there is a target or not. 
    }

    public bool NeedsTarget;                                // If a skill requires a target, it can only be executed if a preferred target is selected. 
                                                            // This must be set to true if any of the actions require a selected entity.
                                                            // Friendly actions will always default to the caster if a suitable target is not selected.

    public eTargetPreferrence PreferredTarget;              // If a correct target is selected, the skill will only execute when it's in range.
    public Action.eTargetState PreferredTargetState;        // Normally targets are required to be alive. 
    public float Range;                                     // Maximum range from target required for the skill to be effective.
    public float MaxAngleFromTarget;                        // The target has to be within this angle from the entity's forward for a skill to work. 
    public bool RequireLineOfSight;                         // If true, the entity will only use the skill if the target is in line of sight.

    public enum eCasterState
    {
        Any,
        Grounded,
        Jumping,
    }

    public eCasterState CasterState;            // Skill can only be executed in the state specified and is cancelled when state changes.
    public bool MovementCancelsSkill;           // Skill is cancelled if the entity moves while casting.

    public List<(string ID, int Stacks)> StatusEffectsRequired; // Skills can have status effect requirements, making them only usable when the entity has the given status effects. 
    public List<string> StatusEffectGroupsRequired;             // Alternatively, the requirement can be more vague - any status effect in the group stated will make the skill usable. 

    public SkillData():this("")
    {

    }

    public SkillData(string skillID)
    {
        SkillID = skillID;
        IsActive = true;
        Interruptible = false;
        SkillChargeData = null;
        SkillTimeline = new ActionTimeline();
        NeedsTarget = false;
        PreferredTarget = eTargetPreferrence.Enemy;
        Range = 20.0f;
        StatusEffectsRequired = new List<(string ID, int Stacks)>();
        StatusEffectGroupsRequired = new List<string>();
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