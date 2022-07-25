using FullSerializer;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class BattleData
{
    public static BattleData Instance = new BattleData();

    #region Game Data
    public List<string> PayloadCategoryData = new List<string>();                                               // These can be used to define and customise payloads.
    public List<string> EntityCategoryData = new List<string>();                                                // These can be used to customise payload effect on entities.
    public Dictionary<string, Value> EntityResourceData = new Dictionary<string, Value>();                      // Values like hit points, mana, stamina, etc and their max values
                                                                                                                // based on entity attributes.
    public List<string> EntityAttributeData = new List<string>();                                               // Stats mainly used to determine outgoing and incoming damage.
    public MultiplierAttributeData MultiplierAttributeData = new MultiplierAttributeData();                     // SOme attributes can be treated as multipliers to damage and other things.
    public List<string> PayloadFlagData = new List<string>();                                                   // Flags to customise payload damage.

    public Dictionary<string, FactionData> FactionData = new Dictionary<string, FactionData>();                 // Define entity allegiance and relations.
    public Dictionary<string, EntityData> EntityData = new Dictionary<string, EntityData>();
    public Dictionary<string, SkillData> SkillData = new Dictionary<string, SkillData>();
    public Dictionary<string, List<string>> SkillGroupData = new Dictionary<string, List<string>>();            // Cooldowns and effects can apply to multiple skills at once.
    public Dictionary<string, StatusEffectData> StatusEffectData = new Dictionary<string, StatusEffectData>();
    public Dictionary<string, List<string>> StatusEffectGroupData = new Dictionary<string, List<string>>();     // Effects can be grouped together and affected all at once.
    public AggroData AggroData = new AggroData();
    #endregion

    #region Getters
    public static List<string> PayloadCategories => Instance.PayloadCategoryData;
    public static List<string> EntityCategories => Instance.EntityCategoryData;
    public static Dictionary<string, Value> EntityResources => Instance.EntityResourceData;

    public static List<string> EntityAttributes => Instance.EntityAttributeData;
    public static MultiplierAttributeData Multipliers => Instance.MultiplierAttributeData;
    public static List<string> PayloadFlags => Instance.PayloadFlagData;

    public static Dictionary<string, FactionData> Factions => Instance.FactionData;
    public static Dictionary<string, EntityData> Entities => Instance.EntityData;
    public static Dictionary<string, SkillData> Skills => Instance.SkillData;
    public static Dictionary<string, List<string>> SkillGroups => Instance.SkillGroupData;
    public static Dictionary<string, StatusEffectData> StatusEffects => Instance.StatusEffectData;
    public static Dictionary<string, List<string>> StatusEffectGroups => Instance.StatusEffectGroupData;
    public static AggroData Aggro => Instance.AggroData;
    #endregion

    public static readonly fsSerializer Serializer = new fsSerializer();
    const string BackupPath = "Data/Backup";

    public static void LoadData(TextAsset textAsset)
    {
        Instance = new BattleData();

        // Parse the JSON data.
        fsData data = fsJsonParser.Parse(textAsset.text);

        // Deserialize the data.
        Serializer.TryDeserialize(data, ref Instance);
    }

    #region Editor
#if UNITY_EDITOR
    public static void LoadData(string path)
    {
        Instance = new BattleData();

        // Read from a file.
        var json = Resources.Load<TextAsset>(path).text;

        // Parse the JSON data.
        fsData data = fsJsonParser.Parse(json);

        // Deserialize the data.
        Serializer.TryDeserialize(data, ref Instance);
    }

    public static void SaveData(string path)
    {
        // Backup the data from the file we're overwriting.
        BackupData(path);

        // Serialize the data.
        Serializer.TrySerialize(Instance, out var data);

        // Emit the data via JSON.
        var json = fsJsonPrinter.CompressedJson(data);

        // Save to a file.
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/" + path + ".json", json);

        // Refresh the project.
        AssetDatabase.Refresh();
    }

    public static void BackupData(string path)
    {
        // Read from a file.
        var json = Resources.Load<TextAsset>(path)?.text;

        // Save to a file.
        if (json != null)
        {
            System.IO.File.WriteAllText(Application.dataPath + "/Resources/" + BackupPath + ".json", json);
        }
    }

    public static void UpdateSkillID(string oldID, string newID)
    {
        if (Skills.ContainsKey(oldID))
        {
            Skills.Add(newID, Skills[oldID]);
            Skills.Remove(oldID);

            foreach (var entity in Entities)
            {
            }
        }
    }

    public static void DeleteSkill(string skillID)
    {
        if (Skills.ContainsKey(skillID))
        {
            Skills.Remove(skillID);

            foreach (var entity in Entities)
            {
            }
        }
    }
#endif
#endregion

    public static EntityData GetEntityData(string entityID)
    {
        if (Entities.ContainsKey(entityID))
        {
            return Entities[entityID];
        }
        else
        {
            Debug.LogError($"Entity ID {entityID} could not be found.");
            return null;
        }
    }

    public static SkillData GetSkillData(string skillID)
    {
        if (Skills.ContainsKey(skillID))
        {
            return Skills[skillID];
        }
        else
        {
            Debug.LogError($"Skill ID {skillID} could not be found.");
            return null;
        }
    }

    public static FactionData GetFactionData(string factionID)
    {
        if (Factions.ContainsKey(factionID))
        {
            return Factions[factionID];
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
