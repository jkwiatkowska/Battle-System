using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public string EntityUID                                                 { get; private set; }
    static int EntityCount = 0;

    // Attributes
    public Dictionary<string, float> BaseAttributes                         { get; private set; }
    public Dictionary<string, float> DepletablesCurrent                     { get; private set; }
    public Dictionary<string, float> DepletablesMax                         { get; private set; }

    // Skills
    protected Dictionary<string, float> SkillAvailableTime;
    public Dictionary<string, ActionResult> ActionResults                   { get; protected set; }
    public string CurrentSkill                                              { get; protected set; }
    protected Coroutine SkillCoroutine;
    protected Coroutine SkillChargeCoroutine;
    public SkillChargeData SkillCharge                                      { get; protected set; }
    public float SkillChargeStartTime                                       { get; protected set; }
    public float SkillChargeRatio                                           { get; protected set; }

    // Triggers
    protected Dictionary<TriggerData.eTrigger, List<Trigger>> Triggers;

    // Other objects that make up an entity
    public EntityCanvas EntityCanvas                                        { get; protected set; }
    public TargetingSystem TargetingSystem                                  { get; protected set; }
    public Targetable Targetable                                            { get; protected set; }
    public EntityMovement EntityMovement                                    { get; protected set; }

    // State
    public eEntityState EntityState                                         { get; private set; }
    public bool IsTargetable                                                { get; private set; }
    public string FactionOverride                                           { get; private set; }
    public Dictionary<string, Dictionary<string, float>> TaggedEntities     { get; protected set; }
    protected Dictionary<string, List<string>> TagsApplied;

    public virtual void Setup(string entityID, int entityLevel)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;
        EntityState = eEntityState.Idle;
        name = EntityUID;

        // Attributes, skills, triggers and tags
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

        TaggedEntities = new Dictionary<string, Dictionary<string, float>>();
        TagsApplied = new Dictionary<string, List<string>>();

        // Dependencies 
        BattleSystem.Instance.AddEntity(this);

        TargetingSystem = GetComponentInChildren<TargetingSystem>();
        if (TargetingSystem == null)
        {
            Debug.LogError($"TargetingSystem could not be found for {EntityUID}");
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
        if (Alive && DepletablesCurrent != null)
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
    public virtual bool TryUseSkill(string skillID)
    {
        // Return if already using this skill
        if (CurrentSkill == skillID)
        {
            return false;
        }

        // Make sure the skill isn't on cooldown and any mandatory costs can be afforded. 
        if (!CanUseSkill(skillID))
        {
            return false;
        }

        // If already casting another skill, interrupt it. 
        CancelSkill();

        var skillData = GameData.GetSkillData(skillID);

        // Ensure target if one is required.
        TargetingSystem.UpdateEntityLists();

        if (skillData.NeedsTarget)
        {
            var hasTarget = TargetingSystem.EnemySelected;

            // If there is no target, try selecting one.
            if (!hasTarget)
            {
                TargetingSystem.SelectBestEnemy();
            }

            // If one couldn't be found, a skill cannot be used.
            if (Target == null)
            {
                return false;
            }
        }

        // Ensure target is in range if a target is required or a preferred target is selected
        if (Target != null)
        {
            var checkRange = skillData.NeedsTarget || (skillData.PreferredTarget == SkillData.eTargetPreferrence.Friendly && TargetingSystem.FriendlySelected) ||
                                                      (skillData.PreferredTarget == SkillData.eTargetPreferrence.Enemy && TargetingSystem.EnemySelected);
            if (checkRange)
            {
                // Return if target is out of range.
                if (!Utility.IsInRange(this, Target, skillData.Range))
                {
                    OnTargetOutOfRange();
                    return false;
                }
            }
        }

        CurrentSkill = skillID;

        SkillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
        return true;
    }

    public virtual IEnumerator UseSkillCoroutine(SkillData skillData)
    {
        // Rotate toward target
        if (Target != null)
        {
            yield return EntityMovement.RotateTowardCoroutine(Target.transform.position);
        }

        // Charge skill
        if (skillData.HasChargeTime)
        {
            yield return ChargeSkillCoroutine(skillData);
        }

        EntityState = eEntityState.CastingSkill;

        yield return skillData.SkillTimeline.ExecuteActions(this, Target);

        CurrentSkill = null;
        EntityState = eEntityState.Idle;
    }

    protected virtual IEnumerator ChargeSkillCoroutine(SkillData skillData)
    {
        EntityState = eEntityState.ChargingSkill;
        SkillCharge = skillData.SkillChargeData;
        SkillChargeStartTime = BattleSystem.Time;

        EntityCanvas.StartSkillCharge(SkillCharge, CurrentSkill);

        SkillChargeCoroutine = StartCoroutine(SkillCharge.PreChargeTimeline.ExecuteActions(this, Target));

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
        if (EntityState == eEntityState.ChargingSkill)
        {
            EntityCanvas.StopSkillCharge();

            if (SkillChargeCoroutine != null)
            {
                StopCoroutine(SkillChargeCoroutine);
            }
        }

        if (SkillCoroutine != null)
        {
            StopCoroutine(SkillCoroutine);
        }

        CurrentSkill = null;
        EntityState = eEntityState.Idle;
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
                OnDeath();
                break;
            }
            case TriggerData.eTrigger.OnKill:
            {
                OnKill(payloadResult);
                break;
            }
        }
    }

    protected virtual void OnDeath()
    {
        EntityState = eEntityState.Dead;

        RemoveAllTagsOnSelf();
        RemoveAllTagsOnEntities();
    }

    protected virtual void OnKill(PayloadResult payloadResult)
    {
        if (payloadResult != null && Target == payloadResult.Target)
        {
            TargetingSystem.ClearSelection();
        }
    }

    protected virtual void OnTargetOutOfRange()
    {

    }
    #endregion

    #region Tagging
    protected void UpdateTags()
    {
        foreach(var tag in TaggedEntities)
        {
            foreach (var entity in tag.Value)
            {
                if (entity.Value < BattleSystem.Time)
                {
                    RemoveTagOnEntity(tag.Key, BattleSystem.Entities[entity.Key], false);
                }
            }
        }
    }

    public List<Entity> GetEntitiesWithTag(string tag)
    {
        var entities = new List<Entity>();
        if (TaggedEntities.ContainsKey(tag))
        {
            foreach (var entity in TaggedEntities[tag])
            {
                entities.Add(BattleSystem.Entities[entity.Key]);
            }
        }

        return entities;
    }

    public void TagEntity(TagData tagData, Entity entity)
    {
        if (!TaggedEntities.ContainsKey(tagData.TagID))
        {
            TaggedEntities.Add(tagData.TagID, new Dictionary<string, float>());
        }

        if (TaggedEntities[tagData.TagID].ContainsKey(entity.EntityUID))
        {
            TaggedEntities[tagData.TagID][entity.EntityUID] = BattleSystem.Time + tagData.TagDuration;
        }
        else
        {
            TaggedEntities[tagData.TagID].Add(entity.EntityUID, BattleSystem.Time + tagData.TagDuration);
            entity.ApplyTag(tagData.TagID, EntityUID);

            if (TaggedEntities[tagData.TagID].Count > tagData.TagLimit)
            {
                var entityToUntag = BattleSystem.Entities[TaggedEntities[tagData.TagID].Aggregate((l, r) => l.Value < r.Value ? l : r).Key];
                RemoveTagOnEntity(tagData.TagID, entityToUntag, false);
            }
        }
    }

    public void RemoveTagOnEntity(string tag, Entity entity, bool selfOnly)
    {
        if (entity != null)
        {
            TaggedEntities[tag].Remove(entity.EntityUID);
            if (!selfOnly)
            {
                entity.RemoveTagOnSelf(tag, EntityUID);
            }
        }
        else
        {
            Debug.LogError($"Trying to remove tag from entity, but it's null.");
        }
    }

    protected void RemoveAllTagsOnEntities()
    {
        foreach (var tag in TaggedEntities)
        {
            foreach (var entity in tag.Value)
            {
                BattleSystem.Entities[entity.Key].RemoveTagOnSelf(tag.Key, EntityUID);
            }
            tag.Value.Clear();
        }
        TaggedEntities.Clear();
    }

    public void ApplyTag(string tag, string sourceUID)
    {
        if (!TagsApplied.ContainsKey(sourceUID))
        {
            TagsApplied.Add(sourceUID, new List<string>());
        }

        if (!TagsApplied[sourceUID].Contains(tag))
        {
            TagsApplied[sourceUID].Add(tag);
        }
    }

    public void RemoveTagOnSelf(string tag, string sourceUID)
    {
        if (TagsApplied.ContainsKey(sourceUID))
        {
            TagsApplied[sourceUID].Remove(tag);
        }
    }

    protected void RemoveAllTagsOnSelf()
    {
        foreach (var source in TagsApplied)
        {
            var entity = BattleSystem.Entities[source.Key];
            foreach (var tag in source.Value)
            {
                entity.RemoveTagOnEntity(tag, this, true);
            }
            source.Value.Clear();
        }
        TagsApplied.Clear();
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
                return GameData.GetFactionData(EntityData.Faction);
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

    public Entity Target
    {
        get
        {
            if (TargetingSystem.SelectedTarget != null)
            {
                return TargetingSystem.SelectedTarget;
            }
            return this;
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
