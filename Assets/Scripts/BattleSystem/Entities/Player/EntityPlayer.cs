using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : Entity
{
    public Dictionary<KeyCode, string> PlayerSkills;

    public override void Setup(string entityID, int entityLevel, EntitySummonDetails summonDetails)
    {
        base.Setup(entityID, entityLevel);

        PlayerSkills = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Alpha1, "singleTargetAttack"},
            { KeyCode.Alpha2, "singleTargetAttackWithDrain"},
            { KeyCode.Alpha3, "coneAttack" },
            { KeyCode.Alpha4, "rectangleAttack" },
            { KeyCode.Alpha5, "chargedAttack" },
            { KeyCode.Alpha8, "projectileSkill" },
            { KeyCode.Alpha9, "summonSkill" },
            { KeyCode.Alpha0, "healAll" },
        };
    }

    protected override void OnTargetOutOfRange()
    {
        base.OnTargetOutOfRange();

        var text = NamesAndText.OutOfRangeMessage(out var color);
        MessageHUD.Instance.DisplayMessage(text, color);
    }
}
