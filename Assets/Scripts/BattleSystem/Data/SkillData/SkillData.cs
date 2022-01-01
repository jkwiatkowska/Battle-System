using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillData
{
    public string SkillID;                      // Used to obtain data about a skill

    public float ChargeTime;                    // Skill starts being executed after this much time passes
    public float Duration;                      // Execution is stopped after this much time passes
    public float Cooldown;                      // After using a skill, it goes on cooldown and cannot be used again until this much time passes

    public string CostType;                     // One of the depletable attributes defined in game data
    public float Cost;                          // How much is depleted

    public List<SkillActionData> SkillTimeline; // Actions executed during skill cast

    public bool NeedsTarget                     // Some skills cannot be cast without a target selected
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
    public float Range;                         // Minimum range from target required to execute the skill
}