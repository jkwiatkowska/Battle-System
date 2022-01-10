using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] string EntityID;
    [SerializeField] Animator Animator;
    [SerializeField] AudioRenderer AudioRenderer;
    public string EntityUID                                 { get; private set; }
    static int EntityCount = 0;
    public Dictionary<string, float> Attributes             { get; private set; }
    public Dictionary<string, float> DepletablesCurrent     { get; private set; }
    public Dictionary<string, float> DepletablesMax         { get; private set; }
    public int EntityLevel                                  { get; private set; }

    protected Dictionary<string, float> SkillAvailableTime;
    public Dictionary<string, ActionResult> ActionResults   { get; private set; }
    protected string CurrentSkill;
    protected Dictionary<string, Coroutine> SkillCoroutines;
    protected SkillChargeData SkillCharge                   { get; private set; }
    protected float SkillChargeStartTime                    { get; private set; }
    public float SkillChargeRatio                           { get; private set; }

    [SerializeField] protected TargetingSystem TargetingSystem;
    public Dictionary<string, List<Entity>> TaggedEntities  { get; private set; }

    public string FactionOverride                           { get; private set; }

    public float TimeOfLastAttack                           { get; private set;}


    #region Getters
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

        Attributes = EntityData.GetAttributesForLevel(entityLevel);
        DepletablesCurrent = EntityData.GetStartingDepletablesForLevel(entityLevel);
        DepletablesMax = EntityData.GetMaxDepletablesForLevel(entityLevel);

        SkillAvailableTime = new Dictionary<string, float>();

        BattleSystem.Instance.AddEntity(this);

        TargetingSystem.Setup(this);

        name = EntityUID;
    }

    protected virtual void Start()
    {
        Setup(EntityID, EntityLevel);
    }

    protected virtual void Update()
    {
        var isInCombat = IsInCombat();

        if (DepletablesCurrent != null)
        {
            foreach (var depletable in DepletablesCurrent)
            {
                if (isInCombat)
                {
                    var recovery = EntityData.DepletableRecovery[depletable.Key].x * Time.deltaTime;
                    if (recovery != 0.0f)
                    {
                        ApplyChangeToDepletable(depletable.Key, recovery);
                    }
                }
                else
                {
                    var recovery = EntityData.DepletableRecovery[depletable.Key].y * Time.deltaTime;
                    if (recovery != 0.0f)
                    {
                        ApplyChangeToDepletable(depletable.Key, recovery);
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
            var timeBeforeAction = startTime + action.Timestamp - BattleSystem.TimeSinceStart;
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
            var timeBeforeAction = startTime + action.Timestamp - BattleSystem.TimeSinceStart;
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

        if (BattleSystem.TimeSinceStart < SkillChargeStartTime + SkillCharge.FullChargeTime)
        {
            SkillChargeStop();
        }
    }

    public virtual bool SkillChargeStop()
    {
        // Hide UI

        var timeElapsed = BattleSystem.TimeSinceStart - SkillChargeStartTime;
        if (timeElapsed >= SkillCharge.RequiredChargeTime)
        {
            var minCharge = SkillCharge.RequiredChargeTime;
            var maxCharge = SkillCharge.FullChargeTime;
            SkillChargeRatio = maxCharge - minCharge / timeElapsed - minCharge;

            var skillData = GameData.GetSkillData(CurrentSkill);
            var skillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
            SkillCoroutines[CurrentSkill] = skillCoroutine;

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

            SkillChargeStop();
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
        DepletablesCurrent[depletable] = Mathf.Clamp(DepletablesCurrent[depletable] + change, 0, DepletablesMax[depletable]);
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
