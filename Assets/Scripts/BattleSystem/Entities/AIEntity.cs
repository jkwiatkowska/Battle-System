using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEntity : Entity
{
    AIEntityData AIEntityData;
    public override void Setup(string entityID, int entityLevel, EntitySummonDetails summonDetails)
    {
        base.Setup(entityID, entityLevel, summonDetails);
    }
}
