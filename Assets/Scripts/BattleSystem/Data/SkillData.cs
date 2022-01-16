using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public string SkillID;                      // Used to obtain data about a skill.

    public bool Interruptible;                  // If true, a skill can be interrupted by other entities. 

    public SkillChargeData SkillChargeData;     // A charge time before skill execution can be added. Additional actions can be executed at that point.
    public ActionTimeline SkillTimeline;          // Actions executed during skill cast.

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