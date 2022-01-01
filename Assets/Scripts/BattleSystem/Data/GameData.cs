using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    public static List<string> Affinities;              // Damage types and resistances can be calculated using these
    public static List<string> EntityDepletables;       // Values like hit points, mana, stamina, etc.
    public static string HitPoints;                     // Which of the depletables represents HP
    public static List<string> EntityAttributes;        // Stats mainly used to determine outgoing and incoming damage
    public static List<string> PayloadFlags;            // Flags to customise payload damage
    
    static Dictionary<string, FactionData> FactionData; // Define entity allegiance and relations
    static Dictionary<string, EntityData> EntityData;
    static Dictionary<string, PayloadData> PayloadData;
    static Dictionary<string, SkillData> SkillData;

    public static int MaxEntityLevel;

    public static void LoadData(string path)
    {

    }

    public static void LoadMockData()
    {
        FactionData = new Dictionary<string, FactionData>
        {
            {
                "Player",
                new FactionData()
                {
                    FactionID = "Player",
                    FriendlyFactions = new List<string>(),
                    EnemyFactions = new List<string>()
                    {
                        "Dummy"
                    }
                }
            },
            {
                "Dummy",
                new FactionData()
                {
                    FactionID = "Dummy",
                    FriendlyFactions = new List<string>(),
                    EnemyFactions = new List<string>()
                }
            }
        };

        EntityData = new Dictionary<string, EntityData>
        {
            {
                "Player",
                new EntityData()
                {
                    BaseAttributes = new Dictionary<string, Vector2>(),
                    MaxDepletables = new Dictionary<string, Vector2>(),
                    IsTargetable = true,
                    Faction = "Player",
                    IsAI = false
                }
            },
            {
                "Dummy",
                new EntityData()
                {
                    BaseAttributes = new Dictionary<string, Vector2>(),
                    MaxDepletables = new Dictionary<string, Vector2>(),
                    IsTargetable = true,
                    Faction = "Dummy",
                    IsAI = false
                }
            }
        };
    }

    #region Editor
    public static void SaveData(string path)
    {

    }

    public static void UpdateSkillID(string oldID, string newID)
    {
        if (SkillData.ContainsKey(oldID))
        {
            SkillData.Add(newID, SkillData[oldID]);
            SkillData.Remove(oldID);

            foreach (var entity in EntityData)
            {
                if (entity.Value.IsAI)
                {
                    var aiEntity = entity.Value as AIEntityData;
                    if (aiEntity == null)
                    {
                        Debug.LogError($"Entity {entity.Key} expected to be an AIEntity.");
                    }

                    var entitySkills = aiEntity.Skills;
                    while (entitySkills.Contains(oldID))
                    {
                        entitySkills[entitySkills.IndexOf(oldID)] = newID;
                    }
                }
            }
        }
    }

    public static void DeleteSkill(string skillID)
    {
        if (SkillData.ContainsKey(skillID))
        {
            SkillData.Remove(skillID);

            foreach (var entity in EntityData)
            {
                if (entity.Value.IsAI)
                {
                    var aiEntity = entity.Value as AIEntityData;
                    if (aiEntity == null)
                    {
                        Debug.LogError($"Entity {entity.Key} expected to be an AIEntity.");
                    }

                    var entitySkills = aiEntity.Skills;
                    if (entitySkills.Contains(skillID))
                    {
                        entitySkills.RemoveAll(s => s == skillID);
                    }
                }
            }
        }
    }
    #endregion

    public static EntityData GetEntityData(string entityID)
    {
        if (EntityData.ContainsKey(entityID))
        {
            return EntityData[entityID];
        }
        else
        {
            Debug.LogError($"Entity ID {entityID} could not be found.");
            return null;
        }
    }

    public static PayloadData GetPayloadData(string payloadID)
    {
        if (PayloadData.ContainsKey(payloadID))
        {
            return PayloadData[payloadID];
        }
        else
        {
            Debug.LogError($"Payload ID {payloadID} could not be found.");
            return null;
        }
    }

    public static SkillData GetSkillData(string skillID)
    {
        if (SkillData.ContainsKey(skillID))
        {
            return SkillData[skillID];
        }
        else
        {
            Debug.LogError($"Skill ID {skillID} could not be found.");
            return null;
        }
    }

    public static FactionData GetFactionData(string factionID)
    {
        if (FactionData.ContainsKey(factionID))
        {
            return FactionData[factionID];
        }
        else
        {
            Debug.LogError($"Faction ID {factionID} could not be found.");
            return null;
        }
    }

    public static List<Entity> GetAllEntities()
    {
        List<Entity> entities = new List<Entity>();

        return entities;
    }

    public static List<Entity> GetEntitiesInRange(Vector3 position, float range)
    {
        List<Entity> entities = new List<Entity>();

        return entities;
    }
}
