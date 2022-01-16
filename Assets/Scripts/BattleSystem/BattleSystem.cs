using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] string DataPath;
    public static BattleSystem Instance { get; private set; }
    TargetingSystemPlayer PlayerTargeting;

    public Dictionary<string, Entity> Entities  { get; private set; }
    public List<Entity> TargetableEntities      { get; private set; }

    public static float Time { get; private set; }

    void Awake()
    {
        GameData.LoadMockData();

        Instance = this;
        Entities = new Dictionary<string, Entity>();
        TargetableEntities = new List<Entity>();
        Time = 0.0f;

        PlayerTargeting = FindObjectOfType<TargetingSystemPlayer>();
    }
    
    void Update()
    {
        Time += UnityEngine.Time.deltaTime;
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

        var entity = Instance.Entities[entityUID];
        var entityFaction = entity.FactionData.FactionID;

        var targetEntity = Instance.Entities[targetUID];
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

        var entity = Instance.Entities[entityUID];
        var entityFaction = entity.FactionData.FactionID;

        var targetEntity = Instance.Entities[targetUID];
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
