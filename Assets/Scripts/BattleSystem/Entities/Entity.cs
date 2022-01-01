using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] string EntityID;
    public string EntityUID                         { get; private set; }
    static int EntityCount = 0;
    public Dictionary<string, float> Attributes     { get; private set; }
    public Dictionary<string, Vector2> Depletables  { get; private set; }   // Current and max value.
    public int EntityLevel                          { get; private set; }

    protected Dictionary<string, float> SkillCooldown;
    protected Coroutine SkillCoroutine;

    [SerializeField] protected TargetingSystem TargetingSystem;

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

    private void Start()
    {
        Setup(EntityID, EntityLevel);
    }

    public virtual void Setup(string entityID, int entityLevel)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;

        Attributes = EntityData.GetAttributesForLevel(entityLevel);
        Depletables = EntityData.GetDepletablesForLevel(entityLevel);

        SkillCooldown = new Dictionary<string, float>();

        BattleSystem.Instance.AddEntity(this);
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
        TargetingSystem.UpdateEntityLists();
        SkillCoroutine = StartCoroutine(SkillSystem.UseSkillCoroutine(skillID, this, TargetingSystem.SelectedTarget, TargetingSystem.EnemyEntities, TargetingSystem.FriendlyEntities));
    }

    public void CancelSkill()
    {
        if (SkillCoroutine != null)
        {
            StopCoroutine(SkillCoroutine);
            SkillSystem.CancelSkillCast(this);
        }
    }

    public void Destroy()
    {
        BattleSystem.Instance.RemoveEntity(EntityUID);
    }
}
