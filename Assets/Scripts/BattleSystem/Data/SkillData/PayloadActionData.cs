using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PayloadActionData : ActionData
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
