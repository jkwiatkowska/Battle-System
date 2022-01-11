using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] string EntityID;
    [SerializeField] int EntityLevel = 1;

    EntityUI EntityUI;
    public string EntityUID                                 { get; private set; }
    static int EntityCount = 0;
    public Dictionary<string, float> BaseAttributes         { get; private set; }
    public Dictionary<string, float> DepletablesCurrent     { get; private set; }
    public Dictionary<string, float> DepletablesMax         { get; private set; }

    protected Dictionary<string, float> SkillAvailableTime;
    public Dictionary<string, ActionResult> ActionResults   { get; protected set; }
    protected string CurrentSkill;
    protected Dictionary<string, Coroutine> SkillCoroutines;
    protected SkillChargeData SkillCharge;
    protected float SkillChargeStartTime;
    public float SkillChargeRatio                           { get; protected set; }

    public TargetingSystem TargetingSystem                  { get; protected set; }
    public Dictionary<string, List<Entity>> TaggedEntities  { get; private set; }

    public string FactionOverride                           { get; private set; }

    public float TimeOfLastAttack                           { get; private set;}


    #region Getters
    public string ID
    {
        get
        {
            return EntityID;
        }
    }

    public int Level
    {
        get
        {
            return EntityLevel;
        }
    }

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
    public TargetingSystem EntityTargetingSystem
    {
        get
        {
            return TargetingSystem;
        }
    }

    public float DepletableRatio(string depletableName)
    {
        if (DepletablesCurrent.ContainsKey(depletableName) && DepletablesMax.ContainsKey(depletableName))
        {
            return DepletablesCurrent[depletableName] / DepletablesMax[depletableName];
        }
        else
        {
            Debug.LogError($"Depletable name {depletableName} is invalid.");
            return 0.0f;
        }
    }
    #endregion

    public virtual void Setup(string entityID, int entityLevel)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;

        BaseAttributes = new Dictionary<string, float>();
        foreach (var attribute in GameData.EntityAttributes)
        {
            BaseAttributes.Add(attribute, Formulae.EntityBaseAttribute(this, attribute));
        }
        DepletablesMax = new Dictionary<string, float>();
        DepletablesCurrent = new Dictionary<string, float>();
        foreach (var depletable in GameData.EntityDepletables)
        {
            DepletablesMax.Add(depletable, Formulae.DepletableMaxValue(this, depletable));
            DepletablesCurrent.Add(depletable, Formulae.DepletableStartValue(this, depletable));
        }

        SkillAvailableTime = new Dictionary<string, float>();

        BattleSystem.Instance.AddEntity(this);

        TargetingSystem = GetComponentInChildren<TargetingSystem>();
        if (TargetingSystem == null)
        {
            Debug.LogError("TargetingSystem could not be found");
        }
        TargetingSystem.Setup(this);

        EntityUI = GetComponentInChildren<EntityUI>();
        if (EntityUI == null)
        {
            Debug.LogError("EntityUI could not be found");
        }

        SkillCoroutines = new Dictionary<string, Coroutine>();
        ActionResults = new Dictionary<string, ActionResult>();

        name = EntityUID;
    }

    protected virtual void Start()
    {
        Setup(EntityID, EntityLevel);
    }

    protected virtual void Update()
    {
        if (DepletablesCurrent != null)
        {
            foreach (var depletable in GameData.EntityDepletables)
            {
                if (DepletablesCurrent.ContainsKey(depletable))
                {
                    var recovery = Formulae.DepletableRecoveryRate(this, depletable) * Time.deltaTime;
                    if (recovery != 0.0f)
                    {
                        ApplyChangeToDepletable(depletable, recovery);
                    }
                }
            }
        }
    }

    #region Skills
    public virtual void UseSkill(string skillID)
    {
        var skillData = GameData.GetSkillData(skillID);

        if (!skillData.ParallelSkill)
        {
            CancelCurrentSkill();
            CurrentSkill = skillID;
        }

        if (skillData.NeedsTarget)
        {
            var hasTarget = TargetingSystem.EnemySelected;

            if (!hasTarget)
            {
                // Ensure target is close enough and turn toward it. 
            }
        }

        if (skillData.HasChargeTime)
        {
            SkillChargeStart(skillData.SkillChargeData);
        }
        else
        {
            StartSkillCoroutine(skillData);
        }
    }

    protected virtual void StartSkillCoroutine(SkillData skillData)
    {
        var skillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
        SkillCoroutines.Add(skillData.SkillID, skillCoroutine);
    }

    public virtual IEnumerator PreSkillChargeCoroutine(SkillData skillData)
    {
        var startTime = BattleSystem.TimeSinceStart;
        ActionResults.Clear();

        foreach (var action in skillData.SkillChargeData.PreChargeTimeline)
        {
            var timeBeforeAction = startTime + action.TimestampForEntity(this) - BattleSystem.TimeSinceStart;
            if (timeBeforeAction > 0.0f)
            {
                yield return new WaitForSeconds(timeBeforeAction);
            }

            action.Execute(this, out var actionResult);
            ActionResults[action.ActionID] = actionResult;
        }
        yield return null;
    }

    public virtual IEnumerator UseSkillCoroutine(SkillData skillData)
    {
        var startTime = BattleSystem.TimeSinceStart;
        ActionResults.Clear();

        foreach (var action in skillData.SkillTimeline)
        {
            var timeBeforeAction = startTime + action.TimestampForEntity(this) - BattleSystem.TimeSinceStart;
            if (timeBeforeAction > 0.0f)
            {
                yield return new WaitForSeconds(timeBeforeAction);
            }

            action.Execute(this, out var actionResult);
            ActionResults[action.ActionID] = actionResult;
        }

        yield return null;
    }

    #region Skill Charge
    public virtual void SkillChargeStart(SkillChargeData skillChargeData)
    {
        SkillCharge = skillChargeData;
        SkillChargeStartTime = BattleSystem.TimeSinceStart;

        // Show charge UI 
    }

    public virtual void SkillChargeUpdate()
    {
        // Update UI

        if (BattleSystem.TimeSinceStart < SkillChargeStartTime + SkillCharge.FullChargeTimeForEntity(this))
        {
            SkillChargeStop();
        }
    }

    public virtual bool SkillChargeStop()
    {
        // Hide UI

        var timeElapsed = BattleSystem.TimeSinceStart - SkillChargeStartTime;
        if (timeElapsed >= SkillCharge.RequiredChargeTimeForEntity(this))
        {
            var minCharge = SkillCharge.RequiredChargeTimeForEntity(this);
            var maxCharge = SkillCharge.FullChargeTimeForEntity(this);
            SkillChargeRatio = maxCharge - minCharge / timeElapsed - minCharge;

            var skillData = GameData.GetSkillData(CurrentSkill);
            var skillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
            SkillCoroutines[CurrentSkill] = skillCoroutine;

            SkillCharge = null;
            return true;
        }
        else
        {
            return false;
        }
    }
    #endregion

    public virtual void SetSkillAvailableTime(string skillID, float time)
    {
        SkillAvailableTime[skillID] = time;
    }

    public virtual void CancelCurrentSkill()
    {
        if (!string.IsNullOrEmpty(CurrentSkill))
        {
            CancelSkill(CurrentSkill);

            if (SkillCharge != null)
            {
                SkillChargeStop();
            }
        }
    }

    public virtual void CancelSkill(string skillID)
    {
        if (SkillCoroutines.ContainsKey(skillID))
        {
            if (SkillCoroutines[skillID] != null)
            {
                StopCoroutine(SkillCoroutines[skillID]);
            }
            SkillCoroutines.Remove(skillID);
        }
    }
    #endregion

    public virtual void DestroyEntity()
    {
        BattleSystem.Instance.RemoveEntity(EntityUID);
    }

    #region Change Functions
    public void ApplyChangeToDepletable(string depletable, float change)
    {
        var previous = DepletablesCurrent[depletable];
        DepletablesCurrent[depletable] = Mathf.Clamp(DepletablesCurrent[depletable] + change, 0, DepletablesMax[depletable]);
        if (previous != DepletablesCurrent[depletable])
        {
            if (EntityUI != null)
            {
                EntityUI.UpdateDepletableUI(depletable);
            }
            else
            {
                Debug.LogError($"Entity {EntityUID} is missing EntityUI.");
            }
        }
    }
    #endregion

    #region Bool Checks
    public virtual bool IsInCombat()
    {
        return false;
    }

    protected virtual bool IsSkillOnCooldown(string skillID)
    {
        // Not on cooldown if cooldown hasn't been registered
        if (!SkillAvailableTime.ContainsKey(skillID))
        {
            return false;
        }

        return SkillAvailableTime[skillID] > BattleSystem.TimeSinceStart;
    }

    protected virtual bool CanAffordCost(List<ActionCostCollection> costActions)
    {
        var costs = new Dictionary<string, float>();
        foreach (var cost in costActions)
        {
            if (costs.ContainsKey(cost.DepletableName))
            {
                costs[cost.DepletableName] += cost.GetValue(this);
            }
            else
            {
                costs.Add(cost.DepletableName, cost.GetValue(this));
            }
        }
        foreach (var cost in costs)
        {
            if (cost.Value > DepletablesCurrent[cost.Key])
            {
                return false;
            }
        }
        return true;
    }

    public virtual bool CanUseSkill(string skillID)
    {
        if (IsSkillOnCooldown(skillID))
        {
            return false;
        }

        if (!CanAffordCost(GameData.GetSkillData(skillID).SkillCost))
        {
            return false;
        }

        return true;
    }
    #endregion
}
