using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] public string EntityID         { get; private set; }
    public string EntityUID                         { get; private set; }
    public Dictionary<string, float> Attributes     { get; private set; }
    public Dictionary<string, Vector2> Depletables  { get; private set; } // Current and max value.
    public int EntityLevel                          { get; private set; }

    Dictionary<string, float> SkillCooldown;
    Coroutine SkillCoroutine;

    Entity SelectedEntity;
    List<Entity> EnemyEntities;
    List<Entity> FriendlyEntities;

    public string FactionOverride                   { get; private set; }

    public EntityData EntityData
    {
        get
        {
            return GameData.GetEntityData(EntityID);
        }
    }
    public FactionData FactionData
    {
        get
        {
            return GameData.GetFactionData(EntityID);
        }
    }
    public float GetDepletableCurrent(string depletableName)
    {
        return Depletables[depletableName].x;
    }

    public float GetDepletableMax(string depletableName)
    {
        return Depletables[depletableName].y;
    }

    public void Setup(string entityID, int entityLevel)
    {
        EntityID = entityID;
        EntityLevel = entityLevel;

        Attributes = EntityData.GetAttributesForLevel(entityLevel);
        Depletables = EntityData.GetDepletablesForLevel(entityLevel);

        SkillCooldown = new Dictionary<string, float>();
        foreach(var skill in EntityData.Skills)
        {
            SkillCooldown.Add(skill, 0);
        }
    }

    public bool CanUseSkill(string skillID)
    {
        // Skill on cooldown
        if (SkillCooldown[skillID] > 0)
        {
            return false;
        }

        // Cost too high
        var skill = GameData.GetSkillData(skillID);
        if (skill.Cost > Depletables[skill.CostType].x)
        {
            return false;
        }

        return true;
    }

    public void UseSkill(string skillID)
    {
        SkillCoroutine = StartCoroutine(SkillSystem.UseSkillCoroutine(skillID, this, SelectedEntity, EnemyEntities, FriendlyEntities));
    }

    public void CancelSkill()
    {
        if (SkillCoroutine != null)
        {
            StopCoroutine(SkillCoroutine);
            SkillSystem.CancelSkillCast(this);
        }
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    public void Destroy()
    {

    }
}
