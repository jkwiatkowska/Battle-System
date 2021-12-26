using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public static BattleSystem Instance;

    Dictionary<string, Entity> Entities;
    Dictionary<string, List<string>> Factions;

    void Awake()
    {
        Instance = new BattleSystem();
        Entities = new Dictionary<string, Entity>();
        Factions = new Dictionary<string, List<string>>();
    }
    
    void Update()
    {
        
    }

    public void AddEntity(Entity entity)
    {
        Entities.Add(entity.EntityUID, entity);
    }

    public void RemoveEntity(string entityUID)
    {
        Entities.Remove(entityUID);
    }

    public List<string> GetFriendlyEntities(string entityID)
    {
        var entityData = GameData.GetEntityData(entityID);        
        var friendlyFactions = GameData.GetFactionData(entityData.Faction).FriendlyFactions;

        var friendlyEntities = new List<string>();

        friendlyEntities.AddRange(Factions[entityData.Faction]);

        foreach (var faction in friendlyFactions)
        {
            friendlyEntities.AddRange(Factions[faction]);
        }

        return friendlyEntities;
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
}
