using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySkills
{
    Entity Entity;
    EntitySkillsData Data => Entity.EntityData.Skills;
    TargetingSystem Targeting => Entity.TargetingSystem;
    Entity Target => Targeting.SelectedTarget;
    MovementEntity Movement => Entity.Movement;

    public enum eSkillState
    {
        Idle,
        ChargingSkill,
        CastingSkill,
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

    public EntitySkills(Entity entity)
    {
        Entity = entity;
        SkillState = eSkillState.Idle;
        SkillAvailableTime = new Dictionary<string, float>();
    }

    public void Update()
    {
        // Skill use and behaviour
        if (Data.SkillMode == EntitySkillsData.eSkillMode.Input)
        {
            foreach (var skill in Data.InputSkills)
            {
                if (SkillState == eSkillState.ChargingSkill && CurrentSkill != null && CurrentSkill.SkillID == skill.SkillID)
                {
                    if (Input.GetKeyUp(skill.KeyCode))
                    {
                        ChargeCancelled = true;
                    }
                }

                if (Input.GetKeyDown(skill.KeyCode))
                {
                    TryUseSkill(skill.SkillID);
                }
            }
        }
        else if (Data.SkillMode != EntitySkillsData.eSkillMode.None)
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

        // Skill cancelation
        if (SkillState == eSkillState.CastingSkill)
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
        else if (SkillState == eSkillState.ChargingSkill)
        {
            if (SkillCharge.MovementCancelsCharge && Movement != null &&
               (Movement.LastMoved > SkillStartTime || Movement.LastJumped > SkillStartTime))
            {
                ChargeCancelled = true;
            }
        }
    }

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

        if (skillData.NeedsTarget)
        {
            var hasTarget = Targeting.EnemySelected;

            // If there is no target, try selecting one.
            if (!hasTarget)
            {
                Targeting.SelectBestEnemy();
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
                if (!Utility.IsInRange(Entity, Target, skillData.Range))
                {
                    Entity.OnTargetOutOfRange();
                    return false;
                }
            }
        }

        SkillCoroutine = Entity.StartCoroutine(UseSkillCoroutine(skillData));
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

        SkillState = eSkillState.CastingSkill;
        SkillStartTime = BattleSystem.Time;

        yield return skillData.SkillTimeline.ExecuteActions(Entity, Target);

        CurrentSkill = null;
        SkillState = eSkillState.Idle;
    }

    protected virtual IEnumerator ChargeSkillCoroutine(SkillData skillData)
    {
        SkillState = eSkillState.ChargingSkill;
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

        if (SkillState == eSkillState.ChargingSkill)
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

        CurrentSkill = null;
        SkillState = eSkillState.Idle;
    }

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

            if (!CanAffordCost(BattleData.GetSkillData(skillData.SkillID).SkillCost))
            {
                return false;
            }

        return true;
    }
    #endregion
}
