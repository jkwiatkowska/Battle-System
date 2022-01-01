using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PayloadActionData : SkillActionData
{
    public enum eTarget
    {
        Self,
        EnemyEntities,
        FriendlyEntities
    }

    public eTarget Target;  // Which entities the payload affects
    public PayloadData Payload;
}
