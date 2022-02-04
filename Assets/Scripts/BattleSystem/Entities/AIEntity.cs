using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEntity : Entity
{
    AIEntityData AIEntityData;
    public override void Setup(string entityID, int entityLevel, Entity source = null)
    {
        base.Setup(entityID, entityLevel, source);
    }
}
