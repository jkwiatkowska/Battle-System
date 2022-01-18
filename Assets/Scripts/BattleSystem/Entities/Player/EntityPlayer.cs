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
            { KeyCode.Alpha2, "chargedAttack" },
            { KeyCode.Alpha3, "coneAttack" },
            { KeyCode.Alpha0, "healAll" },
        };
    }
}
