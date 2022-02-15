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
    public EntityData EntityData { get; protected set; }

    public string EntityUID                                                 { get; private set; }
    static int EntityCount = 0;

    // Attributes
    protected Dictionary<string, float> BaseAttributes;
    public Dictionary<string, float> ResourcesCurrent                       { get; protected set; }
    public Dictionary<string, float> ResourcesMax                           { get; protected set; }

    // Skills
    protected Dictionary<string, float> SkillAvailableTime;
    public SkillData CurrentSkill                                           { get; protected set; }
    protected Coroutine SkillCoroutine;
    protected Coroutine SkillChargeCoroutine;
    public SkillChargeData SkillCharge                                      { get; protected set; }
    public float SkillStartTime                                             { get; protected set; }
    public float SkillChargeRatio                                           { get; protected set; }

    // Status effects
    protected Dictionary<string, StatusEffect> StatusEffects;
    protected Dictionary<string, Dictionary<string, AttributeChange>> AttributeChanges;

    // Triggers
    protected Dictionary<TriggerData.eTrigger, List<Trigger>> Triggers;

    // Other objects that make up an entity
    public EntityCanvas EntityCanvas                                        { get; protected set; }
    public TargetingSystem EntityTargetingSystem                            { get; protected set; }
    public Targetable Targetable                                            { get; protected set; }
    public MovementEntity Movement                                          { get; protected set; }

    // State
    public eEntityState EntityState                                         { get; protected set; }
    public bool IsTargetable                                                { get; protected set; }
    protected string EntityFaction;
    protected string FactionOverride;
    protected bool SetupComplete;

    // Connections with other entities
    public Dictionary<string, Dictionary<string, float>> TaggedEntities     { get; protected set; }
    protected Dictionary<string, List<string>> TagsAppliedBy;

    public Dictionary<string, List<EntitySummon>> SummonedEntities          { get; protected set; }

    public virtual void Setup(string entityID, int entityLevel, Entity source = null)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;
        EntityState = eEntityState.Idle;
        name = EntityUID;
        EntityData = GameData.GetEntityData(entityID);
        EntityFaction = EntityData.Faction;

        // Attributes
        BaseAttributes = new Dictionary<string, float>();
        foreach (var attribute in GameData.EntityAttributes)
        {
            BaseAttributes.Add(attribute, Formulae.EntityBaseAttribute(this, attribute));
        }

        AttributeChanges = new Dictionary<string, Dictionary<string, AttributeChange>>();

        SetupResourcesMax();
        SetupResourcesStart();

        // Skills
        SkillAvailableTime = new Dictionary<string, float>();

        // Status effects
        StatusEffects = new Dictionary<string, StatusEffect>();

        // Triggers
        Triggers = new Dictionary<TriggerData.eTrigger, List<Trigger>>();
        foreach (var trigger in EntityData.Triggers)
        {
            AddTrigger(new Trigger(trigger));
        }
        IsTargetable = EntityData.IsTargetable;

        // Connections
        TaggedEntities = new Dictionary<string, Dictionary<string, float>>();
        TagsAppliedBy = new Dictionary<string, List<string>>();
        SummonedEntities = new Dictionary<string, List<EntitySummon>>();

        // Dependencies and components
        BattleSystem.AddEntity(this);

        EntityTargetingSystem = GetComponentInChildren<TargetingSystem>();
        if (EntityTargetingSystem == null)
        {
            Debug.LogError($"TargetingSystem could not be found for {EntityUID}");
        }
        EntityTargetingSystem.Setup(this);

        Movement = GetComponentInChildren<MovementEntity>();
        if (Movement != null)
        {
            Movement.Setup(this);
        }

        EntityCanvas = GetComponentInChildren<EntityCanvas>();
        if (EntityCanvas != null)
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

        SetupComplete = true;
        OnTrigger(TriggerData.eTrigger.OnSpawn, source == null ? this : source);
    }

    protected virtual void Start()
    {
        if (!SetupComplete)
        {
            Setup(EntityID, EntityLevel, this);
        }
    }

    protected virtual void Update()
    {
        if (SetupComplete && Alive)
        {
            if (ResourcesCurrent != null)
            {
                foreach (var resource in GameData.EntityResources)
                {
                    if (ResourcesCurrent.ContainsKey(resource.Key))
                    {
                        var recovery = Formulae.ResourceRecoveryRate(this, resource.Key) * Time.deltaTime;
                        if (recovery != 0.0f)
                        {
                            ApplyChangeToResource(resource.Key, recovery);
                        }
                    }
                }
            }

            if (EntityState == eEntityState.CastingSkill)
            {
                if (CurrentSkill.MovementCancelsSkill)
                {
                    var skillCancelled = Movement != null && (SkillStartTime < Movement.LastJumped || SkillStartTime < Movement.LastMoved);
                    if (skillCancelled)
                    {
                        CancelSkill();
                    }
                }
            }
        }
    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void FixedUpdate()
    {

    }

    #region Skills
    public virtual bool TryUseSkill(string skillID)
    {
        // Return if already using this skill
        if (CurrentSkill != null && CurrentSkill.SkillID == skillID)
        {
            return false;
        }

        var skillData = GameData.GetSkillData(skillID);

        // Make sure the skill isn't on cooldown and any mandatory costs can be afforded. 
        if (!CanUseSkill(skillData))
        {
            return false;
        }

        // If already casting another skill, interrupt it. 
        CancelSkill();

        // Ensure target if one is required.
        EntityTargetingSystem.UpdateEntityLists();

        if (skillData.NeedsTarget)
        {
            var hasTarget = EntityTargetingSystem.EnemySelected;

            // If there is no target, try selecting one.
            if (!hasTarget)
            {
                EntityTargetingSystem.SelectBestEnemy();
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
            var checkRange = skillData.NeedsTarget;
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

        SkillCoroutine = StartCoroutine(UseSkillCoroutine(skillData));
        return true;
    }

    public virtual IEnumerator UseSkillCoroutine(SkillData skillData)
    {
        CurrentSkill = skillData;

        // Rotate toward target
        if (Target != null && Movement != null && skillData.PreferredTarget != SkillData.eTargetPreferrence.None)
        {
            yield return Movement.RotateTowardCoroutine(Target.transform.position);
        }

        // Charge skill
        if (skillData.HasChargeTime)
        {
            yield return ChargeSkillCoroutine(skillData);
        }

        EntityState = eEntityState.CastingSkill;
        SkillStartTime = BattleSystem.Time;

        yield return skillData.SkillTimeline.ExecuteActions(this, Target);

        CurrentSkill = null;
        EntityState = eEntityState.Idle;
    }

    protected virtual IEnumerator ChargeSkillCoroutine(SkillData skillData)
    {
        EntityState = eEntityState.ChargingSkill;
        SkillCharge = skillData.SkillChargeData;
        SkillStartTime = BattleSystem.Time;

        if (EntityCanvas != null)
        {
            EntityCanvas.StartSkillCharge(SkillCharge, skillData.SkillID);
        }
        SkillChargeCoroutine = StartCoroutine(SkillCharge.PreChargeTimeline.ExecuteActions(this, Target));

        bool chargeComplete;
        bool chargeCancelled;
        var fullChargeTime = SkillCharge.FullChargeTimeForEntity(this);
        do
        {
            chargeComplete = BattleSystem.Time > (SkillStartTime + fullChargeTime);
            chargeCancelled = SkillCharge.MovementCancelsCharge && Movement != null && 
                             (Movement.LastMoved > SkillStartTime || Movement.LastJumped > SkillStartTime);
            yield return null;
        }
        while (!chargeComplete && !chargeCancelled);

        if (EntityCanvas != null)
        {
            EntityCanvas.StopSkillCharge();
        }

        var timeElapsed = BattleSystem.Time - SkillStartTime;
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

    public virtual void ModifySkillAvailableTime(string skillID, float change)
    {
        if (!IsSkillOnCooldown(skillID))
        {
            SkillAvailableTime[skillID] = BattleSystem.Time;
        }

        SkillAvailableTime[skillID] += change;
    }

    public virtual void SetSkillAvailableTime(string skillID, float time)
    {
        SkillAvailableTime[skillID] = time;
    }

    public virtual void CancelSkill()
    {
        if (CurrentSkill == null)
        {
            return;
        }

        if (EntityState == eEntityState.ChargingSkill)
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.StopSkillCharge();
            }

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

    public virtual void OnTrigger(TriggerData.eTrigger trigger, Entity source, PayloadResult payloadResult = null)
    {
        //var plText = payloadResult != null ? $", action: {payloadResult.ActionID}, target: {payloadResult.Target}" : "";
        //Debug.Log($"{name} got trigger: {trigger}, source: {source.EntityUID}{plText}, time: {BattleSystem.Time}");
        // Variable triggers
        if (Triggers.ContainsKey(trigger))
        {
            var target = Target;
            if (payloadResult != null)
            {
                if (trigger == TriggerData.eTrigger.OnHitIncoming || trigger == TriggerData.eTrigger.OnDamageTaken || 
                    trigger == TriggerData.eTrigger.OnRecoveryReceived || trigger == TriggerData.eTrigger.OnDeath)
                {
                    target = payloadResult.Caster;
                }
            }

            foreach (var t in Triggers[trigger])
            {
                t.TryExecute(this, target, payloadResult, out var keep);
                if (!keep)
                {
                    Triggers[trigger].Remove(t);
                }
            }
        }

        // Hard coded triggers
        switch (trigger)
        {
            case TriggerData.eTrigger.OnHitOutgoing:
            {
                OnHitOutgoing(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnHitIncoming:
            {
                OnHitIncoming(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnHitMissed:
            {
                OnHitMissed();
                break;
            }
            case TriggerData.eTrigger.OnDamageDealt:
            {
                OnDamageDealt(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnDamageTaken:
            {
                OnDamageTaken(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnRecoveryGiven:
            {
                OnRecoveryGiven(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnRecoveryReceived:
            {
                OnRecoveryReceived(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnDeath:
            {
                OnDeath(source, payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnKill:
            {
                OnKill(payloadResult);
                break;
            }
            case TriggerData.eTrigger.OnSpawn:
            {
                OnSpawn();
                break;
            }
        }
    }

    protected virtual void OnHitOutgoing(PayloadResult payloadResult)
    {

    }

    protected virtual void OnHitIncoming(PayloadResult payloadResult)
    {

    }

    protected virtual void OnHitMissed()
    {

    }

    protected virtual void OnDamageDealt(PayloadResult payloadResult)
    {

    }

    protected virtual void OnDamageTaken(PayloadResult payloadResult)
    {

    }

    protected virtual void OnRecoveryGiven(PayloadResult payloadResult)
    {

    }

    protected virtual void OnRecoveryReceived(PayloadResult payloadResult)
    {

    }

    protected virtual void OnDeath(Entity source = null, PayloadResult payloadResult = null)
    {
        EntityState = eEntityState.Dead;

        RemoveAllTagsOnSelf();
        RemoveAllTagsOnEntities();
        if (Targetable != null)
        {
            Targetable.RemoveTargeting();
        }
        KillLinkedSummons();
    }

    protected virtual void OnKill(PayloadResult payloadResult = null)
    {

    }

    protected virtual void OnSpawn()
    {

    }

    protected virtual void OnTargetOutOfRange()
    {

    }
    #endregion

    #region Status Effects
    public void ApplyStatusEffect(Action action, StatusEffectData statusEffectData, int stacks, Payload sourcePayload)
    {
        if (!StatusEffects.ContainsKey(statusEffectData.StatusID))
        {
            if (stacks < 0)
            {
                return;
            }

            StatusEffects[statusEffectData.StatusID] = new StatusEffect(target: this, sourcePayload.Source, statusEffectData, action, sourcePayload);
        }
        else if (sourcePayload.Source.EntityUID != StatusEffects[statusEffectData.StatusID].Caster.EntityUID)
        {
            StatusEffects[statusEffectData.StatusID].Setup(target: this, sourcePayload.Source, statusEffectData, action, sourcePayload);
        }

        StatusEffects[statusEffectData.StatusID].ApplyStacks(stacks);
    }

    public void ClearStatusEffect(string statusID)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            StatusEffects[statusID].ClearStatus();
        }
    }

    public void RemoveStatusEffect(string statusID)
    {
        // This has to be called from StatusEffect.RemoveStatus, after all effects are removed.
        if (StatusEffects.ContainsKey(statusID))
        {
            StatusEffects.Remove(statusID);
        }
    }

    public void ApplyAttributeChange(AttributeChange attributeChange)
    {
        if (!AttributeChanges.ContainsKey(attributeChange.Attribute))
        {
            AttributeChanges[attributeChange.Attribute] = new Dictionary<string, AttributeChange>();
        }
        AttributeChanges[attributeChange.Attribute][attributeChange.Key] = attributeChange;

        SetupResourcesMax();
    }

    public void RemoveAttributeChange(string attribute, string key)
    {
        if (!AttributeChanges.ContainsKey(attribute) || AttributeChanges[attribute].ContainsKey(key))
        {
            return;
        }

        AttributeChanges[attribute].Remove(key);

        SetupResourcesMax();
    }

    public void Convert(string faction)
    {
        FactionOverride = faction;
    }

    public void RemoveConversion()
    {
        FactionOverride = "";
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

    public virtual List<Entity> GetEntitiesWithTag(string tag)
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

    public virtual void TagEntity(TagData tagData, Entity entity)
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
        if (!TagsAppliedBy.ContainsKey(sourceUID))
        {
            TagsAppliedBy.Add(sourceUID, new List<string>());
        }

        if (!TagsAppliedBy[sourceUID].Contains(tag))
        {
            TagsAppliedBy[sourceUID].Add(tag);
        }
    }

    public void RemoveTagOnSelf(string tag, string sourceUID)
    {
        if (TagsAppliedBy.ContainsKey(sourceUID))
        {
            TagsAppliedBy[sourceUID].Remove(tag);
        }
    }

    protected void RemoveAllTagsOnSelf()
    {
        foreach (var source in TagsAppliedBy)
        {
            var entity = BattleSystem.Entities[source.Key];
            foreach (var tag in source.Value)
            {
                entity.RemoveTagOnEntity(tag, this, true);
            }
            source.Value.Clear();
        }
        TagsAppliedBy.Clear();
    }
    #endregion

    #region Summon
    public void AddSummonedEntity(EntitySummon entity, ActionSummon summonAction)
    {
        if (!SummonedEntities.ContainsKey(summonAction.EntityID))
        {
            SummonedEntities.Add(summonAction.EntityID, new List<EntitySummon>());
        }

        while (SummonedEntities[summonAction.EntityID].Count >= summonAction.SummonLimit)
        {
            var entityToRemove = SummonedEntities[summonAction.EntityID][0];
            entityToRemove.OnTrigger(TriggerData.eTrigger.OnDeath, entity);
        }

        SummonedEntities[summonAction.EntityID].Add(entity);
    }

    public void RemoveSummonedEntity(EntitySummon entity)
    {
        if (SummonedEntities.ContainsKey(entity.EntityID))
        {
            SummonedEntities[entity.EntityID].Remove(entity);
        }
    }

    void KillLinkedSummons()
    {
        var linkedSummons = new List<Entity>();
        foreach (var group in SummonedEntities)
        {
            foreach (var entity in group.Value)
            {
                if (entity.IsLinked)
                {
                    linkedSummons.Add(entity);
                }
            }
        }

        foreach (var entity in linkedSummons)
        {
            entity.OnTrigger(TriggerData.eTrigger.OnDeath, entity);
        }
    }
    #endregion

    #region Resources
    protected void SetupResourcesMax()
    {
        ResourcesMax = new Dictionary<string, float>();
        var attributes = EntityAttributes(skillID: null, actionID: null, statusID: null, categories: null);

        foreach (var resource in GameData.EntityResources)
        {
            ResourcesMax.Add(resource.Key, Formulae.ResourceMaxValue(attributes, resource.Key));
        }
    }

    protected void SetupResourcesStart()
    {
        ResourcesCurrent = new Dictionary<string, float>();
        var attributes = EntityAttributes(skillID: null, actionID: null, statusID: null, categories: null);

        foreach (var resource in GameData.EntityResources)
        {
            ResourcesCurrent.Add(resource.Key, Formulae.ResourceMaxValue(attributes, resource.Key));
        }
    }

    public void ApplyChangeToResource(string resource, PayloadResult payloadResult, bool setTriggers = true)
    {
        var previous = ResourcesCurrent[resource];
        ResourcesCurrent[resource] = Mathf.Clamp(ResourcesCurrent[resource] + payloadResult.Change, 0.0f, ResourcesMax[resource]);
        if (previous != ResourcesCurrent[resource])
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.UpdateResourceDisplay(resource);
            }

            payloadResult.Change = previous - ResourcesCurrent[resource];
            var source = payloadResult.Caster;

            if (setTriggers)
            {
                if (payloadResult.Change < -Constants.Epsilon)
                {
                    OnTrigger(TriggerData.eTrigger.OnRecoveryReceived, source, payloadResult);
                    payloadResult.Caster.OnTrigger(TriggerData.eTrigger.OnRecoveryGiven, source, payloadResult);
                }
                else if (payloadResult.Change > Constants.Epsilon)
                {
                    OnTrigger(TriggerData.eTrigger.OnDamageTaken, source, payloadResult);
                    payloadResult.Caster.OnTrigger(TriggerData.eTrigger.OnDamageDealt, source, payloadResult);
                }
            }

            if (ResourcesCurrent[resource] <= 0.0f && EntityData.LifeResources.Contains(resource))
            {
                OnTrigger(TriggerData.eTrigger.OnDeath, source, payloadResult);
                payloadResult.Caster.OnTrigger(TriggerData.eTrigger.OnKill, source, payloadResult);
            }
        }
        else
        {
            payloadResult.Change = 0.0f;
        }
    }

    public void ApplyChangeToResource(string resource, float change)
    {
        var previous = ResourcesCurrent[resource];
        ResourcesCurrent[resource] = Mathf.Clamp(ResourcesCurrent[resource] + change, 0.0f, ResourcesMax[resource]);
        if (previous != ResourcesCurrent[resource])
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.UpdateResourceDisplay(resource);
            }

            if (ResourcesCurrent[resource] <= 0.0f && EntityData.LifeResources.Contains(resource))
            {
                OnTrigger(TriggerData.eTrigger.OnDeath, null);
            }
        }
    }

    public virtual void DestroyEntity()
    {
        BattleSystem.RemoveEntity(this);
        Destroy(gameObject);
    }
    #endregion

    #region Getters and Checks
    public int Level => EntityLevel;
    public virtual Entity SummoningEntity => this;
    public string Faction => string.IsNullOrEmpty(FactionOverride) ? EntityFaction : FactionOverride;
    public TargetingSystem TargetingSystem => EntityTargetingSystem;
    public bool Alive => EntityState != eEntityState.Dead;

    public float BaseAttribute(string attribute)
    {
        if (BaseAttributes.ContainsKey(attribute))
        {
            return BaseAttributes[attribute];
        }
        else
        {
            Debug.LogError($"Entity {EntityID} doesn't have a [{attribute}] attribute.");
            return 0.0f;
        }
    }

    public float Attribute(string attribute, string skillID, string actionID, string statusID, List<string> categories)
    {
        if (BaseAttributes.ContainsKey(attribute))
        {
            var value = BaseAttributes[attribute];
            if (AttributeChanges.ContainsKey(attribute))
            {
                foreach (var entry in AttributeChanges[attribute])
                {
                    var attributeChange = entry.Value;
                    var requirement = attributeChange.Requirement;

                    switch (attributeChange.PayloadFilter)
                    {
                        case Effect.ePayloadFilter.All:
                        {
                            break;
                        }
                        case Effect.ePayloadFilter.Action:
                        {
                            if (!string.IsNullOrEmpty(actionID) && requirement == actionID)
                            {
                                break;
                            }
                            continue;
                        }
                        case Effect.ePayloadFilter.Category:
                        {
                            if (categories != null && categories.Contains(requirement))
                            {
                                break;
                            }
                            continue;
                        }
                        case Effect.ePayloadFilter.Skill:
                        {
                            if (!string.IsNullOrEmpty(skillID) && requirement == skillID)
                            {
                                break;
                            }
                            continue;
                        }
                        case Effect.ePayloadFilter.SkillGroup:
                        {
                            if (!string.IsNullOrEmpty(skillID) && GameData.SkillGroups.ContainsKey(requirement))
                            {
                                break;
                            }
                            continue;
                        }
                        case Effect.ePayloadFilter.Status:
                        {
                            if (!string.IsNullOrEmpty(statusID) && requirement == statusID)
                            {
                                break;
                            }
                            continue;
                        }
                        case Effect.ePayloadFilter.StatusGroup:
                        {
                            if (!string.IsNullOrEmpty(statusID) && GameData.StatusEffectGroups.ContainsKey(requirement))
                            {
                                break;
                            }
                            continue;
                        }
                        default:
                        {
                            Debug.LogError($"Unimplemented payload filter: {attributeChange.PayloadFilter}");
                            break;
                        }
                    }
                    value += attributeChange.Value.IncomingValue(this);
                }
            }

            return value;
        }
        else
        {
            Debug.LogError($"Entity {EntityID} doesn't have a [{attribute}] attribute.");
            return 0.0f;
        }
    }

    public Dictionary<string, float> EntityAttributes(string skillID, string actionID, string statusID, List<string> categories)
    {
        var attributes = new Dictionary<string, float>();

        foreach (var attribute in BaseAttributes)
        {
            attributes.Add(attribute.Key, Attribute(attribute.Key, skillID, actionID, statusID, categories));
        }

        return attributes;
    }

    public Vector3 Origin
    {
        get
        {
            var position = transform.position;
            position.y += EntityData.OriginHeight;

            return position;
        }
    }

    public FactionData FactionData
    {
        get
        {
            return GameData.GetFactionData(Faction);
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
        protected set
        {
            TargetingSystem.SelectTarget(value);
        }
    }

    public float ResourceRatio(string resourceName)
    {
        if (ResourcesCurrent.ContainsKey(resourceName) && ResourcesMax.ContainsKey(resourceName))
        {
            return ResourcesCurrent[resourceName] / ResourcesMax[resourceName];
        }
        else
        {
            Debug.LogError($"Resource name {resourceName} is invalid.");
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

    protected virtual bool CanUseSkillInCurrentState(SkillData skillData)
    {
        if (EntityState == eEntityState.Dead)
        {
            return false;
        }

        switch (skillData.CasterState)
        {
            case SkillData.eCasterState.Grounded:
            {
                return Movement.IsGrounded;
            }
            case SkillData.eCasterState.Jumping:
            {
                return !Movement.IsGrounded;
            }
            default:
            {
                return true;
            }
        }
    }

    protected virtual bool CanAffordCost(List<ActionCostCollection> costActions)
    {
        var costs = new Dictionary<string, float>();
        foreach (var cost in costActions)
        {
            if (costs.ContainsKey(cost.ResourceName))
            {
                costs[cost.ResourceName] += cost.GetValue(this);
            }
            else
            {
                costs.Add(cost.ResourceName, cost.GetValue(this));
            }
        }
        foreach (var cost in costs)
        {
            if (cost.Value > ResourcesCurrent[cost.Key])
            {
                return false;
            }
        }
        return true;
    }

    public virtual bool CanUseSkill(SkillData skillData)
    {
        if (IsSkillOnCooldown(skillData.SkillID))
        {
            return false;
        }

        if (!CanUseSkillInCurrentState(skillData))

        if (!CanAffordCost(GameData.GetSkillData(skillData.SkillID).SkillCost))
        {
            return false;
        }

        return true;
    }
    #endregion
}
