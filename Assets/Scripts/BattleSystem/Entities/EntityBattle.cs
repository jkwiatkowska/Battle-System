using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityBattle
{
    Entity Entity;
    EntitySkillsData Data => Entity.EntityData.Skills;
    TargetingSystem Targeting => Entity.TargetingSystem;
    Entity Target => Targeting.SelectedTarget;
    MovementEntity Movement => Entity.Movement;

    public Dictionary<string, Entity> EngagedEntities { get; protected set; }
    public bool InCombat => EngagedEntities.Count > 0;

    public enum eSkillState
    {
        Idle,
        SkillPrepare,
        SkillCharge,
        SkillCast,
    }

    public eSkillState SkillState;
    
    protected Dictionary<string, float> SkillAvailableTime;
    public SkillData CurrentSkill { get; protected set; }
    bool ChargeCancelled;
    protected Coroutine SkillCoroutine;

    protected Coroutine SkillChargeCoroutine;
    public SkillChargeData SkillCharge { get; protected set; }
    public float SkillStartTime { get; protected set; }
    public float SkillChargeRatio { get; protected set; }

    protected float PrepareStartTime;
    protected SkillData PrepareSkill;

    protected float AutoAttackTime;

    public EntityBattle(Entity entity)
    {
        Entity = entity;
        SkillState = eSkillState.Idle;
        SkillAvailableTime = new Dictionary<string, float>();
        EngagedEntities = new Dictionary<string, Entity>();
    }

    #region States
    public void SetIdle()
    {
        CurrentSkill = null;
        PrepareSkill = null;
        if (Movement != null)
        {
            Movement.SetRunning(false);
        }
        SkillState = eSkillState.Idle;
        StartAutoAttack();
    }

    void SetPrepare(Entity target, SkillData skillData)
    {
        PrepareStartTime = BattleSystem.Time + 1.0f;
        SkillState = eSkillState.SkillPrepare;
        Targeting.SelectTarget(target);
        PrepareSkill = skillData;
    }

    public void Engage(Entity entity)
    {
        if (EngagedEntities.ContainsKey(entity.EntityUID) || !entity.Alive)
        {
            return;
        }

        var entityData = entity.EntityData;
        if (entityData.IsTargetable && entityData.CanEngage)
        {
            if (!InCombat)
            {
                StartAutoAttack();
            }

            Entity.OnEngage();
            EngagedEntities.Add(entity.EntityUID, entity);
        }
    }

    public void Disengage(string entityUID)
    {
        if (EngagedEntities.ContainsKey(entityUID))
        {
            EngagedEntities.Remove(entityUID);
            Entity.OnDisengage();
        }
    }

    public void DisengageAll()
    {
        foreach (var entity in EngagedEntities)
        {
            if (entity.Value != null)
            {
                entity.Value.EntityBattle.Disengage(Entity.EntityUID);
            }
        }
    }

    void StartAutoAttack()
    {
        if (Data.AutoAttack != null)
        {
            AutoAttackTime = BattleSystem.Time + Formulae.AutoAttackInterval(Entity, Target);
        }
    }
    #endregion

    #region Update
    public void Update()
    {
        // If idle and in battle, the entity can perform actions at given interval.
        PerformAutoAttack();

        // Check if an entity should use a skill and which.
        PickSkill();

        // Skill and skill charge cancelation if a skill is being cast.
        CheckForSkillCancel();
    }

    public void PerformAutoAttack()
    {
        if (SkillState == eSkillState.Idle && InCombat && Data.AutoAttack != null && AutoAttackTime <= BattleSystem.Time)
        {
            if (Data.AutoAttackRequiredTarget)
            {
                if (Target == null || !EngagedEntities.ContainsKey(Target.EntityUID))
                {
                    return;
                }
                var maxDist = Data.AutoAttackRange * Data.AutoAttackRange;
                var dist = (Target.transform.position - Entity.transform.position).sqrMagnitude;

                if (dist > maxDist)
                {
                    return;
                }
            }

            Entity.StartCoroutine(Data.AutoAttack.ExecuteActions(Entity, Target));
            StartAutoAttack();
        }
    }

    public void FixedUpdate()
    {
        // Movement and rotation toward target before a skill is cast.
        if (SkillState == eSkillState.SkillPrepare && !GetInPosition())
        {
            SetIdle();
        }
    }

    protected virtual void PickSkill()
    {
        // Read input in input mode and use skills if a corresponding key was pressed
        if (Data.SkillMode == EntitySkillsData.eSkillMode.Input)
        {
            foreach (var skill in Data.InputSkills)
            {
                if (SkillState == eSkillState.SkillCharge && CurrentSkill != null && CurrentSkill.SkillID == skill.SkillID)
                {
                    if (Input.GetKeyUp(skill.KeyCode))
                    {
                        ChargeCancelled = true;
                    }
                }

                if (Input.GetKeyDown(skill.KeyCode))
                {
                    TryUseSkill(skill.SkillID);
                    return;
                }
            }
        }
        // Otherwise select skills automatically.
        else if (Data.SkillMode != EntitySkillsData.eSkillMode.None)
        {
            if (SkillState == eSkillState.Idle)
            {
                switch (Data.SkillMode)
                {
                    case EntitySkillsData.eSkillMode.AutoSequence:
                    {
                        break;
                    }
                    case EntitySkillsData.eSkillMode.AutoRandom:
                    {
                        break;
                    }
                    case EntitySkillsData.eSkillMode.AutoBestRange:
                    {
                        break;
                    }
                }
            }
        }
    }

    protected virtual bool GetInPosition()
    {
        // Returns false if the skill can no longer be used
        var target = Targeting.SelectedTarget;
        if (target == null)
        {
            return false;
        }

        if ((PrepareSkill.PreferredTargetState == SkillData.eTargetStatePreferrence.Alive && !target.Alive) ||
            (PrepareSkill.PreferredTargetState == SkillData.eTargetStatePreferrence.Dead && target.Alive))
        {
            return false;
        }

        if (PrepareInterrupted())
        {
            return false;
        }

        var forward = Entity.transform.forward;
        var dir = (target.Origin - Entity.Origin).normalized;

        var dot = Vector3.Dot(forward, dir);

        var inRange = IsInSkillRange(target, PrepareSkill);
        if (!inRange)
        {
            // Target out of range and the entity cannot move.
            if (!Data.MoveToTargetIfNotInRange)
            {
                return false;
            }
            // Move toward the target if it's in front of the entity
            else if (dot > 0.0f)
            {
                Movement.SetRunning(true);
                Movement.Move(Entity.transform.forward, updateRotation: false, speedMultiplier: 1.0f, updateLastMoved: false);
            }
        }

        var inAngleRange = IsInSkillAngleRange(target, PrepareSkill);
        if (!inAngleRange || !inRange)
        {
            Movement.RotateTowardPosition(target.Origin);
        }
        else if (inRange && inAngleRange)
        {
            UseSkill(PrepareSkill, target);
        }

        return true;
    }

    protected virtual void CheckForSkillCancel()
    {
        if (SkillState == eSkillState.SkillCharge)
        {
            if (SkillCharge.MovementCancelsCharge && Movement != null &&
               (Movement.LastMoved > SkillStartTime || Movement.LastJumped > SkillStartTime))
            {
                ChargeCancelled = true;
            }
        }
        else if (SkillState == eSkillState.SkillCast)
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
    #endregion

    #region Skill Use
    // Returns true if a state change is triggered.
    public virtual bool TryUseSkill(string skillID)
    {
        // Return if already using this skill
        if (CurrentSkill != null && CurrentSkill.SkillID == skillID)
        {
            return false;
        }

        var skillData = BattleData.GetSkillData(skillID);

        // Make sure the skill isn't on cooldown and any mandatory costs can be afforded. 
        if (!CanUseSkill(skillData))
        {
            return false;
        }

        // If already casting another skill, interrupt it. 
        CancelSkill();

        // Ensure target if one is required.
        Targeting.UpdateEntityLists();

        var target = Targeting.SelectedTarget;

        if (skillData.NeedsTarget)
        {
            if (skillData.PreferredTarget == SkillData.eTargetPreferrence.Enemy || 
                skillData.PreferredTarget == SkillData.eTargetPreferrence.Any)
            {
                var hasTarget = skillData.PreferredTarget == SkillData.eTargetPreferrence.Any ? 
                                Targeting.SelectedTarget != null : Targeting.EnemySelected;

                // If an enemy is not selected, try selecting one.
                if (!hasTarget && Data.AutoSelectTargetOnSkillUse)
                {
                    target = Targeting.GetBestEnemy(skillData.PreferredTargetState);
                }
            }
            else if (skillData.PreferredTarget == SkillData.eTargetPreferrence.Friendly)
            {
                var hasTarget = Targeting.FriendlySelected;

                // If a friendly entity is not selected, try selecting one.
                if (!hasTarget && Data.AutoSelectTargetOnSkillUse)
                {
                    target = Targeting.GetBestFriend(skillData.PreferredTargetState);
                }
            }

            // If a target could not be found, a skill cannot be used.
            if (target == null)
            {
                return false;
            }
        }

        // Ensure target is in range if a target is required or a preferred target is selected
        if (target != null)
        {
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

        UseSkill(skillData, target);
        return true;
    }

    protected virtual void UseSkill(SkillData skillData, Entity target)
    {
        Targeting.SelectTarget(target);
        SkillCoroutine = Entity.StartCoroutine(UseSkillCoroutine(skillData));
    }

    public virtual IEnumerator UseSkillCoroutine(SkillData skillData)
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

        SetIdle();
    }

    protected virtual IEnumerator ChargeSkillCoroutine(SkillData skillData)
    {
        SkillState = eSkillState.SkillCharge;
        SkillCharge = skillData.SkillChargeData;
        SkillStartTime = BattleSystem.Time;

        if (Entity.EntityCanvas != null)
        {
            Entity.EntityCanvas.StartSkillCharge(SkillCharge, skillData.SkillID);
        }
        SkillChargeCoroutine = Entity.StartCoroutine(SkillCharge.PreChargeTimeline.ExecuteActions(Entity, Target));

        bool chargeComplete;
        var fullChargeTime = SkillCharge.FullChargeTimeForEntity(Entity);
        do
        {
            chargeComplete = BattleSystem.Time > (SkillStartTime + fullChargeTime);
            yield return null;
        }
        while (!chargeComplete && !ChargeCancelled);

        if (Entity.EntityCanvas != null)
        {
            Entity.EntityCanvas.StopSkillCharge();
        }

        var timeElapsed = BattleSystem.Time - SkillStartTime;
        var minCharge = SkillCharge.RequiredChargeTimeForEntity(Entity);
        if (timeElapsed >= minCharge)
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
        }
        yield return null;
    }

    public virtual void CancelSkill()
    {
        if (CurrentSkill == null)
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

        SetIdle();
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

    #region Checks
    protected bool IsInSkillRange(Entity target, SkillData skillData)
    {
        // No range requirement
        if (skillData.Range < 0.0f + Constants.Epsilon)
        {
            return true;
        }

        // Check if target is in range
        var distance = (target.Origin - Entity.Origin).sqrMagnitude;
        return distance < skillData.Range * skillData.Range;
    }

    protected bool IsInSkillAngleRange(Entity target, SkillData skillData)
    {
        var maxAngle = skillData.MaxAngleFromTarget + Constants.Epsilon;

        // No angle requirement
        if (maxAngle >= 180.0f)
        {
            return true;
        }

        // Check if target is in angle range
        var angle = Vector3.Angle(Entity.transform.forward, (target.transform.position - Entity.transform.position).normalized);
        return angle <= maxAngle;
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

        if (IsSkillOnCooldown(PrepareSkill.SkillID))
        {
            return true;
        }

        if (Movement != null && (Movement.LastMoved > PrepareStartTime || Movement.LastJumped > PrepareStartTime))
        {
            return true;
        }

        return false;
    }

    public virtual bool CanUseSkill(SkillData skillData)
    {
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

    protected virtual bool CanAffordCost(List<ActionCostCollection> costActions)
    {
        var costs = new Dictionary<string, float>();

        foreach (var cost in costActions)
        {
            if (costs.ContainsKey(cost.ResourceName))
            {
                costs[cost.ResourceName] += cost.GetValue(Entity);
            }
            else
            {
                costs.Add(cost.ResourceName, cost.GetValue(Entity));
            }
        }
        foreach (var cost in costs)
        {
            if (cost.Value > Entity.ResourcesCurrent[cost.Key])
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}
