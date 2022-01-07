using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public string SkillID;                      // Used to obtain data about a skill.

    public float Duration;                      // Execution is stopped after this much time passes.
    public bool ParallelSkill;                  // Parallel skills can be activated as the same time as other skills, without cancelling them. 

    public SkillChargeData SkillChargeData;     // A charge time before skill execution can be added.
    public List<SkillActionData> SkillTimeline; // Actions executed during skill cast.

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
            var cooldownAction = SkillTimeline.Find(a => a.ActionType == SkillActionData.eSkillActionType.ApplyCooldown) as ApplyCooldownAction;
            if (cooldownAction != null)
            {
                return cooldownAction.Cooldown;
            }
            return 0.0f;
        }
    }

    public List<CollectCostAction> SkillCost
    {
        get
        {
            var cost = new List<CollectCostAction>();

            foreach (var action in SkillTimeline)
            {
                if (action.ActionType == SkillActionData.eSkillActionType.CollectCost)
                {
                    var costAction = action as CollectCostAction;
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