using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityBattle
{
    Entity Entity;
    EntitySkillsData Data => Entity.EntityData.Skills;
    TargetingSystem Targeting => Entity.TargetingSystem;
    Entity Target => Targeting.SelectedTarget;
    MovementEntity Movement => Entity.Movement;

    public List<System.Action> OnSkillCancel;

    public class EngagedEntity
    {
        public Entity Entity;
        public float Aggro;

        public EngagedEntity(Entity entity)
        {
            Entity = entity;
            Aggro = 0.0f;
        }
    }
    public Dictionary<string, EngagedEntity> EngagedEntities   { get; protected set; }
    public List<Entity> EngagedEntityList { get; protected set; }
    public bool InCombat => EngagedEntities.Count > 0;

    public enum eSkillState
    {
        Idle,
        SkillPrepare,
        SkillCharge,
        SkillCast,
    }

    public eSkillState SkillState                       { get; protected set; }
    public Dictionary<string, float> SkillAvailableTime { get; protected set; }

    SkillData PrepareSkill;

    Coroutine SkillChargeCoroutine;
    public SkillChargeData SkillCharge                  { get; protected set; }
    public float SkillChargeRatio                       { get; protected set; }

    public SkillData CurrentSkill                       { get; protected set; }
    bool ChargeCancelled;
    Coroutine SkillCoroutine;
    public float SkillStartTime                         { get; protected set; }

    float AutoAttackTime;

    int SequenceIndex = 0;
    int SequenceSkillUses = 0;

    string SkillToUse = "";
    float NextSkillTime = 0;

    bool HasAutoAttack;
    float RefreshTimer;

    public EntityBattle(Entity entity)
    {
        Entity = entity;
        SkillState = eSkillState.Idle;
        SkillAvailableTime = new Dictionary<string, float>();
        EngagedEntities = new Dictionary<string, EngagedEntity>();
        EngagedEntityList = new List<Entity>();
        OnSkillCancel = new List<System.Action>();
        HasAutoAttack = Data.AutoAttack != null && Data.AutoAttackRequiredTarget;
        RefreshTimer = Constants.EntityRefreshRate;

        if (Data.SequenceSkills != null && Data.SequenceSkills.Count > 0)
        {
            SequenceSkillUses = Random.Range(Data.SequenceSkills[SequenceIndex].UsesMin, Data.SequenceSkills[SequenceIndex].UsesMax + 1);
        }
    }

    #region States
    public void SetIdle()
    {
        if (Movement != null)
        {
            Movement.SetRunning(false);
        }
        SkillState = eSkillState.Idle;
        StartAutoAttack();
    }

    void SetPrepare(Entity target, SkillData skillData)
    {
        SkillState = eSkillState.SkillPrepare;
        Targeting.SelectTarget(target);
        PrepareSkill = skillData;
    }

    public void Engage(Entity entity)
    {
        if (EngagedEntities.ContainsKey(entity.UID) || !entity.Alive)
        {
            return;
        }

        var entityData = entity.EntityData;
        if (entityData.IsTargetable && entityData.CanEngage)
        {
            if (!InCombat)
            {
                StartAutoAttack();

                SequenceIndex = 0;
                if (Data.SequenceSkills != null && Data.SequenceSkills.Count > 0)
                {
                    SequenceSkillUses = Random.Range(Data.SequenceSkills[0].UsesMin, Data.SequenceSkills[0].UsesMax + 1);
                }

                Entity.OnBattleStart(entity);
            }

            Entity.OnEngage(entity);
            EngagedEntities.Add(entity.UID, new EngagedEntity(entity));
            EngagedEntityList.Add(entity);
        }
    }

    public void Disengage(string entityUID)
    {
        var wasInCombat = InCombat; 

        if (EngagedEntities.ContainsKey(entityUID))
        {
            Entity.OnDisengage(EngagedEntities[entityUID].Entity);
            EngagedEntityList.Remove(EngagedEntities[entityUID].Entity);
            EngagedEntities.Remove(entityUID);
        }

        if (wasInCombat && !InCombat)
        {
            Entity.OnBattleEnd();
            SetIdle();
        }
    }

    public void DisengageAll()
    {
        if (EngagedEntities == null || EngagedEntities.Count == 0)
        {
            return;
        }

        var entities = new List<Entity>();

        foreach (var entity in EngagedEntities)
        {
            if (entity.Value?.Entity != null)
            {
                entities.Add(entity.Value.Entity);
            }
        }
        for (int i = entities.Count - 1; i >= 0; i--)
        {
            var entity = entities[i];
            entity.EntityBattle?.Disengage(Entity.UID);
        }
    }

    void StartAutoAttack()
    {
        if (Data.AutoAttack != null)
        {
            AutoAttackTime = BattleSystem.Time + Formulae.AutoAttackInterval(Entity, Target);
        }
    }

    void SetSkillDelay()
    {
        if (Data.SkillDelayMin > Constants.Epsilon)
        {
            NextSkillTime = BattleSystem.Time + Formulae.SkillDelay(Entity);
        }
    }

    public void OnMoved()
    {
        switch (SkillState)
        {
            case eSkillState.SkillPrepare:
            {
                SetIdle();
                return;
            }
            case eSkillState.SkillCharge:
            {
                if (SkillCharge != null)
                {
                    if (SkillCharge.MovementCancelsCharge)
                    {
                        ChargeCancelled = true;
                    }
                }
                return;
            }
            case eSkillState.SkillCast:
            {
                if (CurrentSkill.MovementCancelsSkill)
                {
                    CancelSkill();
                }
                return;
            }
        }
    }
    #endregion

    #region Update
    public void Update()
    {
        // If idle and in battle, the entity can perform actions at given interval.
        if (HasAutoAttack && SkillState == eSkillState.Idle && InCombat && AutoAttackTime <= BattleSystem.Time)
        {
            PerformAutoAttack();
        }

        // Check if an entity should use a skill and which.
        PickSkill();

        // Aggro
        if (EngagedEntities.Count > 0)
        {
            UpdateAggro();
        }
    }

    public void FixedUpdate()
    {
        // Movement and rotation toward target before a skill is cast.
        if (SkillState == eSkillState.SkillPrepare && !GetInPosition())
        {
            SetIdle();
        }
        // or between skill casts.
        else if (SkillState == eSkillState.Idle && Data.MoveToTargetWhenNotUsingSkills)
        {
            GetInPosition();
        }
    }

    void UpdateAggro()
    {
        var valueInfo = new ValueInfo(Entity.EntityInfo, targetInfo: null, actionResults: null);
        foreach (var entity in EngagedEntities)
        {
            valueInfo.Target = new EntityInfo(entity.Value.Entity);
            ChangeAggro(entity.Value.Entity, BattleData.Aggro.AggroChangePerSecond.GetAggroChange(valueInfo));
        }
    }
    #endregion

    #region Skill Use
    // Returns true if a state change is triggered.
    protected virtual void PickSkill()
    {
        if (NextSkillTime > BattleSystem.Time)
        {
            return;
        }

        // Read input in input mode and use skills if a corresponding key was pressed
        if (Data.SkillMode == EntitySkillsData.eSkillMode.Input)
        {
            foreach (var skill in Data.InputSkills)
            {
                if (SkillState == eSkillState.SkillCharge && CurrentSkill != null && CurrentSkill.SkillID == skill.SkillID && skill.HoldToCharge)
                {
                    if (!Input.GetKey(skill.KeyCode))
                    {
                        ChargeCancelled = true;
                    }
                }

                if (Input.GetKeyDown(skill.KeyCode) || (skill.AllowContinuousCast && Input.GetKey(skill.KeyCode)))
                {
                    TryUseSkill(skill.SkillID);
                    return;
                }
            }
        }
        // Skills selected automatically.
        else if (SkillState == eSkillState.Idle && (Data.SkillMode == EntitySkillsData.eSkillMode.AutoSequence ||
                 Data.SkillMode == EntitySkillsData.eSkillMode.AutoRandom))
        {
            if ((Target == null || !InCombat) && Entity.EntityData.Skills.EngageOnSight)
            {
                RefreshTimer -= Time.deltaTime;
                if (RefreshTimer < 0.0f)
                {
                    UpdateTarget(true);
                }
            }

            if (!Data.UseSkillsOutOfCombat && !InCombat)
            {
                return;
            }

            if (string.IsNullOrEmpty(SkillToUse))
            {
                switch (Data.SkillMode)
                {
                    case EntitySkillsData.eSkillMode.AutoSequence:
                    {
                        if (Data.SequenceSkills != null && Data.SequenceSkills.Count > 0)
                        {
                            if (SequenceSkillUses <= 0)
                            {
                                SequenceIndex++;
                                if (SequenceIndex >= Data.SequenceSkills.Count)
                                {
                                    SequenceIndex = 0;
                                }

                                SequenceSkillUses = Random.Range(Data.SequenceSkills[SequenceIndex].UsesMin, Data.SequenceSkills[SequenceIndex].UsesMax + 1);
                            }

                            var sequenceSkill = Data.SequenceSkills[SequenceIndex];

                            if (sequenceSkill.ExecuteChance > 1.0f - Constants.Epsilon || sequenceSkill.ExecuteChance < Random.value)
                            {
                                if (sequenceSkill.ElementType == EntitySkillsData.SequenceElement.eElementType.Skill)
                                {
                                    var skill = sequenceSkill.SkillID;
                                    SkillToUse = skill;
                                }
                                else if (sequenceSkill.ElementType == EntitySkillsData.SequenceElement.eElementType.RandomSkill)
                                {
                                    if (sequenceSkill.RandomSkills != null && sequenceSkill.RandomSkills.Count > 0)
                                    {
                                        var skill = EntitySkillsData.WeightedSkill.GetSkill(this, sequenceSkill.RandomSkills);
                                        SkillToUse = skill;
                                    }
                                }
                            }

                            SequenceSkillUses--;
                        }
                        break;
                    }
                    case EntitySkillsData.eSkillMode.AutoRandom:
                    {
                        if (Data.WeightedSkills != null && Data.WeightedSkills.Count > 0)
                        {
                            var skill = EntitySkillsData.WeightedSkill.GetSkill(this, Data.WeightedSkills);
                            SkillToUse = skill;
                        }
                        break;
                    }
                }
            }
            else
            {
                if (TryUseSkill(SkillToUse))
                {
                    SkillToUse = "";
                }
            }
        }
    }

    public virtual void UpdateTarget(bool engagedOnly = true)
    {
        Targeting.SelectTarget(Targeting.GetBestEnemy(Action.eTargetState.Alive, engagedOnly: engagedOnly));
        RefreshTimer = Constants.EntityRefreshRate;
    }

    public virtual bool TryUseSkill(string skillID, Entity target = null)
    {
        var skillData = BattleData.GetSkillData(skillID);

        // Return if already using a skill that's not of a lower priority
        if (skillData.IsActive && CurrentSkill != null)
        {
            if (skillData.SkillPriority <= CurrentSkill.SkillPriority)
            {
                SkillToUse = skillID;
                return false;
            }
        }

        // Make sure the skill isn't on cooldown and any mandatory costs can be afforded. 
        if (!CanUseSkill(skillData))
        {
            return false;
        }

        if (skillData.IsActive)
        {
            // If already casting another skill, interrupt it. 
            CancelSkill();

            // Ensure target if one is required.
            Targeting.UpdateEntityLists();
            if (target == null)
            {
                target = Targeting.SelectedTarget;
            }

            // Ensure correct target.
            if (!IsCorrectTargetSelected(target, skillData) && Data.AutoSelectTargetOnSkillUse)
            {
                if (skillData.PreferredTarget == SkillData.eTargetPreferrence.Enemy || 
                    skillData.PreferredTarget == SkillData.eTargetPreferrence.Any)
                {
                    target = Targeting.GetBestEnemy(skillData.PreferredTargetState, engagedOnly: true);
                }
            }

            // If a target could not be found, a skill cannot be used.
            if (skillData.NeedsTarget && target == null)
            {
                return false;
            }

            // Ensure target is in range if a target is required or a preferred target is selected
            if (target != null)
            {
                if (skillData.NeedsTarget && !LineOfSightCheck(skillData, target))
                {
                    Entity.OnTargetNotInLineOfSight();
                    return false;
                }

                if (!IsInSkillRange(target, skillData))
                {
                    // Not in range, but the entity can automatically get in range before reattempting.
                    if (Movement != null && Data.MoveToTargetIfNotInRange)
                    {
                        SetPrepare(target, skillData);
                        return true;
                    }
                    // Not in range and the entity cannot do anything about it.
                    else if (skillData.NeedsTarget)
                    {
                        Entity.OnTargetOutOfRange();
                        return false;
                    }
                }

                if (!IsInSkillAngleRange(target, skillData))
                {
                    // Not in angle range, but the entity can automatically rotate toward target before reattempting.
                    if (Movement != null && Data.RotateToTargetIfNotWithinAngle)
                    {
                        SetPrepare(target, skillData);
                        return true;
                    }
                    // Not in angle range and the entity cannot do anything about it.
                    else if (skillData.NeedsTarget)
                    {
                        Entity.OnTargetOutOfRange();
                        return false;
                    }
                }
            }
        }

        UseSkill(skillData, target);
        return true;
    }

    protected virtual bool IsCorrectTargetSelected(Entity target, SkillData skillData)
    {
        // No target preferred.
        if (skillData.PreferredTarget == SkillData.eTargetPreferrence.None && target != null)
        {
            return false;
        }

        // A target is prefered or required, but one wasn't selected.
        if (target == null)
        {
            return false;
        }

        // Check target state.
        if (skillData.PreferredTargetState == Action.eTargetState.Alive && !target.Alive ||
            skillData.PreferredTargetState == Action.eTargetState.Dead && target.Alive)
        {
            return false;
        }

        // Check faction relations.
        if (skillData.PreferredTarget == SkillData.eTargetPreferrence.Enemy && !Entity.IsEnemy(target.Faction))
        {
            return false;
        }
        if (skillData.PreferredTarget == SkillData.eTargetPreferrence.Friendly && !Entity.IsFriendly(target.Faction))
        {
            return false;
        }

        return true;
    }

    protected virtual bool GetInPosition()
    {
        if (!Entity.Alive)
        {
            return false;
        }

        var target = Targeting.SelectedTarget;
        if (target == null)
        {
            if (SkillState == eSkillState.SkillPrepare)
            {
                SetIdle();
            }
            return false;
        }

        if (SkillState == eSkillState.SkillPrepare)
        {
            if ((PrepareSkill.PreferredTargetState == Action.eTargetState.Alive && !target.Alive) ||
                (PrepareSkill.PreferredTargetState == Action.eTargetState.Dead && target.Alive))
            {
                return false;
            }

            if (PrepareInterrupted())
            {
                return false;
            }
        }

        // Check if a status effect prevents the entity from moving.
        if (Entity.IsMovementLocked)
        {
            return false;
        }

        var forward = Entity.transform.forward;
        var dir = (target.Origin - Entity.Origin).normalized;

        var dot = Vector3.Dot(forward, dir);

        var inRange = SkillState == eSkillState.SkillPrepare ? IsInSkillRange(target, PrepareSkill) : IsInPreferredRange(target);
        if (!inRange)
        {
            // Target out of range and the entity cannot move.
            if (!Data.MoveToTargetIfNotInRange || Movement == null)
            {
                return false;
            }
            // Move toward the target if it's in front of the entity
            else if (dot > 0.0f)
            {
                Movement.SetRunning(true);
                Movement.Move(Entity.transform.forward, faceMovementDirection: false, speedMultiplier: 1.0f, setMovementTrigger: false);
            }
        }

        var inAngleRange = SkillState == eSkillState.SkillPrepare && IsInSkillAngleRange(target, PrepareSkill);
        if (!inAngleRange || !inRange)
        {
            Movement.RotateTowardPosition(target.Origin);
        }
        else if (inRange && inAngleRange && SkillState == eSkillState.SkillPrepare)
        {
            UseSkill(PrepareSkill, target);
        }

        return true;
    }

    protected virtual void UseSkill(SkillData skillData, Entity target)
    {
        if (skillData.IsActive)
        {
            if (target != null)
            {
                Targeting.SelectTarget(target);
            }
            SkillCoroutine = Entity.StartCoroutine(UseActiveSkillCoroutine(skillData));
        }
        else
        {
            Entity.StartCoroutine(skillData.SkillTimeline.ExecuteActions(Entity, target));
        }
    }

    public void ForceUseSkill(string skillID)
    {
        var skillData = BattleData.GetSkillData(skillID);

        if (skillData != null)
        {
            UseSkill(skillData, Target);
        }
    }

    public virtual IEnumerator UseActiveSkillCoroutine(SkillData skillData)
    {
        CurrentSkill = skillData;

        // Charge skill
        if (skillData.HasChargeTime)
        {
            yield return ChargeSkillCoroutine(skillData);
        }

        // Start skill cast
        SkillState = eSkillState.SkillCast;
        SkillStartTime = BattleSystem.Time;

        yield return skillData.SkillTimeline.ExecuteActions(Entity, Target);

        // Go back to idle stat if another active skill cast did not start.
        if (CurrentSkill == skillData)
        {
            CurrentSkill = null;
            SetIdle();
            SetSkillDelay();
        }
    }

    protected virtual IEnumerator ChargeSkillCoroutine(SkillData skillData)
    {
        ChargeCancelled = false;
        SkillState = eSkillState.SkillCharge;
        SkillCharge = skillData.SkillChargeData;
        SkillStartTime = BattleSystem.Time;

        if (Entity.EntityCanvas != null && SkillCharge.ShowUI)
        {
            Entity.EntityCanvas.StartSkillCharge(SkillCharge, skillData.SkillID);
        }
        SkillChargeCoroutine = Entity.StartCoroutine(SkillCharge.PreChargeTimeline.ExecuteActions(Entity, Target));

        bool chargeComplete;
        var fullChargeTime = SkillCharge.FullChargeTimeForEntity(Entity);
        var checkLineOfSight = skillData.NeedsTarget && skillData.RequireLineOfSight;
        do
        {
            chargeComplete = BattleSystem.Time > (SkillStartTime + fullChargeTime);

            if (checkLineOfSight && !LineOfSightCheck(CurrentSkill, Target))
            {
                ChargeCancelled = true;
            }
            yield return null;
        }
        while (!chargeComplete && !ChargeCancelled);

        if (SkillChargeCoroutine != null)
        {
            Entity.StopCoroutine(SkillChargeCoroutine);
        }

        if (Entity.EntityCanvas != null && SkillCharge.ShowUI)
        {
            Entity.EntityCanvas.StopSkillCharge();
        }

        var timeElapsed = BattleSystem.Time - SkillStartTime;
        var minCharge = SkillCharge.RequiredChargeTimeForEntity(Entity);
        if (timeElapsed >= minCharge - Constants.Epsilon)
        {
            var fullCharge = SkillCharge.FullChargeTimeForEntity(Entity);
            SkillChargeRatio = timeElapsed / fullCharge;

            SkillCharge = null;
        }
        else
        {
            SkillCharge = null;
            CurrentSkill = null;
            Entity.StopCoroutine(SkillCoroutine);
            SetIdle();
        }
        yield return null;
    }

    public virtual void CancelSkill(Payload payload = null) // Payload passed if skill was interrupted
    {
        if (CurrentSkill == null)
        {
            return;
        }
        
        if (payload != null && !CurrentSkill.Interruptible)
        {
            return;
        }

        if (SkillState == eSkillState.SkillCharge)
        {
            if (Entity.EntityCanvas != null)
            {
                Entity.EntityCanvas.StopSkillCharge();
            }

            if (SkillChargeCoroutine != null)
            {
                Entity.StopCoroutine(SkillChargeCoroutine);
            }
        }

        if (SkillCoroutine != null)
        {
            Entity.StopCoroutine(SkillCoroutine);
        }

        foreach (var action in OnSkillCancel)
        {
            action.Invoke();
        }

        CurrentSkill = null;
        SetSkillDelay();
        SetIdle();
    }

    public void PerformAutoAttack()
    {
        if (Target == null || !EngagedEntities.ContainsKey(Target.UID) || Entity.IsAutoAttackLocked())
        {
            return;
        }
        var maxDist = Data.AutoAttackRange * Data.AutoAttackRange;
        var dist = (Target.Position - Entity.Position).sqrMagnitude;
    
        if (dist > maxDist)
        {
            return;
        }
    
        if (Data.AutoAttackRequiresLineOfSight && !Targeting.IsInLineOfSight(Target))
        {
            return;
        }
    
        Entity.StartCoroutine(Data.AutoAttack.ExecuteActions(Entity, Target));
        StartAutoAttack();
    }
    #endregion

    #region Cooldowns
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
    #endregion

    #region Aggro
    public void ChangeAggro(Entity entity, float change)
    {
        var increase = change > Constants.Epsilon;
        if (increase && !EngagedEntities.ContainsKey(entity.UID))
        {
            Engage(entity);
        }

        if (EngagedEntities.ContainsKey(entity.UID))
        {
            EngagedEntities[entity.UID].Aggro = Mathf.Clamp(EngagedEntities[entity.UID].Aggro + change, 0.0f, 
                                                      BattleData.Aggro.MaxAggro > Constants.Epsilon ? BattleData.Aggro.MaxAggro : float.MaxValue);

            if (increase && (Target == null || (Targeting.Targeting.EnemyTargetPriority.TargetPriority == 
                EntityTargetingData.TargetingPriority.eTargetPriority.Aggro && entity != Target && 
                GetAggro(entity.UID) > GetAggro(Target.UID) + Constants.Epsilon)))
            {
                Targeting.SelectTarget(entity);
            }
        }
    }

    public float GetAggro(string entity)
    {
        if (EngagedEntities.ContainsKey(entity))
        {
            return EngagedEntities[entity].Aggro;
        }
        else
        {
            return 0.0f;
        }
    }
    #endregion

    #region Checks
    protected bool IsInSkillRange(Entity target, SkillData skillData)
    {
        // No range requirement
        if (skillData.Range < 0.0f + Constants.Epsilon)
        {
            return true;
        }

        // Check if target is in range
        return IsInRange(target.Origin, skillData.Range * skillData.Range, target.EntityData.Radius);
    }

    protected bool IsInPreferredRange(Entity target)
    {
        // No range requirement
        var range = Data.PreferredTargetRange;
        if (range < 0.0f + Constants.Epsilon)
        {
            return true;
        }

        // Check if target is in range
        return IsInRange(target.Origin, range * range, target.EntityData.Radius);
    }

    protected bool IsInRange(Vector3 pos, float maxDist, float targetRadius)
    {
        var r = Entity.EntityData.Radius;
        var distance = (pos - Entity.Origin).sqrMagnitude - r * r - targetRadius * targetRadius;

        return distance < maxDist;
    }

    protected bool IsInSkillAngleRange(Entity target, SkillData skillData)
    {
        var maxAngle = skillData.MaxAngleFromTarget + Constants.Epsilon;

        // No angle requirement
        if (maxAngle >= 180.0f - Constants.Epsilon)
        {
            return true;
        }

        // Check if target is in angle range
        var forward = Entity.transform.forward;
        forward.y = 0.0f;

        var dir = (target.Position - Entity.Position);
        dir.y = 0.0f;
        dir.Normalize();

        var angle = Vector3.Angle(forward, dir);
        return angle <= maxAngle + Constants.Epsilon;
    }

    public virtual bool PrepareInterrupted()
    {
        if (Entity.IsSkillLocked(PrepareSkill.SkillID))
        {
            return true;
        }

        if (!CanAffordCost(PrepareSkill.SkillCost))
        {
            return true;
        }

        return false;
    }

    public virtual bool CanUseSkill(SkillData skillData)
    {
        if (!StatusEffectConditionMet(skillData))
        {
            return false;
        }

        if (IsSkillOnCooldown(skillData.SkillID))
        {
            return false;
        }

        if (Entity.IsSkillLocked(skillData.SkillID))
        {
            return false;
        }

        if (!CanUseSkillInCurrentState(skillData))
        {
            return false;
        }

        if (!CanAffordCost(skillData.SkillCost))
        {
            return false;
        }

        return true;
    }

    protected virtual bool StatusEffectConditionMet(SkillData skillData)
    {
        if (skillData.StatusEffectsRequired?.Count > 0)
        {
            foreach (var effect in skillData.StatusEffectsRequired)
            {
                if (Entity.GetHighestStatusEffectStacks(effect.ID) < effect.Stacks)
                {
                    return false;
                }
            }
        }

        if (skillData.StatusEffectGroupsRequired?.Count > 0)
        {
            foreach (var effect in skillData.StatusEffectGroupsRequired)
            {
                if (!Entity.StatusEffects.ContainsKey(effect))
                {
                    return false;
                }
            }
        }

        return true;
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
        if (!skillData.IsActive)
        {
            return true;
        }

        if (Entity.EntityState == Entity.eEntityState.Dead)
        {
            return false;
        }

        switch (skillData.CasterState)
        {
            case SkillData.eCasterState.Grounded:
            {
                return Entity.Movement.IsGrounded;
            }
            case SkillData.eCasterState.Jumping:
            {
                return !Entity.Movement.IsGrounded;
            }
            default:
            {
                return true;
            }
        }
    }

    public virtual bool LineOfSightCheck(SkillData skillData, Entity target)
    {
        if (skillData.RequireLineOfSight)
        {
            return Targeting.IsInLineOfSight(target);
        }

        return true;
    }

    protected virtual bool CanAffordCost(List<ActionCostCollection> costActions)
    {
        var resources = new Dictionary<string, float>();

        foreach (var cost in costActions)
        {
            // If a cost action cannot be invoked, the cost is not collected so ignore those.
            if (!cost.ConditionsMet(Entity, Target, null))
            {
                continue;
            }

            // Ignore costs that use a resource the entity does not have. 
            // It can be changed to return false instead.
            if (Entity.ResourcesCurrent.ContainsKey(cost.ResourceName))
            {
                continue;
            }

            // Add up all the costs
            if (resources.ContainsKey(cost.ResourceName))
            {
                resources[cost.ResourceName] -= cost.GetValue(Entity);
            }
            else
            {
                resources.Add(cost.ResourceName, Entity.ResourcesCurrent[cost.ResourceName] - cost.GetValue(Entity));
            }

            if (Entity.ResourcesCurrent[cost.ResourceName] < -Constants.Epsilon)
            {
                return false;
            }
        }

        return true;
    }
    #endregion
}
