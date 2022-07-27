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

    public string UID                                                 { get; private set; }
    static int EntityCount = 0;

    // Attributes
    public Dictionary<string, float> BaseAttributes                         { get; protected set; }
    public Dictionary<string, float> ResourcesCurrent                       { get; protected set; }
    public Dictionary<string, float> ResourcesMax                           { get; protected set; }

    // Status effects
    public Dictionary<string, Dictionary<string, StatusEffect>> StatusEffects   { get; protected set; } // Key: Status ID, key2: caster UID
    protected Dictionary<string, Dictionary<string, AttributeChange>> AttributeChanges;                 // Key: Attribute affected, key 2: generated key.
    protected Dictionary<EffectData.ePayloadFilter, Dictionary<string, Immunity>> Immunities;           // Key: Payload filter, key2: generated key.
    protected Dictionary<EffectLock.eLockType, Dictionary<string, EffectLock>> Locks;                   // Key: Lock type, key 2: generated key.
    protected Dictionary<EffectData.ePayloadFilter, Dictionary<string, EffectResistance>> Resistances;  // Not implemented.
    protected Dictionary<string, Dictionary<string, ResourceGuard>> ResourceGuards;                     // Key: Guarded resource, key2: generated key.
    protected Dictionary<string, Dictionary<string, Shield>> Shields;                                   // Key: Attribute protected, key 2: generated key.
    protected Dictionary<TriggerData.eTrigger, Dictionary<string, Trigger>> EffectTriggers;             // Key: Trigger type, Key2: trigger ID.

    // Saved values
    public class SavedValue
    {
        string Key;
        float Value;
        int UsesLeft;

        public SavedValue(string key, float value, int uses)
        {
            Key = key;
            Value = value;
            UsesLeft = uses;
        }

        public float PeekValue()
        {
            return Value;
        }

        public float GetValue(Entity entity)
        {
            if (UsesLeft > 0)
            {
                UsesLeft--;
                if (UsesLeft < 1)
                {
                    entity.RemoveSavedValue(Key);
                }
            }

            return Value;
        }
    }

    protected Dictionary<string, SavedValue> SavedValues;

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

    public EntityInfo EntityInfo                                            { get; protected set; }

    public Vector3 Position                                                 { get; private set; }

    // Connections with other entities
    public Dictionary<string, Dictionary<string, float>> TaggedEntities     { get; protected set; }
    [Serializable] struct EntityToTag { public string tag; public Entity entity; };
    [SerializeField] List<EntityToTag> EntitiesToTag;
    protected Dictionary<string, List<string>> TagsAppliedBy;

    public Dictionary<string, List<EntitySummon>> SummonedEntities          { get; protected set; }

    // Saved transforms (can be used together with area and movement skills)
    [Serializable]
    public class TransformInfo
    {
        public string ID;
        public Transform Transform;
    }

    public class SavedTransformInfo
    {
        public Vector3 Position;
        public Vector3 Forward;

        public SavedTransformInfo(Transform transform)
        {
            Position = transform.position;
            Forward = transform.forward;
        }

        public SavedTransformInfo(Vector3 position, Vector3 forward)
        {
            Position = position;
            Forward = forward;
        }
    }

    [SerializeField] List<TransformInfo> SavedTransformsList;               // Set in editor
    protected Dictionary<string, SavedTransformInfo> SavedTransforms;       // Set and updated in scripts.

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
        UID = entityID + EntityCount.ToString();
        EntityCount++;
        EntityLevel = entityLevel;
        EntityState = eEntityState.Alive;
        name = UID;
        var entityData = EntityData;

        // Attributes
        BaseAttributes = new Dictionary<string, float>();
        foreach (var attribute in entityData.BaseAttributes)
        {
            BaseAttributes[attribute.Key] = Formulae.EntityBaseAttribute(this, attribute.Key);
        }

        EntityInfo = new EntityInfo(this, BaseAttributes);

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
        StatusEffects = new Dictionary<string, Dictionary<string, StatusEffect>>();
        AttributeChanges = new Dictionary<string, Dictionary<string, AttributeChange>>();
        Immunities = new Dictionary<EffectData.ePayloadFilter, Dictionary<string, Immunity>>();
        Locks = new Dictionary<EffectLock.eLockType, Dictionary<string, EffectLock>>();
        Resistances = new Dictionary<EffectData.ePayloadFilter, Dictionary<string, EffectResistance>>();
        ResourceGuards = new Dictionary<string, Dictionary<string, ResourceGuard>>();
        Shields = new Dictionary<string, Dictionary<string, Shield>>();
        EffectTriggers = new Dictionary<TriggerData.eTrigger, Dictionary<string, Trigger>>();

        foreach (var statusEffect in entityData.StatusEffects)
        {
            ApplyStatusEffect(this, statusEffect.Status, statusEffect.Stacks, refreshDuration: false, refreshPayloads: false, isNew: false);
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
            Debug.LogError($"TargetingSystem could not be found for {UID}");
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

        // Saved values
        SavedValues = new Dictionary<string, SavedValue>();

        // Saved transforms
        SavedTransforms = new Dictionary<string, SavedTransformInfo>();
        if (SavedTransformsList != null)
        {
            foreach (var transform in SavedTransformsList)
            {
                if (transform?.Transform == null)
                {
                    Debug.LogError($"{UID}'s saved transform [{transform.ID}] is null.");
                }

                SaveTransform(transform.ID, transform.Transform.position, transform.Transform.forward);
            }
        }

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
            if (ResourcesCurrent != null)
            {
                foreach (var resource in EntityData.Resources)
                {
                    if (ResourcesCurrent.ContainsKey(resource.Key))
                    {
                        var recovery = Formulae.ResourceRecoveryRate(EntityInfo, resource.Value);
                        if (recovery > Constants.Epsilon || recovery < Constants.Epsilon)
                        {
                            recovery *= Time.deltaTime;
                            UpdateResource(resource.Key, recovery);
                        }
                    }
                }
            }

            // Status effects
            var effectsToRemove = new List<(string StatusEffect, string EntityUID)>();
            foreach (var statusEffect in StatusEffects)
            {
                foreach (var instance in statusEffect.Value)
                {
                    var expired = !instance.Value.Update();
                    if (expired)
                    {
                        effectsToRemove.Add((StatusEffect: statusEffect.Key, EntityUID: instance.Key));
                    }
                }
            }

            foreach (var keySet in effectsToRemove)
            {
                OnStatusExpired(StatusEffects[keySet.StatusEffect][keySet.EntityUID]);
                RemoveStatusEffect(keySet.StatusEffect, keySet.EntityUID);
            }

            // Skills
            EntityBattle.Update();

            // Triggers
            OnTrigger(TriggerData.eTrigger.EveryFrame, triggerSource: this);
        }
    }

    protected virtual void LateUpdate()
    {

    }

    protected virtual void FixedUpdate()
    {
        Position = transform.position;
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

    protected virtual void OnTrigger(TriggerData.eTrigger trigger, Entity triggerSource = null, Payload payload = null, 
                                     PayloadComponentResult payloadResult = null, ActionResult actionResult = null, Action action = null,
                                     Dictionary<string, ActionResult> actionResults = null, string statusID = "", 
                                     TriggerData.eEntityAffected entityAffected = TriggerData.eEntityAffected.Self, string customIdentifier = "")
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
                            entity.OnTrigger(trigger, this, payload, payloadResult, actionResult, action, actionResults, statusID, 
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
                            entity.OnTrigger(trigger, this, payload, payloadResult, actionResult, action, actionResults, statusID, 
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
                        entity.OnTrigger(trigger, this, payload, payloadResult, actionResult, action, actionResults, statusID, 
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
                Triggers[trigger][i].TryExecute(entity: this, out var keep, triggerSource != null ? triggerSource : this, 
                                                payload, payloadResult, action, actionResult, actionResults, statusID, entityAffected);
                if (!keep)
                {
                    Triggers[trigger].RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public virtual void OnCustomTrigger(string identifier, Payload payload = null, PayloadComponentResult payloadResult = null)
    {
        OnTrigger(TriggerData.eTrigger.Custom, triggerSource: payload?.Caster?.Entity, payload: payload, payloadResult: payloadResult, customIdentifier: identifier);
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

    public virtual void OnBattleEnd()
    {
        OnTrigger(TriggerData.eTrigger.OnBattleEnd);
    }

    public virtual void OnPayloadComponentApplied(PayloadComponentResult payloadResult)
    {
        if (payloadResult == null)
        {
            return;
        }

        var entity = payloadResult?.Payload?.Target?.Entity;
        if (entity != null && entity.IsEnemy(Faction))
        {
            EntityBattle.Engage(entity);
        }

        OnTrigger(TriggerData.eTrigger.OnPayloadApplied, triggerSource: entity, payload: payloadResult.Payload, payloadResult: payloadResult);
    }

    public virtual void OnPayloadComponentReceived(PayloadComponentResult payloadResult)
    {
        if (payloadResult == null)
        {
            return;
        }

        var entity = payloadResult?.Payload?.Caster?.Entity;
        if (entity != null && entity.IsEnemy(Faction))
        {
            EntityBattle.Engage(entity);
        }

        if (entity?.EntityData.EntityType == EntityData.eEntityType.SummonnedEntity ||
            entity?.EntityData.EntityType == EntityData.eEntityType.Projectile)
        {
            var summon = entity as EntitySummon;
            if (summon != null && summon.Summoner != null)
            {
                EntityBattle.Engage(summon.Summoner);
            }
        }

        OnTrigger(TriggerData.eTrigger.OnPayloadReceived, triggerSource: entity, payload: payloadResult.Payload, payloadResult: payloadResult);
    }

    public virtual void OnHitMissed(Entity target, Payload payload)
    {
        OnTrigger(TriggerData.eTrigger.OnHitMissed, triggerSource: target, payload: payload);

        HUDPopupText.Instance.DisplayMiss(target);
    }

    public virtual void OnResourceChanged(PayloadComponentResult payloadResult)
    {
        if (payloadResult == null)
        {
            return;
        }

        OnTrigger(TriggerData.eTrigger.OnResourceChanged, triggerSource: payloadResult?.Payload?.Caster?.Entity, payload: payloadResult?.Payload, payloadResult: payloadResult);
    }

    public virtual void OnReviveIncoming(PayloadComponentResult payloadResult)
    {
        if (!Alive)
        {
            // Cancel any action timelines invoked by death trigger
            StopAllCoroutines();

            // Reset states
            EntityState = eEntityState.Alive;
            EntityBattle.SetIdle();

            // Triggers
            var source = payloadResult?.Payload?.Caster?.Entity;
            OnTrigger(TriggerData.eTrigger.OnReviveIncoming, triggerSource: source, payload: payloadResult?.Payload, payloadResult: payloadResult);
            source.OnReviveOutgoing(payloadResult);
        }
    }

    public virtual void OnReviveOutgoing(PayloadComponentResult payloadResult)
    {
        OnTrigger(TriggerData.eTrigger.OnReviveOutgoing, triggerSource: payloadResult?.Payload?.Target?.Entity, payload: payloadResult?.Payload, payloadResult: payloadResult);
    }

    public virtual void OnActionUsed(Action action, ActionResult actionResult, Dictionary<string, ActionResult> actionResults)
    {
        OnTrigger(TriggerData.eTrigger.OnActionUsed, action: action, actionResults: actionResults, actionResult: actionResult);
    }

    #region Status Triggers
    public virtual void OnStatusApplied(Entity target, string statusID)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusApplied, triggerSource: target, statusID: statusID);
    }

    public virtual void OnStatusReceived(StatusEffect statusEffect)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusReceived, triggerSource: statusEffect.Caster.Entity, statusID: statusEffect.Data.StatusID);
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
        OnTrigger(TriggerData.eTrigger.OnStatusExpired, triggerSource: statusEffect.Caster.Entity, statusID: statusEffect.Data.StatusID);
    }

    public virtual void OnStatusEnded(StatusEffect statusEffect)
    {
        OnTrigger(TriggerData.eTrigger.OnStatusEnded, triggerSource: statusEffect.Caster.Entity, statusID: statusEffect.Data.StatusID);
    }
    #endregion

    public virtual void OnImmune(Entity source = null, Payload payloadInfo = null)
    {
        OnTrigger(TriggerData.eTrigger.OnImmune, source, payload: payloadInfo);
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

    public virtual void OnDeath(Entity source = null, PayloadComponentResult payloadResult = null)
    {
        if (!Alive)
        {
            return;
        }

        payloadResult?.Payload?.Caster?.Entity?.OnKill(payloadResult);

        if (gameObject != null)
        {
            StopAllCoroutines();
        }
        EntityState = eEntityState.Dead;
        EntityBattle?.CancelSkill();

        OnTrigger(TriggerData.eTrigger.OnDeath, triggerSource: source, payload: payloadResult?.Payload, payloadResult: payloadResult);

        EntityBattle.DisengageAll();

        RemoveAllTagsOnSelf();
        RemoveAllTagsOnEntities();

        if (Targetable != null)
        {
            Targetable.RemoveTargeting();
        }

        KillLinkedSummons();
    }

    public virtual void OnKill(PayloadComponentResult payloadResult = null, string statusID = "")
    {
        if (payloadResult == null)
        {
            return;
        }

        OnTrigger(TriggerData.eTrigger.OnKill, triggerSource: payloadResult?.Payload?.Target?.Entity, payload: payloadResult?.Payload, payloadResult: payloadResult, statusID: statusID);
    }
    #endregion

    #region Physical Triggers
    public void OnTriggerEnter(Collider other)
    {
        var entityHit = other.GetComponentInParent<Entity>();

        if (entityHit != null && entityHit.UID != UID)
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

    #region Trigger Collision Triggers
    protected virtual void OnTriggerStay(Collider other)
    {
        var entityHit = other.GetComponentInParent<Entity>();

        if (entityHit != null && entityHit.UID != UID)
        {
            OnTriggerCollisionEntity(entityHit);

            if (entityHit.IsTargetable)
            {
                OnTriggerCollisionTargetableEntity(entityHit);
            }
        }
    }

    protected virtual void OnTriggerCollisionEntity(Entity entity)
    {
        OnTrigger(TriggerData.eTrigger.OnEntityInTriggerCollider, triggerSource: entity);
    }

    protected virtual void OnTriggerCollisionTargetableEntity(Entity entity)
    {
        OnTrigger(TriggerData.eTrigger.OnTargetableInTriggerCollider, triggerSource: entity);
    }
    #endregion
    #endregion

    #region Status Effects
    #region Status
    public void ApplyStatusEffect(Entity sourceEntity, string statusID, int stacks, bool refreshDuration, bool refreshPayloads, bool isNew, Payload payload = null)
    {
        // Ensure a status effect with this ID exists and fetch its data. 
        if (!BattleData.StatusEffects.ContainsKey(statusID))
        {
            Debug.LogError($"Invalid status effect ID: {statusID}");
            return;
        }

        var statusEffectData = BattleData.StatusEffects[statusID];

        // Get caster ID and ensure it exists.
        var entity = sourceEntity.UID;
        if (statusEffectData.MultipleInstances && sourceEntity == null)
        {
            Debug.LogError($"Entity applying {statusID} status is null.");
            return;
        }

        // Remove the existing status effect if we're replacing it. 
        if (isNew && StatusEffects.ContainsKey(statusID) && StatusEffects[statusID].ContainsKey(entity))
        {
            RemoveStatusEffect(statusID, entity, false);
        }

        // If no group exists for this status effect, make a new one.
        if (!StatusEffects.ContainsKey(statusID))
        {
            if (stacks < 0)
            {
                return;
            }
            StatusEffects[statusID] = new Dictionary<string, StatusEffect>();
        }

        // If no entry exists for the caster, make a new one. 
        if (!StatusEffects[statusID].ContainsKey(entity))
        {
            StatusEffects[statusID][entity] = MakeStatusEffect(statusEffectData, sourceEntity, payload);
        }
        // If a status effect doesn't allow multiple instances and an instance from another caster already exists, replace the old instance
        else if (!statusEffectData.MultipleInstances)
        {
            var current = StatusEffects[statusID].First().Value.Caster.UID;
            if (current != entity)
            {
                RemoveStatusEffect(statusID, current, false);
                StatusEffects[statusID][entity] = MakeStatusEffect(statusEffectData, sourceEntity, payload);
            }
        }

        StatusEffects[statusID][entity].ApplyStacks(stacks, refreshDuration, refreshPayloads);

        OnStatusReceived(StatusEffects[statusID][entity]);
        sourceEntity.OnStatusApplied(this, statusID);
    }

    public void ChangeStatusEffectDuration(string statusEffect, string from, float change)
    {
        if (HasStatusEffect(statusEffect, from))
        {
            StatusEffects[statusEffect][from].ChangeDuration(change);
        }
    }

    StatusEffect MakeStatusEffect(StatusEffectData statusEffectData, Entity caster, Payload payload)
    {
        if (payload != null)
        {
            return new StatusEffect(statusEffectData, payload);
        }
        else
        {
            if (caster == null)
            {
                caster = this;
            }

            return new StatusEffect(statusEffectData, caster.EntityInfo, EntityInfo);
        }
    }

    public bool HasStatusEffect(string statusID, string from = "")
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            if (from == null || StatusEffects[statusID].ContainsKey(from))
            {
                return true;
            }
        }

        return false;
    }

    public int GetTotalStatusEffectStacks(string statusEffect)
    {
        if (StatusEffects.ContainsKey(statusEffect))
        {
            int stacks = 0;
            foreach (var instance in StatusEffects[statusEffect])
            {
                stacks += instance.Value.CurrentStacks;
            }
            return stacks;
        }

        return 0;
    }

    public int GetHighestStatusEffectStacks(string statusEffect)
    {
        if (StatusEffects.ContainsKey(statusEffect))
        {
            int stacks = 0;
            foreach (var instance in StatusEffects[statusEffect])
            {
                if (instance.Value.CurrentStacks > stacks)
                {
                    stacks = instance.Value.CurrentStacks;
                }
            }
            return stacks;
        }

        return 0;
    }

    public void ClearStatusEffect(Entity source, string statusID, ref int limit)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            var instances = StatusEffects[statusID].Count;

            foreach (var instance in StatusEffects[statusID])
            {
                var caster = instance.Key;
                instance.Value.ClearStatus();
                RemoveStatusEffect(statusID, caster);

                instances--;
                if (instances == 0)
                {
                    break;
                }

                if (limit != 0)
                {
                    limit--;
                    if (limit == 0)
                    {
                        break; ;
                    }
                }
            }

            OnStatusClearedIncoming(source, statusID);
            source.OnStatusClearedOutgoing(this, statusID);
        }
    }

    public void ClearStatusEffect(Entity source, string statusID, string statusSource)
    {
        if (StatusEffects.ContainsKey(statusID) && StatusEffects[statusID].ContainsKey(statusSource))
        {
            StatusEffects[statusID][statusSource].ClearStatus();
            StatusEffects[statusID][statusSource].RemoveEffect = true;

            OnStatusClearedIncoming(source, statusID);
            source.OnStatusClearedOutgoing(this, statusID);
        }
    }

    public void RemoveStatusEffectStacks(Entity source, string statusID, int stacks, int limit)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            foreach (var instance in StatusEffects[statusID])
            {
                instance.Value.RemoveStacks(stacks);
                if (instance.Value.CurrentStacks <= 0)
                {
                    ClearStatusEffect(source, statusID, instance.Key);
                }

                if (limit != 0)
                {
                    limit--;
                    if (limit == 0)
                    {
                        return;
                    }
                }
            }
        }
    }

    public void DelayedStatusEffectRemoval(string statusID, string casterUID)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            StatusEffects[statusID][casterUID].RemoveEffect = true;
        }
    }

    public void RemoveStatusEffect(string statusID, string casterUID, bool removeDict = true)
    {
        if (StatusEffects.ContainsKey(statusID))
        {
            OnStatusEnded(StatusEffects[statusID][casterUID]);
            StatusEffects[statusID][casterUID].RemoveStatus();
            StatusEffects[statusID].Remove(casterUID);
            if (removeDict && StatusEffects[statusID].Count < 1)
            {
                StatusEffects.Remove(statusID);
            }
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

        if (attributeChange.PayloadFilter == EffectData.ePayloadFilter.All)
        {
            SetupResourcesMax();
            EntityInfo.Attributes = EntityAttributes();
        }
    }

    public void RemoveAttributeChange(string attribute, string key)
    {
        if (!AttributeChanges.ContainsKey(attribute) || !AttributeChanges[attribute].ContainsKey(key))
        {
            return;
        }

        var update = AttributeChanges[attribute][key].PayloadFilter == EffectData.ePayloadFilter.All;

        AttributeChanges[attribute].Remove(key);

        if (update)
        {
            SetupResourcesMax();
            EntityInfo.Attributes = EntityAttributes();
        }
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

    public void RemoveImmunity(EffectData.ePayloadFilter payloadFilter, string key)
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

        if (Immunities.ContainsKey(EffectData.ePayloadFilter.All) && Immunities[EffectData.ePayloadFilter.All].Count > 0)
        {
            immunity = Immunities[EffectData.ePayloadFilter.All].ElementAt(0).Value;
        }

        if (Immunities.ContainsKey(EffectData.ePayloadFilter.Skill) && Immunities[EffectData.ePayloadFilter.Skill].Count > 0)
        {
            foreach (var i in Immunities[EffectData.ePayloadFilter.Skill])
            {
                if (i.Value.Data.PayloadName == action.SkillID)
                {
                    immunity = i.Value;
                    break;
                }
            }
        }

        if (Immunities.ContainsKey(EffectData.ePayloadFilter.SkillGroup) && Immunities[EffectData.ePayloadFilter.SkillGroup].Count > 0)
        {
            foreach (var i in Immunities[EffectData.ePayloadFilter.SkillGroup])
            {
                if (BattleData.SkillGroups.ContainsKey(i.Value.Data.PayloadName) && BattleData.SkillGroups[i.Value.Data.PayloadName].Contains(action.SkillID))
                {
                    immunity = i.Value;
                    break;
                }
            }
        }

        if (Immunities.ContainsKey(EffectData.ePayloadFilter.Action) && Immunities[EffectData.ePayloadFilter.Action].Count > 0)
        {
            foreach (var i in Immunities[EffectData.ePayloadFilter.Action])
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

        if (Immunities.ContainsKey(EffectData.ePayloadFilter.All) && Immunities[EffectData.ePayloadFilter.All].Count > 0)
        {
            immunity = Immunities[EffectData.ePayloadFilter.All].ElementAt(0).Value;
        }
        else if (Immunities.ContainsKey(EffectData.ePayloadFilter.Status) && Immunities[EffectData.ePayloadFilter.Status].Count > 0)
        {
            foreach (var i in Immunities[EffectData.ePayloadFilter.Status])
            {
                if (i.Value.Data.PayloadName == statusID)
                {
                    immunity = i.Value;
                    break;
                }
            }
        }
        else if (Immunities.ContainsKey(EffectData.ePayloadFilter.StatusGroup) && Immunities[EffectData.ePayloadFilter.StatusGroup].Count > 0)
        {
            foreach (var i in Immunities[EffectData.ePayloadFilter.StatusGroup])
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

        if (Immunities.ContainsKey(EffectData.ePayloadFilter.All) && Immunities[EffectData.ePayloadFilter.All].Count > 0)
        {
            immunity = Immunities[EffectData.ePayloadFilter.All].ElementAt(0).Value;
        }
        else if (Immunities.ContainsKey(EffectData.ePayloadFilter.Category) && Immunities[EffectData.ePayloadFilter.Category].Count > 0)
        {
            foreach (var i in Immunities[EffectData.ePayloadFilter.Category])
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

        if (EntityBattle != null && EntityBattle.CurrentSkill != null && 
            EntityBattle.CurrentSkill.Interruptible && IsSkillLocked(EntityBattle.CurrentSkill.SkillID))
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

    public bool IsAutoAttackLocked()
    {
        return Locks.ContainsKey(EffectLock.eLockType.AutoAttack);
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

    public void RemoveResistance(EffectData.ePayloadFilter payloadFilter, string key)
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
            UpdateResource(shield.Data.ShieldResource, resourceGranted);
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

    #region Saved Values
    public void SaveValue(string key, float value, int uses)
    {
        SavedValues[key] = new SavedValue(key, value, uses);
    }

    public float GetSavedValue(string key, float defaultValue = 0.0f)
    {
        if (SavedValues.ContainsKey(key))
        {
            return SavedValues[key].GetValue(this);
        }

        return defaultValue;
    }

    public void RemoveSavedValue(string key)
    {
        if (SavedValues.ContainsKey(key))
        {
            SavedValues.Remove(key);
        }
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

    public virtual void TagEntity(string tag, Entity entity, TagData tagData = null)
    {
        if (!TaggedEntities.ContainsKey(tag))
        {
            TaggedEntities.Add(tag, new Dictionary<string, float>());
        }

        if (TaggedEntities[tag].ContainsKey(entity.UID))
        {
            TaggedEntities[tag][entity.UID] = BattleSystem.Time + (tagData != null ? tagData.TagDuration : 0);
        }
        else
        {
            TaggedEntities[tag].Add(entity.UID, BattleSystem.Time + (tagData != null ? tagData.TagDuration : 0));
            entity.ApplyTag(tag, UID);

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
            TaggedEntities[tag].Remove(entity.UID);
            if (!selfOnly)
            {
                entity.RemoveTagOnSelf(tag, UID);
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
                BattleSystem.Entities[entity.Key].RemoveTagOnSelf(tag.Key, UID);
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

    public void RemoveTagFromAny(string tag, bool all)
    {
        foreach (var source in TagsAppliedBy)
        {
            foreach (var t in source.Value)
            {
                if (t == tag)
                {
                    var entity = BattleSystem.Entities[source.Key];
                    entity.RemoveTagOnEntity(tag, this, true);

                    if (!all)
                    {
                        return;
                    }
                    break;
                }
            }
        }
    }

    protected void RemoveAllTagsOnSelf()
    {
        foreach (var source in TagsAppliedBy)
        {
            if (BattleSystem.Entities.ContainsKey(source.Key))
            {
                var entity = BattleSystem.Entities[source.Key];
                foreach (var tag in source.Value)
                {
                    entity.RemoveTagOnEntity(tag, this, true);
                }
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

        if (summonAction.SummonLimit > 0)
        {
            while (SummonedEntities[summonAction.EntityID].Count >= summonAction.SummonLimit)
            {
                var entityToRemove = SummonedEntities[summonAction.EntityID][0];
                entityToRemove.OnDeath();
            }
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

        EntityInfo.Attributes = EntityAttributes();
        foreach (var resource in EntityData.Resources)
        {
            ResourcesMax[resource.Key] = Formulae.ResourceMaxValue(EntityInfo, resource.Key);
        }
    }

    protected void SetupResourcesStart()
    {
        ResourcesCurrent = new Dictionary<string, float>();

        foreach (var resource in EntityData.Resources)
        {
            ResourcesCurrent.Add(resource.Key, Formulae.ResourceMaxValue(EntityInfo, resource.Key));
        }
    }

    public void UpdateResource(string resource, float change, PayloadComponentResult payloadResult = null)
    {
        var previous = ResourcesCurrent[resource];
        ResourcesCurrent[resource] = Mathf.Clamp(ResourcesCurrent[resource] + change, 0.0f, ResourcesMax[resource]);

        var payloadInfo = payloadResult?.Payload;

        // Resource guards
        var entity = payloadInfo != null ? payloadInfo.Target : EntityInfo;
        UseResourceGuards(resource, entity);

        // Update display and set any related triggers
        if (previous != ResourcesCurrent[resource])
        {
            if (EntityCanvas != null)
            {
                EntityCanvas.UpdateResourceDisplay(resource);
            }

            if (payloadResult != null)
            {
                OnResourceChanged(payloadResult);
            }

            if (ResourcesCurrent[resource] < Constants.Epsilon && EntityData.LifeResources.Contains(resource))
            {
                OnDeath(payloadResult?.Payload?.Caster.Entity, payloadResult);
            }
        }
    }

    public void ApplyDamage(string resource, float damage, PayloadResourceChange payload, PayloadComponentResult payloadResult)
    {
        var payloadInfo = payloadResult?.Payload;

        // Shield
        if (payload != null && !payload.IgnoreShield)
        {
            UseShield(ref resource, payloadInfo, ref damage);
        }

        // Apply change
        UpdateResource(resource, -damage, payloadResult);

        // Show damage
        if (payload != null && payload.DisplayPopupText)
        {
            HUDPopupText.Instance.DisplayDamage(resource, -damage, payloadResult);
        }
    }

    public void ApplyRecovery(string resource, float recovery, PayloadResourceChange payload, PayloadComponentResult payloadResult)
    {
        // Apply change
        UpdateResource(resource, recovery, payloadResult);

        // Show recovery
        if (payload.DisplayPopupText)
        {
            HUDPopupText.Instance.DisplayDamage(resource, recovery, payloadResult);
        }
    }

    public void SetResource(string resource, float newValue, PayloadResourceChange payload, PayloadComponentResult payloadResult)
    {
        // Calculate and apply change
        var change = newValue - ResourcesCurrent[resource];

        UpdateResource(resource, change, payloadResult);

        // Show change
        if (payload != null && payload.DisplayPopupText)
        {
            HUDPopupText.Instance.DisplayDamage(resource, change, payloadResult);
        }
    }

    void UseShield(ref string resource, Payload payloadInfo, ref float damage)
    {
        // Replace the resource and update change if it's being shielded
        Shield shield = null;
        if (Shields.ContainsKey(resource) && Shields[resource].Count > 0)
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
                if (payloadInfo.PayloadData.Categories.Contains(m.Key))
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

            damage *= multiplier;

            resource = shield.Data.ShieldResource;
        }

        // Remove shield if there is one and it ran out.
        if (shield != null)
        {
            if (damage >= ResourcesCurrent[resource])
            {
                shield.RemoveShield(this);
            }
            else
            {
                shield.UseShield(this, -damage);
            }
        }
    }

    void UseResourceGuards(string resource, EntityInfo entityInfo)
    {
        // Resource guards
        if (ResourceGuards.ContainsKey(resource) && ResourceGuards[resource].Count > 0)
        {
            var resourceNow = ResourcesCurrent[resource];
            ResourceGuard min = null;
            ResourceGuard max = null;

            // Go through all guards. Use the most extreme ones applied.
            foreach (var guard in ResourceGuards[resource])
            {
                if (guard.Value.Guard(entityInfo, resourceNow, out var changedResource))
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

            ResourcesCurrent[resource] = resourceNow;
        }
    }

    public virtual void DestroyEntity()
    {
        BattleSystem.RemoveEntity(this);
        Destroy(gameObject);
    }
    #endregion

    #region Transforms
    public void SaveTransform(string transformID, Vector3 position, Vector3 forward)
    {
        SavedTransforms[transformID] = new SavedTransformInfo(position, forward);
    }

    public bool GetSavedTransform(string transformID, out Vector3 position, out Vector3 forward)
    {
        if (SavedTransforms.ContainsKey(transformID))
        {
            position = SavedTransforms[transformID].Position;
            forward = SavedTransforms[transformID].Forward;
            return true;
        }

        position = transform.position;
        forward = transform.forward;
        return false;
    }

    public bool GetSavedPosition(string transformID, out Vector3 position)
    {
        if (SavedTransforms.ContainsKey(transformID))
        {
            position = SavedTransforms[transformID].Position;
            return true;
        }

        position = transform.position;
        return false;
    }

    public bool GetSavedForward(string transformID, out Vector3 forward)
    {
        if (SavedTransforms.ContainsKey(transformID))
        {
            forward = SavedTransforms[transformID].Forward;
            return true;
        }

        forward = transform.forward;
        return false;
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

    public float Attribute(string attribute, PayloadData payload = null, Action action = null, string statusID = "", float defaultValue = 0.0f)
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
                        case EffectData.ePayloadFilter.All:
                        {
                            break;
                        }
                        case EffectData.ePayloadFilter.Action:
                        {
                            var actionID = action?.ActionID;
                            if (!string.IsNullOrEmpty(actionID) && requirement == actionID)
                            {
                                break;
                            }
                            continue;
                        }
                        case EffectData.ePayloadFilter.Category:
                        {
                            var categories = payload?.Categories;
                            if (categories != null && categories.Contains(requirement))
                            {
                                break;
                            }
                            continue;
                        }
                        case EffectData.ePayloadFilter.Skill:
                        {
                            var skillID = action?.SkillID;
                            if (!string.IsNullOrEmpty(skillID) && requirement == skillID)
                            {
                                break;
                            }
                            continue;
                        }
                        case EffectData.ePayloadFilter.SkillGroup:
                        {
                            var skillID = action?.SkillID;
                            if (!string.IsNullOrEmpty(skillID) && BattleData.SkillGroups.ContainsKey(requirement))
                            {
                                break;
                            }
                            continue;
                        }
                        case EffectData.ePayloadFilter.Status:
                        {
                            if (!string.IsNullOrEmpty(statusID) && requirement == statusID)
                            {
                                break;
                            }
                            continue;
                        }
                        case EffectData.ePayloadFilter.StatusGroup:
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
                    value += attributeChange.GetValue(EntityInfo);
                }
            }

            return value;
        }
        else
        {
            return defaultValue;
        }
    }

    public Dictionary<string, float> EntityAttributes(PayloadData payload = null, Action action = null, string statusID = "", float defaultValue = 0.0f)
    {
        var attributes = new Dictionary<string, float>();

        foreach (var attribute in BaseAttributes)
        {
            attributes.Add(attribute.Key, Attribute(attribute.Key, payload, action, statusID, defaultValue));
        }

        return attributes;
    }

    public Vector3 Origin
    {
        get
        {
            var position = Position;
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
