using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] static string DataPath;
    public static BattleSystem Instance                         { get; private set; }
    public LayerMask TerrainLayers;

    public static Dictionary<string, Entity> Entities           { get; private set; }
    public static List<Entity> TargetableEntities               { get; private set; }
    public static Dictionary<string, GameObject> EntityPrefabs  { get; private set; }

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

        LoadEntityPrefabs();
    }
    
    void Update()
    {
        Time += UnityEngine.Time.deltaTime * TimeMultiplier;
    }

    public static void LoadEntityPrefabs()
    {
        EntityPrefabs = new Dictionary<string, GameObject>();

        foreach (var entity in GameData.EntityData)
        {
            var path = $"Entities/{entity.Key}";
            var prefab = Resources.Load<GameObject>(path);

            EntityPrefabs.Add(entity.Key, prefab);
        }
    }

    public static Entity SpawnEntity(string entityID)
    {
        // To do: add pooling

        var summon = Instantiate(EntityPrefabs[entityID]);

        var entity = summon.GetComponentInChildren<Entity>();
        if (entity == null)
        {
            Debug.LogError($"A prefab for entity {entityID} does not have an Entity component.");
        }

        return entity;
    }

    public static void AddEntity(Entity entity)
    {
        if (!Entities.ContainsKey(entity.EntityUID))
        {
            Entities.Add(entity.EntityUID, entity);

            if (entity.EntityData.IsTargetable)
            {
                TargetableEntities.Add(entity);
            }
        }
    }

    public static void RemoveEntity(Entity entity)
    {
        Entities.Remove(entity.EntityUID);

        TargetableEntities.Remove(entity);
    }

    public static bool IsFriendly(string entityFaction, string targetFaction)
    {
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

    public static bool IsEnemy(string entityFaction, string targetFaction)
    {
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

    public static bool IsOnTerrainLayer(GameObject gameObject)
    {
        return Instance.TerrainLayers == (Instance.TerrainLayers | (1 << gameObject.layer));
    }
}
