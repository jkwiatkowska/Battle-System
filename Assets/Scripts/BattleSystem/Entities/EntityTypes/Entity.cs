using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum eEntityState
    {
        Inactive,
        Alive,
        Dead
    }

    [SerializeField] public string EntityID;
    [SerializeField] int EntityLevel = 1;
    public EntityData EntityData => BattleData.GetEntityData(EntityID);

    public string EntityUID                                                 { get; private set; }
    static int EntityCount = 0;

    // Attributes
    public Dictionary<string, float> BaseAttributes                         { get; protected set; }
    public Dictionary<string, float> ResourcesCurrent                       { get; protected set; }
    public Dictionary<string, float> ResourcesMax                           { get; protected set; }

    // Status effects
    public Dictionary<string, StatusEffect> StatusEffects                   { get; protected set; } // Key: Status ID.
    protected Dictionary<string, Dictionary<string, AttributeChange>> AttributeChanges;             // Key: Attribute affected, key 2: generated key.
    protected Dictionary<Effect.ePayloadFilter, Dictionary<string, Immunity>> Immunities;           // Key: Payload filter, key2: generated key.
    protected Dictionary<EffectLock.eLockType, Dictionary<string, EffectLock>> Locks;               // Key: Lock type, key 2: generated key.
    protected Dictionary<Effect.ePayloadFilter, Dictionary<string, EffectResistance>> Resistances;  // Not implemented.
    protected Dictionary<string, Dictionary<string, ResourceGuard>> ResourceGuards;                 // Key: Guarded resource, key2: generated key.
    protected Dictionary<string, Dictionary<string, Shield>> Shields;                               // Key: Attribute protected, key 2: generated key.
    protected Dictionary<TriggerData.eTrigger, Dictionary<string, Trigger>> EffectTriggers;         // Key: Trigger type, Key2: trigger ID.

    // Triggers
    public Dictionary<TriggerData.eTrigger, List<Trigger>> Triggers         { get; protected set; }

    // Other objects that make up an entity
    public EntityCanvas EntityCanvas                                        { get; protected set; }
    public TargetingSystem EntityTargetingSystem                            { get; protected set; }
    public Targetable Targetable                                            { get; protected set; }
    public MovementEntity Movement                                          { get; protected set; }
    public EntityBattle EntityBattle                                        { get; protected set; }

    // State
    public eEntityState EntityState                                         { get; protected set; }
    public bool IsTargetable                                                { get; protected set; }
    public virtual string Faction => EntityData.Faction;
    protected string FactionOverride;
    protected bool SetupComplete;

    // Connections with other entities
    public Dictionary<string, Dictionary<string, float>> TaggedEntities     { get; protected set; }
    [Serializable] struct EntityToTag { public string tag; public Entity entity; };
    [SerializeField] List<EntityToTag> EntitiesToTag;
    protected Dictionary<string, List<string>> TagsAppliedBy;

    public Dictionary<string, List<EntitySummon>> SummonedEntities          { get; protected set; }

    #region Editor
    #if UNITY_EDITOR
    public void UpdateID(string entityID)
    {
        EntityID = entityID;
    }

    public void UpdateLevel(int entityLevel)
    {
        EntityLevel = entityLevel;
    }

    public void RefreshEntity()
    {
        Setup(EntityID, EntityLevel);
    }

    #endif
    #endregion

    public virtual void Setup(string entityID, int entityLevel, Entity source = null)
    {
        EntityID = entityID;
        EntityUID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;
        EntityState = eEntityState.Alive;
        name = EntityUID;
        var entityData = EntityData;

        // Attributes
        BaseAttributes = new Dictionary<string, float>();
        foreach (var attribute in entityData.BaseAttributes)
        {
            BaseAttributes[attribute.Key] = Formulae.EntityBaseAttribute(this, attribute.Key);
        }

        // Resources
        SetupResourcesMax(setup: true);
        SetupResourcesStart();

        // Skills
        EntityBattle = new EntityBattle(this);

        // Triggers
        Triggers = new Dictionary<TriggerData.eTrigger, List<Trigger>>();
        foreach (var trigger in entityData.Triggers)
        {
            AddTrigger(new Trigger(trigger));
        }
        IsTargetable = entityData.IsTargetable;

        // Status effects
        StatusEffects = new Dictionary<string, StatusEffect>();
        AttributeChanges = new Dictionary<string, Dictionary<string, AttributeChange>>();
        Immunities = new Dictionary<Effect.ePayloadFilter, Dictionary<string, Immunity>>();
        Locks = new Dictionary<EffectLock.eLockType, Dictionary<string, EffectLock>>();
        Resistances = new Dictionary<Effect.ePayloadFilter, Dictionary<string, EffectResistance>>();
        ResourceGuards = new Dictionary<string, Dictionary<string, ResourceGuard>>();
        Shields = new Dictionary<string, Dictionary<string, Shield>>();
        EffectTriggers = new Dictionary<TriggerData.eTrigger, Dictionary<string, Trigger>>();

        foreach (var statusEffect in entityData.StatusEffects)
        {
            ApplyStatusEffect(this, statusEffect.Status, statusEffect.Stacks);
        }

        // Connections
        TaggedEntities = new Dictionary<string, Dictionary<string, float>>();
        foreach (var entity in EntitiesToTag)
        {
            TagEntity(entity.tag, entity.entity);
        }    
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

        StartCoroutine(LateSetupCoroutine());
    }

    protected IEnumerator LateSetupCoroutine()
    {
        yield return null;

        TargetingSystem.UpdateEntityLists();
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
            // Resource recovery
            var attributes = EntityAttributes();
            if (ResourcesCurrent != null)
            {
                foreach (var resource in EntityData.Resources)
                {
                    if (ResourcesCurrent.ContainsKey(resource.Key))
                    {
                        var recovery = Formulae.ResourceRecoveryRate(this, attributes, resource.Value);
                        if (recovery > Constants.Epsilon || recovery < Constants.Epsilon)
                        {
                            recovery *= Time.deltaTime;
                            ApplyChangeToResource(resource.Key, recovery);
                        }
                    }
                }
            }

            // Status effects
            var effectsToRemove = new List<string>();
            foreach (var status in StatusEffects)
            {
                var expired = !status.Value.Update();
                if (expired)
                {
                    effectsToRemove.Add(status.Key);
                }
            }

            foreach (var key in effectsToRemove)
            {
                OnStatusExpired(StatusEffects[key]);
                RemoveStatusEffect(key);
            }

            // Skills
            EntityBattle.Update();
        }
    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void FixedUpdate()
    {
        EntityBattle.FixedUpdate();
    }

    #region Triggers
    public void AddTrigger(Trigger trigger, string key = "")
    {
        if (Triggers != null && !Triggers.ContainsKey(trigger.TriggerData.Trigger))
        {
            Triggers[trigger.TriggerData.Trigger] = new List<Trigger>();
        }

        trigger.TriggerKey = key;
        Triggers[trigger.TriggerData.Trigger].Add(trigger);
    }

    public void RemoveTrigger(TriggerData triggerData, string key)
    {
        if (Triggers != null && !Triggers.ContainsKey(triggerData.Trigger))
        {
            return;
        }

        Triggers[triggerData.Trigger].RemoveAll(t => t.TriggerKey.Equals(key));
    }

    protected virtual void OnTrigger(TriggerData.eTrigger trigger, Entity triggerSource = null, PayloadResult payloadResult = null, 
                                     ActionResult actionResult = null, Action action = null, string statusID = "", 
                                     TriggerData.eEntityAffected entityAffected = TriggerData.eEntityAffected.Self, 
                                     string customIdentifier = "")
    {
        // Triggers can be caused by other entities, so the triggers gets passed along.
        if (entityAffected == TriggerData.eEntityAffected.Self)
        {
            // Summoner
            if (SummonedEntities != null)
            {
                for (int i = 0; i < SummonedEntities.Count; i++)
                {
                    var summon = SummonedEntities.Values.ElementAt(i);

                    for (int j = 0; j < summon.Count; j++)
                    {
                        var entity = summon.ElementAt(j);
                        if (entity != null)
                        {
                            entity.OnTrigger(trigger, this, payloadResult, actionResult, action, statusID, 
                                             entityAffected: TriggerData.eEntityAffected.Summoner, customIdentifier);
                        }
                    }
                }
            }

            // Tagged entity
            if (TagsAppliedBy != null)
            {
                for (int i = 0; i < TagsAppliedBy.Count; i++)
                {
                    var key = TagsAppliedBy.Keys.ElementAt(i);

                    if (BattleSystem.Entities.ContainsKey(key))
                    {
                        var entity = BattleSystem.Entities[key];

                        if (entity != null)
                        {
                            entity.OnTrigger(trigger, this, payloadResult, actionResult, action, statusID, 
                                             entityAffected: TriggerData.eEntityAffected.TaggedEntity, customIdentifier);
                        }
                    }
                }
            }

            // Engaged entity
            if (EntityBattle.InCombat)
            {
                for (int i = 0; i < EntityBattle.EngagedEntities.Count; i++)
                {
                    var entity = EntityBattle.EngagedEntities.Values.ElementAt(i).Entity;
                    if (entity != null)
                    {
                        entity.OnTrigger(trigger, this, payloadResult, actionResult, action, statusID, 
                                                      entityAffected: TriggerData.eEntityAffected.EngagedEntity);
                    }
                }
            }
        }

        // Act on the trigger.
        if (Triggers != null && Triggers.ContainsKey(trigger))
        {
            for (int i = 0; i < Triggers[trigger].Count; i++)
            {
                Triggers[trigger][i].TryExecute(entity: this, out var keep, triggerSource != null ? triggerSource : this, payloadResult, 
                             action, actionResult, statusID, entityAffected);
                if (!keep)
                {
                    Triggers[trigger].RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public virtual void OnCustomTrigger(string identifier, PayloadResult payloadResult = null)
    {
        OnTrigger(TriggerData.eTrigger.Custom, triggerSource: payloadResult?.Caster, customIdentifier: identifier);
    }

    #region Battle Triggers
    public virtual void OnEngage(Entity triggerSource)
    {
        OnTrigger(TriggerData.eTrigger.OnEngage, triggerSource: triggerSource);
    }

    public virtual void OnDisengage(Entity triggerSource)
    {
        OnTrigger(TriggerData.eTrigger.OnDisengage, triggerSource: triggerSource);
    }

    public virtual void OnBattleStart(Entity triggerSource)
    {
        OnTrigger(TriggerData.eTrigger.OnBattleStart, triggerSource: triggerSource);
    }

    public virtual void OnbattleEnd()
    {
        OnTrigger(TriggerData.eTrigger.OnBattleEnd);
    }

    public virtual void OnPayloadApplied(PayloadResult payloadResult)
    {
        if (payloadResult == null)
        {
            return;
        }

        var entity = payloadResult.Target;
        if (entity.IsEnemy(Faction))
        {
            EntityBattle.Engage(entity);
        }

        OnTrigger(TriggerData.eTrigger.OnPayloadApplied, triggerSource: payloadResult.Target, payloadResult: payloadResult);
    }

    public virtual void OnPayloadReceived(PayloadResult payloadResult)
    {
        if (payloadResult == null)
        {
            return;
        }

        var entity = payloadResult.Caster;
        if (entity.IsEnemy(Faction))
        {
            EntityBattle.Engage(entity);
        }
        if (entity.EntityData.EntityType == EntityData.eEntityType.SummonnedEntity ||
            entity.EntityData.EntityType == EntityData.eEntityType.Projectile)
        {
            var summon = entity as EntitySummon;
            if (summon != null && summon.Summoner != null)
            {
                EntityBattle.Engage(summon.Summoner);
            }
        }

        OnTrigger(TriggerData.eTrigger.OnPayloadReceived, triggerSource: payloadResult.Caster, payloadResult: payloadResult);
    }

    public virtual void OnHitMissed(Entity target, PayloadResult payloadResult)
    {
        OnTrigger(TriggerData.eTrigger.OnHitMissed, triggerSource: target, payloadResult: payloadResult);

        HUDPopupText.Instance.DisplayMiss(target);
    }

    public virtual void OnResourceChanged(PayloadResult payloadResult)
    {
        if (payloadResult == null)
        {
            return;
        }

        HUDPopupText.Instance.DisplayDamage(payloadResult);

        OnTrigger(TriggerData.eTrigger.OnResourceChanged, triggerSource: payloadResult.Caster, payloadResult: payloadResult);
    }

    public virtual void OnReviveIncoming(PayloadResult payloadResult)
    {
        if (!Alive)
        {
            // Cancel any action timelines invoked by death trigger
            StopAllCoroutines();

            // Reset states
            EntityState = eEntityState.Alive;
            EntityBattle.SetIdle();

            // Triggers
            OnTrigger(TriggerData.eTrigger.OnReviveIncoming, triggerSource: payloadResult.Caster, payloadResult: payloadResult);
            payloadResult.Caster.OnReviveOutgoing(payloadResult);
        }
    }

    public virtual void OnReviveOutgoing(PayloadResult payloadResult)
    {
        OnTrigger(TriggerData.eTrigger.OnReviveOutgoing, triggerSource: payloadResult.Target, payloadResult: payloadResult);
    }

    public virtual void OnActionUsed(Action action, ActionResult actionResult)
    {
        OnTrigger(TriggerData.eTrigger.OnActionUsed, action: action, actionResult: actionResult);
    }

    #region Status Triggers
    public virtual void OnStatusApplied(Entity target, string statusID)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusApplied, triggerSource: target, statusID: statusID);
    }

    public virtual void OnStatusReceived(StatusEffect statusEffect)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusReceived, triggerSource: statusEffect.Caster, statusID: statusEffect.Data.StatusID);
    }

    public virtual void OnStatusClearedOutgoing(Entity target, string statusID)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusClearedOutgoing, triggerSource: target, statusID: statusID);
    }

    public virtual void OnStatusClearedIncoming(Entity source, string statusName)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusClearedIncoming, triggerSource: source, statusID: statusName);
    }

    public virtual void OnStatusExpired(StatusEffect statusEffect)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusExpired, triggerSource: statusEffect.Caster, statusID: statusEffect.Data.StatusID);
    }

    public virtual void OnStatusEnded(StatusEffect statusEffect)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusEnded, triggerSource: statusEffect.Caster, statusID: statusEffect.Data.StatusID);
    }
    #endregion

    public virtual void OnImmune(Entity source = null, PayloadResult payloadResult = null)
    {
        OnTrigger(TriggerData.eTrigger.OnImmune, source, payloadResult);
        HUDPopupText.Instance.DisplayImmune(this);
    }

    public virtual void OnTargetOutOfRange() { }

    public virtual void OnTargetNotInLineOfSight() { }
    #endregion

    #region Life Triggers
    protected virtual void OnSpawn()
    {
        OnTrigger(TriggerData.eTrigger.OnSpawn);
    }

    public virtual void OnDeath(Entity source = null, PayloadResult payloadResult = null)
    {
        if (!Alive)
        {
            return;
        }

        payloadResult?.Caster.OnKill(payloadResult);

        if (gameObject != null)
        {
            StopAllCoroutines();
        }
        EntityState = eEntityState.Dead;
        EntityBattle.SetIdle();

        OnTrigger(TriggerData.eTrigger.OnDeath, triggerSource: source, payloadResult: payloadResult);

        EntityBattle.DisengageAll();

        RemoveAllTagsOnSelf();
        RemoveAllTagsOnEntities();

        if (Targetable != null)
        {
            Targetable.RemoveTargeting();
        }

        KillLinkedSummons();
    }

    public virtual void OnKill(PayloadResult payloadResult = null, string statusID = "")
    {
        if (payloadResult == null)
        {
            return;
        }

        OnTrigger(TriggerData.eTrigger.OnKill, triggerSource: payloadResult.Target, statusID: statusID);
    }
    #endregion

    #region Physical Triggers
    public void OnTriggerEnter(Collider other)
    {
        var entityHit = other.GetComponentInParent<Entity>();

        if (entityHit != null && entityHit.EntityUID != EntityUID)
        {
            OnCollisionEntity(entityHit);

            if (entityHit.IsTargetable)
            {
                OnCollisionTargetableEntity(entityHit);
            }
        }
        else if (BattleSystem.IsOnTerrainLayer(other.gameObject))
        {
            OnCollisionTerrain(other);
        }
    }

    protected virtual void OnCollisionTargetableEntity(Entity entity)
    {
        OnTrigger(TriggerData.eTrigger.OnCollisionTargetableEntity, triggerSource: entity);
    }

    protected virtual void OnCollisionEntity(Entity entity)
    {
        OnTrigger(TriggerData.eTrigger.OnCollisionEntity, triggerSource: entity);
    }

    protected virtual void OnCollisionTerrain(Collider collider)
    {
        OnTrigger(TriggerData.eTrigger.OnCollisionTerrain);
    }

    public virtual void OnEntityMoved()
    {
        OnTrigger(TriggerData.eTrigger.OnEntityMoved);

        Movement.CancelMovement();
        EntityBattle.OnMoved();
    }

    public virtual void OnEntityJumped()
    {
        OnTrigger(TriggerData.eTrigger.OnEntityJumped);

        Movement.CancelMovement();
        EntityBattle.OnMoved();
    }

    public virtual void OnEntityLanded()
    {
        OnTrigger(TriggerData.eTrigger.OnEntityLanded);
    }

    #endregion
    #endregion

    #region Status Effects
    #region Status
    public void ApplyStatusEffect(Entity sourceEntity, string statusID, int stacks = 1, Action action = null, Payload sourcePayload = null)
    {
        if (!BattleData.StatusEffects.ContainsKey(statusID))
        {
            Debug.LogError($"Invalid status effect ID: {statusID}");
            return;
        }

        var statusEffectData = BattleData.StatusEffects[statusID];

        if (!StatusEffects.ContainsKey(statusEffectData.StatusID))
        {
            if (stacks < 0)
            {
                return;
            }

            StatusEffects[statusID] = new StatusEffect(target: this, sourceEntity, statusEffectData, action, sourcePayload);
        }
        else if (sourceEntity.EntityUID != StatusEffects[statusEffectData.StatusID].Caster.EntityUID)
        {
            StatusEffects[statusID].Setup(target: this, sourceEntity, statusEffectData, action, sourcePayload);
        }

        StatusEffects[statusEffectData.StatusID].ApplyStacks(stacks);

        OnStatusReceived(StatusEffects[statusEffectData.StatusID]);
        sourceEntity.OnStatusApplied(this, statusID);
    }

    public int GetStatusEffectStacks(string statusEffect)
    {
        if (StatusEffects.ContainsKey(statusEffect))
        {
            return StatusEffects[statusEffect].CurrentStacks;
        }

        return 0;
    }

    public void ClearStatusEffect(Entity source, string statusID)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            StatusEffects[statusID].ClearStatus();
            RemoveStatusEffect(statusID);

            OnStatusClearedIncoming(source, statusID);
            source.OnStatusClearedOutgoing(this, statusID);
        }
    }

    public void RemoveStatusEffectStacks(Entity source, string statusID, int stacks)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            StatusEffects[statusID].RemoveStacks(stacks);
            if (StatusEffects[statusID].CurrentStacks <= 0)
            {
                ClearStatusEffect(source, statusID);
            }
        }
    }

    public void DelayedStatusEffectRemoval(string statusID)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            StatusEffects[statusID].RemoveEffect = true;
        }
    }

    public void RemoveStatusEffect(string statusID)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            OnStatusEnded(StatusEffects[statusID]);
            StatusEffects[statusID].RemoveStatus();
            StatusEffects.Remove(statusID);
        }
    }
    #endregion

    #region Attribute Change
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
        if (!AttributeChanges.ContainsKey(attribute) || !AttributeChanges[attribute].ContainsKey(key))
        {
            return;
        }

        AttributeChanges[attribute].Remove(key);

        SetupResourcesMax();
    }
    #endregion

    #region Conversion
    public void Convert(string faction)
    {
        FactionOverride = faction;
    }

    public void RemoveConversion()
    {
        FactionOverride = "";
    }
    #endregion

    #region Immunity
    public void ApplyImmunity(Immunity immunity, string key)
    {
        if (!Immunities.ContainsKey(immunity.Data.PayloadFilter))
        {
            Immunities[immunity.Data.PayloadFilter] = new Dictionary<string, Immunity>();
        }
        Immunities[immunity.Data.PayloadFilter][key] = immunity;
    }

    public void RemoveImmunity(Effect.ePayloadFilter payloadFilter, string key)
    {
        if (!Immunities.ContainsKey(payloadFilter) || !Immunities[payloadFilter].ContainsKey(key))
        {
            return;
        }

        Immunities[payloadFilter].Remove(key);
    }

    public EffectImmunity HasImmunityAgainstAction(ActionPayload action)
    {
        Immunity immunity = null;

        if (Immunities.ContainsKey(Effect.ePayloadFilter.All) && Immunities[Effect.ePayloadFilter.All].Count > 0)
        {
            immunity = Immunities[Effect.ePayloadFilter.All].ElementAt(0).Value;
        }

        if (Immunities.ContainsKey(Effect.ePayloadFilter.Skill) && Immunities[Effect.ePayloadFilter.Skill].Count > 0)
        {
            foreach (var i in Immunities[Effect.ePayloadFilter.Skill])
            {
                if (i.Value.Data.PayloadName == action.SkillID)
                {
                    immunity = i.Value;
                    break;
                }
            }
        }

        if (Immunities.ContainsKey(Effect.ePayloadFilter.SkillGroup) && Immunities[Effect.ePayloadFilter.SkillGroup].Count > 0)
        {
            foreach (var i in Immunities[Effect.ePayloadFilter.SkillGroup])
            {
                if (BattleData.SkillGroups.ContainsKey(i.Value.Data.PayloadName) && BattleData.SkillGroups[i.Value.Data.PayloadName].Contains(action.SkillID))
                {
                    immunity = i.Value;
                    break;
                }
            }
        }

        if (Immunities.ContainsKey(Effect.ePayloadFilter.Action) && Immunities[Effect.ePayloadFilter.Action].Count > 0)
        {
            foreach (var i in Immunities[Effect.ePayloadFilter.Action])
            {
                if (i.Value.Data.PayloadName == action.ActionID)
                {
                    immunity = i.Value;
                    break;
                }
            }
        }

        if (immunity != null)
        {
            immunity.Use(this);
            return immunity.Data;
        }

        return null;
    }

    public EffectImmunity HasImmunityAgainstStatus(string statusID)
    {
        Immunity immunity = null;

        if (Immunities.ContainsKey(Effect.ePayloadFilter.All) && Immunities[Effect.ePayloadFilter.All].Count > 0)
        {
            immunity = Immunities[Effect.ePayloadFilter.All].ElementAt(0).Value;
        }
        else if (Immunities.ContainsKey(Effect.ePayloadFilter.Status) && Immunities[Effect.ePayloadFilter.Status].Count > 0)
        {
            foreach (var i in Immunities[Effect.ePayloadFilter.Status])
            {
                if (i.Value.Data.PayloadName == statusID)
                {
                    immunity = i.Value;
                    break;
                }
            }
        }
        else if (Immunities.ContainsKey(Effect.ePayloadFilter.StatusGroup) && Immunities[Effect.ePayloadFilter.StatusGroup].Count > 0)
        {
            foreach (var i in Immunities[Effect.ePayloadFilter.StatusGroup])
            {
                if (BattleData.StatusEffectGroups.ContainsKey(i.Value.Data.PayloadName) && BattleData.StatusEffectGroups[i.Value.Data.PayloadName].Contains(statusID))
                {
                    immunity = i.Value;
                    break;
                }
            }
        }

        if (immunity != null)
        {
            immunity.Use(this);
            return immunity.Data;
        }

        return null;
    }

    public EffectImmunity HasImmunityAgainstCategory(string category)
    {
        Immunity immunity = null;

        if (Immunities.ContainsKey(Effect.ePayloadFilter.All) && Immunities[Effect.ePayloadFilter.All].Count > 0)
        {
            immunity = Immunities[Effect.ePayloadFilter.All].ElementAt(0).Value;
        }
        else if (Immunities.ContainsKey(Effect.ePayloadFilter.Category) && Immunities[Effect.ePayloadFilter.Category].Count > 0)
        {
            foreach (var i in Immunities[Effect.ePayloadFilter.Category])
            {
                if (i.Value.Data.PayloadName == category)
                {
                    immunity = i.Value;
                }
            }
        }

        if (immunity != null)
        {
            immunity.Use(this);
            return immunity.Data;
        }

        return null;
    }
    #endregion

    #region Locks

    public void ApplyLock(EffectLock lockData, string key)
    {
        if (!Locks.ContainsKey(lockData.LockType))
        {
            Locks[lockData.LockType] = new Dictionary<string, EffectLock>();
        }

        Locks[lockData.LockType].Add(key, lockData);

        if (EntityBattle.CurrentSkill.Interruptible && IsSkillLocked(EntityBattle.CurrentSkill.SkillID))
        {
            EntityBattle.CancelSkill();
        }
    }

    public void RemoveLock(EffectLock lockData, string key)
    {
        if (Locks.ContainsKey(lockData.LockType) && Locks[lockData.LockType].ContainsKey(key))
        {
            Locks[lockData.LockType].Remove(key);
        }
    }

    public bool IsSkillLocked(string skillID)
    {
        if (Locks.ContainsKey(EffectLock.eLockType.SkillsAll) && Locks[EffectLock.eLockType.SkillsAll].Count > 0)
        {
            return true;
        }

        if (Locks.ContainsKey(EffectLock.eLockType.Skill) && Locks[EffectLock.eLockType.Skill].Count > 0)
        {
            foreach (var l in Locks[EffectLock.eLockType.Skill])
            {
                if (l.Value.Skill == skillID)
                {
                    return true;
                }
            }
        }

        if (Locks.ContainsKey(EffectLock.eLockType.SkillsGroup) && Locks[EffectLock.eLockType.SkillsGroup].Count > 0)
        {
            foreach (var group in Locks[EffectLock.eLockType.SkillsGroup])
            {
                if (BattleData.SkillGroups.ContainsKey(group.Value.Skill) && BattleData.SkillGroups[group.Value.Skill].Contains(skillID))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsMovementLocked => (Movement.CurrentMovement != null && Movement.CurrentMovement.LockEntityMovement) || 
                                    (Locks.ContainsKey(EffectLock.eLockType.Movement) && Locks[EffectLock.eLockType.Movement].Count > 0);

    public bool IsJumpingLocked => (Movement.CurrentMovement != null && Movement.CurrentMovement.LockEntityMovement) || 
                                   (Locks.ContainsKey(EffectLock.eLockType.Jump) && Locks[EffectLock.eLockType.Jump].Count > 0);

    #endregion

    #region Resistance
    public void ApplyResistance(EffectResistance resistance, string key)
    {
        if (!Immunities.ContainsKey(resistance.PayloadFilter))
        {
            Resistances[resistance.PayloadFilter] = new Dictionary<string, EffectResistance>();
        }
        Resistances[resistance.PayloadFilter][key] = resistance;
    }

    public void RemoveResistance(Effect.ePayloadFilter payloadFilter, string key)
    {
        if (!Resistances.ContainsKey(payloadFilter) || !Resistances[payloadFilter].ContainsKey(key))
        {
            return;
        }

        Resistances[payloadFilter].Remove(key);
    }
    #endregion

    #region ResourceGuard
    public void ApplyResourceGuard(ResourceGuard resourceGuard, string key)
    {
        if (!ResourceGuards.ContainsKey(resourceGuard.Data.Resource))
        {
            ResourceGuards[resourceGuard.Data.Resource] = new Dictionary<string, ResourceGuard>();
        }
        ResourceGuards[resourceGuard.Data.Resource][key] = resourceGuard;
    }

    public void RemoveResourceGuard(string resource, string key)
    {
        if (!ResourceGuards.ContainsKey(resource) || !ResourceGuards[resource].ContainsKey(key))
        {
            return;
        }

        ResourceGuards[resource].Remove(key);
    }

    #endregion

    #region Shield
    public void ApplyShield(Shield shield, string key, float resourceGranted)
    {
        if (!Shields.ContainsKey(shield.Data.ShieldedResource))
        {
            Shields.Add(shield.Data.ShieldedResource, new Dictionary<string, Shield>());
        }

        Shields[shield.Data.ShieldedResource][key] = shield;

        if (!ResourcesMax.ContainsKey(shield.Data.ShieldResource) || shield.Data.SetMaxShieldResource)
        {
            ResourcesMax[shield.Data.ShieldResource] = resourceGranted;
        }

        if (!ResourcesCurrent.ContainsKey(shield.Data.ShieldResource))
        {
            ResourcesCurrent.Add(shield.Data.ShieldResource, resourceGranted);
        }
        else
        {
            ApplyChangeToResource(shield.Data.ShieldResource, resourceGranted);
        }

        EntityCanvas.SetupResourceDisplay(shield.Data.ShieldResource);
    }

    public void RemoveShield(EffectShield shieldData, string key)
    {
        if (!Shields.ContainsKey(shieldData.ShieldedResource) || !Shields[shieldData.ShieldedResource].ContainsKey(key))
        {
            return;
        }

        if (shieldData.RemoveShieldResourceOnEffectEnd)
        {
            ResourcesCurrent[shieldData.ShieldResource] = 0.0f;
            EntityCanvas.UpdateResourceDisplay(shieldData.ShieldResource);
        }
        Shields[shieldData.ShieldedResource].Remove(key);
    }
    #endregion

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

    public virtual void TagEntity(string tag, Entity entity, TagData tagData = null)
    {
        if (!TaggedEntities.ContainsKey(tag))
        {
            TaggedEntities.Add(tag, new Dictionary<string, float>());
        }

        if (TaggedEntities[tag].ContainsKey(entity.EntityUID))
        {
            TaggedEntities[tag][entity.EntityUID] = BattleSystem.Time + (tagData != null ? tagData.TagDuration : 0);
        }
        else
        {
            TaggedEntities[tag].Add(entity.EntityUID, BattleSystem.Time + (tagData != null ? tagData.TagDuration : 0));
            entity.ApplyTag(tag, EntityUID);

            if (tagData != null && TaggedEntities[tag].Count > tagData.TagLimit)
            {
                var entityToUntag = BattleSystem.Entities[TaggedEntities[tag].Aggregate((l, r) => l.Value < r.Value ? l : r).Key];
                RemoveTagOnEntity(tag, entityToUntag, false);
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

    public bool HasTag(string tag)
    {
        if (TagsAppliedBy == null)
        {
            return false;
        }

        foreach (var source in TagsAppliedBy)
        {
            foreach (var t in source.Value)
            {
                if (tag == t)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool HasTagFromEntity(string tag, string entityUID)
    {
        if (TagsAppliedBy == null || !TagsAppliedBy.ContainsKey(entityUID))
        {
            return false;
        }

        return TagsAppliedBy[entityUID].Contains(tag);
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
            entityToRemove.OnDeath();
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
            entity.OnDeath(source: this);
        }
    }
    #endregion

    #region Resources
    protected void SetupResourcesMax(bool setup = false)
    {
        if (setup)
        {
            ResourcesMax = new Dictionary<string, float>();
        }

        var attributes = EntityAttributes();

        foreach (var resource in EntityData.Resources)
        {
            ResourcesMax[resource.Key] = Formulae.ResourceMaxValue(this, attributes, resource.Key);
        }
    }

    protected void SetupResourcesStart()
    {
        ResourcesCurrent = new Dictionary<string, float>();
        var attributes = EntityAttributes();

        foreach (var resource in EntityData.Resources)
        {
            ResourcesCurrent.Add(resource.Key, Formulae.ResourceMaxValue(this, attributes, resource.Key));
        }
    }

    // Payload resource change.
    public void ApplyChangeToResource(string resource, PayloadResult payloadResult, bool setTriggers = true)
    {
        var change = payloadResult.Change;
        var resourceAffected = resource;

        // Replace the resource and update change if it's being shielded
        Shield shield = null;
        if (change < 0.0f && !payloadResult.PayloadData.IgnoreShield && Shields.ContainsKey(resource) && Shields[resource].Count > 0)
        {
            int best = -1;

            foreach (var s in Shields[resource])
            {
                if (best == -1 || best < s.Value.Data.Priority)
                {
                    best = s.Value.Data.Priority;
                    shield = s.Value;
                }
            }

            var multiplier = 1.0f;
            var multipliers = new List<float>();
            foreach (var m in shield.Data.CategoryMultipliers)
            {
                if (payloadResult.PayloadData.Categories.Contains(m.Key))
                {
                    multipliers.Add(m.Value);
                }
            }
            if (multipliers.Count > 0)
            {
                foreach (var m in multipliers)
                {
                    multiplier *= m;
                }
            }
            else
            {
                multiplier = shield.Data.DamageMultiplier;
            }

            change *= multiplier;

            resourceAffected = shield.Data.ShieldResource;
        }

        // Remove shield if there is one and it ran out.
        if (shield != null)
        {
            if (change >= ResourcesCurrent[resourceAffected])
            {
                shield.RemoveShield(this);
            }
            else
            {
                shield.UseShield(this, -change);
            }
        }

        // Apply change
        var previous = ResourcesCurrent[resourceAffected];
        ResourcesCurrent[resourceAffected] = Mathf.Clamp(ResourcesCurrent[resourceAffected] + change, 0.0f, ResourcesMax[resourceAffected]);

        // Resource guards
        if (ResourceGuards.ContainsKey(resourceAffected) && ResourceGuards[resourceAffected].Count > 0)
        {
            var resourceNow = ResourcesCurrent[resourceAffected];
            ResourceGuard min = null;
            ResourceGuard max = null;

            var attributes = EntityAttributes(payloadResult.SkillID, payloadResult.ActionID, statusID: "", payloadResult.PayloadData.Categories, 
                             payloadResult.PayloadData.TargetAttributesIgnored);

            // Go through all guards. Use the most extreme ones applied.
            foreach (var guard in ResourceGuards[resourceAffected])
            {
                if (guard.Value.Guard(this, attributes, resourceNow, out var changedResource))
                {
                    if (changedResource > resourceNow)
                    {
                        min = guard.Value;
                    }
                    else if (changedResource < resourceNow)
                    {
                        max = guard.Value;
                    }

                    resourceNow = changedResource;
                }
            }

            if (min != null)
            {
                min.Use(this);
            }
            if (max != null)
            {
                max.Use(this);
            }

            ResourcesCurrent[resourceAffected] = resourceNow;
        }

        // Update payload result.
        payloadResult.Change = previous - ResourcesCurrent[resourceAffected];
        payloadResult.ResourceChanged = resourceAffected;

        // Update display and set any related triggers.
        if (previous != ResourcesCurrent[resourceAffected])
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.UpdateResourceDisplay(resourceAffected);
            }

            if (setTriggers)
            {
                if (payloadResult.Change < -Constants.Epsilon || payloadResult.Change > Constants.Epsilon)
                {
                    OnResourceChanged(payloadResult);
                }
            }

            if (ResourcesCurrent[resourceAffected] <= 0.0f && EntityData.LifeResources.Contains(resourceAffected))
            {
                OnDeath(payloadResult.Caster, payloadResult);
            }
        }
        else
        {
            payloadResult.Change = 0.0f;
        }
    }

    // Passive and cost resource change.
    public void ApplyChangeToResource(string resource, float change)
    {
        // Apply change.
        var previous = ResourcesCurrent[resource];
        ResourcesCurrent[resource] = Mathf.Clamp(ResourcesCurrent[resource] + change, 0.0f, ResourcesMax[resource]);

        // Resource guards
        if (ResourceGuards.ContainsKey(resource) && ResourceGuards[resource].Count > 0)
        {
            var resourceNow = ResourcesCurrent[resource];

            foreach (var guard in ResourceGuards[resource])
            {
                if (guard.Value.Guard(this, EntityAttributes(), resourceNow, out var changedResource))
                {
                    resourceNow = changedResource;
                }
            }

            ResourcesCurrent[resource] = resourceNow;
        }

        // Canvas and death trigger
        if (previous != ResourcesCurrent[resource])
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.UpdateResourceDisplay(resource);
            }

            if (ResourcesCurrent[resource] <= 0.0f && EntityData.LifeResources.Contains(resource))
            {
                OnDeath();
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
    public string CurrentFaction => string.IsNullOrEmpty(FactionOverride) ? Faction : FactionOverride;
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
            return 0.0f;
        }
    }

    public float Attribute(string attribute, string skillID = "", string actionID = "", string statusID = "",
                           List<string> categories = null, float defaultValue = 0.0f)
    {
        if (BaseAttributes.ContainsKey(attribute))
        {
            var value = BaseAttributes[attribute];
            if (AttributeChanges != null && AttributeChanges.ContainsKey(attribute))
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
                            if (!string.IsNullOrEmpty(skillID) && BattleData.SkillGroups.ContainsKey(requirement))
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
                            if (!string.IsNullOrEmpty(statusID) && BattleData.StatusEffectGroups.ContainsKey(requirement))
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
                    value += attributeChange.Value.IncomingValue(target: this, targetAttributes: null, attributeChange.MaxValue);
                }
            }

            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public Dictionary<string, float> EntityAttributes(string skillID = "", string actionID = "", 
                      string statusID = "", List<string> categories = null, List<string> ignoreAttributes = null)
    {
        var attributes = new Dictionary<string, float>();

        foreach (var attribute in BaseAttributes)
        {
            if (ignoreAttributes != null && ignoreAttributes.Contains(attribute.Key))
            {
                attributes.Add(attribute.Key, 0.0f);
            }
            else
            {
                attributes.Add(attribute.Key, Attribute(attribute.Key, skillID, actionID, statusID, categories));
            }
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
            return BattleData.GetFactionData(Faction);
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

    public virtual bool IsEnemy(string targetFaction)
    {
        return (BattleSystem.IsEnemy(CurrentFaction, targetFaction) || BattleSystem.IsEnemy(targetFaction, CurrentFaction));
    }

    public virtual bool IsFriendly(string targetFaction)
    {
        return (BattleSystem.IsFriendly(CurrentFaction, targetFaction));
    }
    #endregion
}
