using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public string SkillID;                      // Used to obtain data about a skill.

    public float Duration;                      // Execution is stopped after this much time passes.
    public bool ParallelSkill;                  // Parallel skills can be activated as the same time as other skills, without cancelling them. 
                                                // A skill can only be parallel if it does not require a selected target or if it has a charge time.  

    public SkillChargeData SkillChargeData;     // A charge time before skill execution can be added. Additional actions can be executed at that point.
    public List<Action> SkillTimeline;          // Actions executed during skill cast.

    public bool NeedsTarget                     // Some skills cannot be cast without a target selected.
    {
        get
        {
            foreach (var action in SkillTimeline)
            {
                if (action.NeedsTarget())
                {
                    return true;
                }
            }
            return false;
        }
    }
    public float Range;                         // Minimum range from target required to execute the skill.

    public float Cooldown
    {
        get
        {
            var cooldownAction = SkillTimeline.Find(a => a.ActionType == Action.eActionType.ApplyCooldown) as ActionCooldownApplication;
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