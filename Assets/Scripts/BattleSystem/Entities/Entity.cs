using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum eEntityState
    {
        Idle,
        ChargingSkill,
        CastingSkill,
        Dead
    }

    [SerializeField] string EntityID;
    [SerializeField] int EntityLevel = 1;

    public string EntityUID                                 { get; private set; }
    static int EntityCount = 0;

    // Attributes
    public Dictionary<string, float> BaseAttributes         { get; private set; }
    public Dictionary<string, float> DepletablesCurrent     { get; private set; }
    public Dictionary<string, float> DepletablesMax         { get; private set; }

    // Skills
    protected Dictionary<string, float> SkillAvailableTime;
    public Dictionary<string, ActionResult> ActionResults   { get; protected set; }
    protected string CurrentSkill;
    protected Dictionary<string, Coroutine> SkillCoroutines;
    protected SkillChargeData SkillCharge;
    protected float SkillChargeStartTime;
    public float SkillChargeRatio                           { get; protected set; }

    // Other objects that make up an entity
    EntityCanvas EntityCanvas;
    public TargetingSystem TargetingSystem                  { get; protected set; }
    public Dictionary<string, List<Entity>> TaggedEntities  { get; private set; }

    // State
    public eEntityState EntityState                         { get; private set; }
    public string FactionOverride                           { get; private set; }

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

        EntityCanvas = GetComponentInChildren<EntityCanvas>();
        if (EntityCanvas == null)
        {
            Debug.LogError("EntityCanvas could not be found");
        }
        else
        {
            EntityCanvas.Setup(this);
        }

        SkillCoroutines = new Dictionary<string, Coroutine>();
        ActionResults = new Dictionary<string, ActionResult>();

        EntityState = eEntityState.Idle;

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
                        ApplyChangeToDepletable(depletable, ref recovery);
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
            SkillChargeStart(skillData);
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

    public virtual IEnumerator ExecuteActionsCoroutine(List<Action> timeline)
    {
        var startTime = BattleSystem.TimeSinceStart;
        ActionResults.Clear();

        foreach (var action in timeline)
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
        if (!skillData.ParallelSkill)
        {
            EntityState = eEntityState.CastingSkill;
        }

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

        if (!skillData.ParallelSkill)
        {
            EntityState = eEntityState.Idle;
        }
    }

    #region Skill Charge
    public virtual void SkillChargeStart(SkillData skillData)
    {
        EntityState = eEntityState.ChargingSkill;
        SkillCharge = skillData.SkillChargeData;
        SkillChargeStartTime = BattleSystem.TimeSinceStart;

        SkillCoroutines[skillData.SkillID] = StartCoroutine(ExecuteActionsCoroutine(skillData.SkillChargeData.PreChargeTimeline));

        // Show charge display 
    }

    public virtual void SkillChargeUpdate()
    {
        // Update charge display

        if (BattleSystem.TimeSinceStart < SkillChargeStartTime + SkillCharge.FullChargeTimeForEntity(this))
        {
            SkillChargeStop();
        }
    }

    public virtual bool SkillChargeStop()
    {
        // Hide charge display

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

    #region Change Functions
    public void ApplyChangeToDepletable(string depletable, ref float change)
    {
        var previous = DepletablesCurrent[depletable];
        DepletablesCurrent[depletable] = Mathf.Clamp(DepletablesCurrent[depletable] + change, 0.0f, DepletablesMax[depletable]);
        if (previous != DepletablesCurrent[depletable])
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.UpdateDepletableDisplay(depletable);
            }
            else
            {
                Debug.LogError($"Entity {EntityUID} is missing EntityDisplay.");
            }

            change = previous - DepletablesCurrent[depletable];

            if (DepletablesCurrent[depletable] <= 0.0f && EntityData.LifeDepletables.Contains(depletable))
            {
                Die();
            }
        }
        else
        {
            change = 0.0f;
        }
    }
    public virtual void Die()
    {
        EntityState = eEntityState.Dead;

        StartCoroutine(ExecuteActionsCoroutine(EntityData.OnDeath));
    }

    public virtual void DestroyEntity()
    {
        BattleSystem.Instance.RemoveEntity(this);
        Destroy(gameObject);
    }
    #endregion

    #region Getters and Checks
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
            if (FactionOverride != null)
            {
                return GameData.GetFactionData(FactionOverride);
            }
            else
            {
                return GameData.GetFactionData(EntityID);
            }
        }
    }

    public TargetingSystem EntityTargetingSystem
    {
        get
        {
            return TargetingSystem;
        }
    }

    public bool Alive
    {
        get
        {
            return EntityState != eEntityState.Dead;
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
