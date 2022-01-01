using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectActionData : PayloadActionData
{
    public enum eDirectSkillTargets
    {
        SelectedTarget,
        AllTargets,
        RandomTargets
    }

    public eDirectSkillTargets SkillTargets;
    public int TargetCount;

    public override bool NeedsTarget()
    {
        return SkillTargets != eDirectSkillTargets.SelectedTarget && Target == eTarget.EnemyEntities;
    }
}
