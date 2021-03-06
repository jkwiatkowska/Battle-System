using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : Entity
{
    public Dictionary<KeyCode, string> PlayerSkills;

    public override void Setup(string entityID, int entityLevel, Entity source = null)
    {
        base.Setup(entityID, entityLevel);

        PlayerSkills = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Alpha1, "neutralAttack"},
            { KeyCode.Alpha2, "fireAttack"},
            { KeyCode.Alpha3, "waterAttack" },
            { KeyCode.Alpha6, "projectileSkill" },
            { KeyCode.Alpha7, "projectileSkill2" },
            { KeyCode.Alpha8, "projectileSkill3" },
            { KeyCode.Alpha9, "summonSkill" },
            { KeyCode.Alpha0, "healAll" },
        };
    }

    public override void OnTargetOutOfRange()
    {
        base.OnTargetOutOfRange();

        var text = NamesAndText.OutOfRangeMessage(out var color);
        MessageHUD.Instance.DisplayMessage(text, color);
    }

    public override void OnTargetNotInLineOfSight()
    {
        base.OnTargetNotInLineOfSight();

        var text = NamesAndText.NotInLineOfSightMessage(out var color);
        MessageHUD.Instance.DisplayMessage(text, color);
    }
}
