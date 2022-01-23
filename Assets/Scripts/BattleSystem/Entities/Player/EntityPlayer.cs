using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPlayer : Entity
{
    public Dictionary<KeyCode, string> PlayerSkills;

    public override void Setup(string entityID, int entityLevel)
    {
        base.Setup(entityID, entityLevel);

        PlayerSkills = new Dictionary<KeyCode, string>()
        {
            { KeyCode.Alpha1, "singleTargetAttack"},
            { KeyCode.Alpha2, "singleTargetAttackWithDrain"},
            { KeyCode.Alpha3, "chargedAttack" },
            { KeyCode.Alpha4, "coneAttack" },
            { KeyCode.Alpha5, "rectangleAttack" },
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
