using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] string DataPath;
    public static BattleSystem Instance                         { get; private set; }

    public static Dictionary<string, Entity> Entities           { get; private set; }
    public static List<Entity> TargetableEntities               { get; private set; }

    public static float Time                                    { get; private set; }
    static float TimeMultiplier;

    void Awake()
    {
        GameData.LoadMockData();

        Instance = this;
        Entities = new Dictionary<string, Entity>();
        TargetableEntities = new List<Entity>();
        Time = 0.0f;
        TimeMultiplier = 1.0f;
    }
    
    void Update()
    {
        Time += UnityEngine.Time.deltaTime * TimeMultiplier;
    }

    public Entity SpawnEntity(string entityID, int entityLevel)
    {
        var path = $"Entities/{entityID}";
        var prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"A prefab for entity {entityID} could not be found in Assets/Resources/Entities/");
        }

        var entity = prefab.GetComponentInChildren<Entity>();
        if (entity == null)
        {
            Debug.LogError($"A prefab for entity {entityID} does not have an Entity component.");
        }

        entity.Setup(entityID, entityLevel);

        return entity;
    }

    public void AddEntity(Entity entity)
    {
        Entities.Add(entity.EntityUID, entity);

        if (entity.EntityData.IsTargetable)
        {
            TargetableEntities.Add(entity);
        }
    }

    public void RemoveEntity(Entity entity)
    {
        Entities.Remove(entity.EntityUID);

        TargetableEntities.Remove(entity);
    }

    public static bool IsFriendly(string entityUID, string targetUID)
    {
        if (entityUID == targetUID)
        {
            return true;
        }

        var entity = Entities[entityUID];
        var entityFaction = entity.FactionData.FactionID;

        var targetEntity = Entities[targetUID];
        var targetFaction = targetEntity.FactionData.FactionID;

        if (entityFaction == targetFaction)
        {
            return true;
        }
        if (GameData.GetFactionData(entityFaction).FriendlyFactions.Contains(targetFaction))
        {
            return true;
        }

        return false;
    }

    public static bool IsEnemy(string entityUID, string targetUID)
    {
        if (entityUID == targetUID)
        {
            return false;
        }

        var entity = Entities[entityUID];
        var entityFaction = entity.FactionData.FactionID;

        var targetEntity = Entities[targetUID];
        var targetFaction = targetEntity.FactionData.FactionID;

        if (entityFaction == targetFaction)
        {
            return false;
        }
        if (GameData.GetFactionData(entityFaction).EnemyFactions.Contains(targetFaction))
        {
            return true;
        }

        return false;
    }

    public static bool IsOnPlayerSide(string factionID)
    {
        if (factionID == GameData.PlayerFaction)
        {
            return true;
        }

        if (GameData.GetFactionData(GameData.PlayerFaction).FriendlyFactions.Contains(factionID))
        {
            return true;
        }

        return false;
    }
}
