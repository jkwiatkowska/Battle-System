using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectActionData : PayloadActionData
{
    public enum eDirectSkillTargets
    {
        SelectedEntity,
        AllEntities,
        RandomEntities,
        TaggedEntity
    }

    public eDirectSkillTargets SkillTargets;
    public int TargetCount;
    public string EntityTag;

    public override bool NeedsTarget()
    {
        return SkillTargets != eDirectSkillTargets.SelectedEntity && Target == eTarget.EnemyEntities;
    }
}
