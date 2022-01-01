using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEntity : Entity
{
    AIEntityData AIEntityData;
    public override void Setup(string entityID, int entityLevel)
    {
        base.Setup(entityID, entityLevel);

        AIEntityData = EntityData as AIEntityData;
        if (AIEntityData == null)
        {
            Debug.LogError($"Entity {entityID} is an AIEntity type, but has no AIEntityData.");
        }
        foreach (var skill in AIEntityData.Skills)
        {
            SkillCooldown.Add(skill, 0);
        }
    }
}
