using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum eEntityState
    {
        Idle,
        Transition,
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
    public string CurrentSkill                              { get; protected set; }
    protected Coroutine SkillCoroutine;
    protected Coroutine SkillChargeCoroutine;
    public SkillChargeData SkillCharge                      { get; protected set; }
    public float SkillChargeStartTime                       { get; protected set; }
    public float SkillChargeRatio                           { get; protected set; }

    // Triggers
    protected Dictionary<TriggerData.eTrigger, List<Trigger>> Triggers;

    // Other objects that make up an entity
    public EntityCanvas EntityCanvas                        { get; protected set; }
    public TargetingSystem TargetingSystem                  { get; protected set; }
    public Targetable Targetable                            { get; protected set; }
    public EntityMovement EntityMovement                    { get; protected set; }
    public Dictionary<string, List<Entity>> TaggedEntities  { get; private set; }

    // State
    public eEntityState EntityState                         { get; private set; }
    public bool IsTargetable                                { get; private set; }
    public string FactionOverride                           { get; private set; }

    public virtual void Setup(string entityID, int entityLevel)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;
        EntityState = eEntityState.Idle;
        name = EntityUID;

        // Attributes, skills and triggers
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
        ActionResults = new Dictionary<string, ActionResult>();

        Triggers = new Dictionary<TriggerData.eTrigger, List<Trigger>>();
        foreach (var trigger in EntityData.Triggers)
        {
            AddTrigger(new Trigger(trigger));
        }
        IsTargetable = EntityData.IsTargetable;

        // Dependencies 
        BattleSystem.Instance.AddEntity(this);

        TargetingSystem = GetComponentInChildren<TargetingSystem>();
        if (TargetingSystem == null)
        {
            Debug.LogError("TargetingSystem could not be found");
        }
        TargetingSystem.Setup(this);

        EntityMovement = GetComponentInChildren<EntityMovement>();
        if (EntityMovement != null)
        {
            EntityMovement.Setup(this);
        }

        EntityCanvas = GetComponentInChildren<EntityCanvas>();
        if (EntityCanvas == null)
        {
            Debug.LogError("EntityCanvas could not be found");
        }
        else
        {
            EntityCanvas.Setup(this);
        }

        if (IsTargetable)
        {
            Targetable = GetComponentInChildren<Targetable>();
            if (Targetable != null)
            {
                Targetable.Setup(this);
            }
            else
            {
                Debug.LogError($"Entity {EntityID} marked as targetable, but it does not have a Targetable component.");
            }
        }
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
        if (CurrentSkill == skillID)
        {
            return;
        }

        var skillData = GameData.GetSkillData(skillID);

        CancelSkill();
        CurrentSkill = skillID;

        SkillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
    }

    public virtual IEnumerator UseSkillCoroutine(SkillData skillData)
    {
        // Ensure target
        TargetingSystem.UpdateEntityLists();

        if (skillData.NeedsTarget)
        {
            var hasTarget = TargetingSystem.EnemySelected;

            if (!hasTarget)
            {
                TargetingSystem.SelectBestEnemy();
            }

            if (TargetingSystem.SelectedTarget == null)
            {
                yield break;
            }

            // Rotate toward target
            yield return EntityMovement.RotateTowardCoroutine(TargetingSystem.SelectedTarget.transform.position);
        }

        // Charge skill
        if (skillData.HasChargeTime)
        {
            yield return ChargeSkillCoroutine(skillData);
        }

        EntityState = eEntityState.CastingSkill;

        yield return skillData.SkillTimeline.ExecuteActions(this);

        CurrentSkill = null;
        EntityState = eEntityState.Idle;
    }

    protected virtual IEnumerator ChargeSkillCoroutine(SkillData skillData)
    {
        EntityState = eEntityState.ChargingSkill;
        SkillCharge = skillData.SkillChargeData;
        SkillChargeStartTime = BattleSystem.Time;

        EntityCanvas.StartSkillCharge(SkillCharge, CurrentSkill);

        SkillChargeCoroutine = StartCoroutine(SkillCharge.PreChargeTimeline.ExecuteActions(this));

        bool chargeComplete;
        bool chargeCancelled;
        var fullChargeTime = SkillCharge.FullChargeTimeForEntity(this);
        do
        {
            chargeComplete = BattleSystem.Time > (SkillChargeStartTime + fullChargeTime);
            chargeCancelled = SkillCharge.MovementCancelsCharge && EntityMovement != null && EntityMovement.LastMoved > SkillChargeStartTime;
            yield return null;
        }
        while (!chargeComplete && !chargeCancelled);

        EntityCanvas.StopSkillCharge();

        var timeElapsed = BattleSystem.Time - SkillChargeStartTime;
        var minCharge = SkillCharge.RequiredChargeTimeForEntity(this);
        if (timeElapsed >= minCharge)
        {
            var fullCharge = SkillCharge.FullChargeTimeForEntity(this);
            SkillChargeRatio = timeElapsed / fullCharge;

            SkillCharge = null;
        }
        else
        {
            SkillCharge = null;
            CurrentSkill = null;
            StopCoroutine(SkillCoroutine);
        }
        yield return null;
    }

    public virtual void SetSkillAvailableTime(string skillID, float time)
    {
        SkillAvailableTime[skillID] = time;
    }

    public virtual void CancelSkill()
    {
        if (SkillCoroutine == null)
        {
            return;
        }

        if (SkillChargeCoroutine != null)
        {
            StopCoroutine(SkillChargeCoroutine);
        }

        StopCoroutine(SkillCoroutine);

        CurrentSkill = null;
    }

    #endregion

    #region Triggers
    public void AddTrigger(Trigger trigger)
    {
        if (!Triggers.ContainsKey(trigger.TriggerData.Trigger))
        {
            Triggers[trigger.TriggerData.Trigger] = new List<Trigger>();
        }

        Triggers[trigger.TriggerData.Trigger].Add(trigger);
    }

    public virtual void OnTrigger(TriggerData.eTrigger trigger, PayloadResult payloadResult)
    {
        // Variable triggers
        if (Triggers.ContainsKey(trigger))
        {
            foreach (var t in Triggers[trigger])
            {
                t.TryExecute(this, payloadResult, out var keep);
                if (!keep)
                {
                    Triggers[trigger].Remove(t);
                }
            }
        }

        // Hard coded triggers
        switch (trigger)
        {
            case TriggerData.eTrigger.OnDeath:
            {
                EntityState = eEntityState.Dead;
                break;
            }
            case TriggerData.eTrigger.OnKill:
            {
                if (payloadResult != null && TargetingSystem.SelectedTarget == payloadResult.Target)
                {
                    TargetingSystem.ClearSelection();
                }
                break;
            }
        }
    }
    #endregion

    #region Change Functions
    public void ApplyChangeToDepletable(string depletable, PayloadResult payloadResult)
    {
        var previous = DepletablesCurrent[depletable];
        DepletablesCurrent[depletable] = Mathf.Clamp(DepletablesCurrent[depletable] + payloadResult.Change, 0.0f, DepletablesMax[depletable]);
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

            payloadResult.Change = previous - DepletablesCurrent[depletable];

            if (payloadResult.Change > 0.0f)
            {
                OnTrigger(TriggerData.eTrigger.OnRecoveryReceived, payloadResult);
                payloadResult.Caster.OnTrigger(TriggerData.eTrigger.OnRecoveryDealt, payloadResult);
            }
            else if (payloadResult.Change < 0.0f)
            {
                OnTrigger(TriggerData.eTrigger.OnDamageReceived, payloadResult);
                payloadResult.Caster.OnTrigger(TriggerData.eTrigger.OnRecoveryReceived, payloadResult);
            }

            if (DepletablesCurrent[depletable] <= 0.0f && EntityData.LifeDepletables.Contains(depletable))
            {
                OnTrigger(TriggerData.eTrigger.OnDeath, payloadResult);
                payloadResult.Caster.OnTrigger(TriggerData.eTrigger.OnKill, payloadResult);
            }
        }
        else
        {
            payloadResult.Change = 0.0f;
        }
    }

    public void ApplyChangeToDepletable(string depletable, float change)
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

            if (DepletablesCurrent[depletable] <= 0.0f && EntityData.LifeDepletables.Contains(depletable))
            {
                EntityState = eEntityState.Dead;
                OnTrigger(TriggerData.eTrigger.OnDeath, null);
            }
        }
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

        return SkillAvailableTime[skillID] > BattleSystem.Time;
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
