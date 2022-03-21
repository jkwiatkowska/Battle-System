using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    public Entity Entity                        { get; protected set; }
    public Entity SelectedTarget                { get; protected set; }

    public List<Entity> EnemyEntities           { get; protected set; }
    public List<Entity> FriendlyEntities        { get; protected set; }

    public List<Entity> DetectedEntities        { get; protected set; }
    List<Entity> PotentialTargets => BattleSystem.TargetableEntities;

    EntityTargetingData Targeting => Entity.EntityData.Targeting;

    public virtual void Setup(Entity entity)
    {
        Entity = entity;
        EnemyEntities = new List<Entity>();
        FriendlyEntities = new List<Entity>();
        DetectedEntities = new List<Entity>();
    }

    void Update()
    {
        var disengageDist = Targeting.DisengageDistance;

        for (int i = 0; i < DetectedEntities.Count; i++)
        {
            var entity = DetectedEntities[i];

            if (entity == null || (Entity.transform.position - entity.transform.position).sqrMagnitude > disengageDist)
            {
                DetectedEntities.Remove(entity);
                if (entity != null)
                {
                    Entity.EntityBattle.Disengage(entity.EntityUID);
                }

                if (SelectedTarget == entity)
                {
                    ClearSelection();
                }

                i--;
            }
        }
    }

    public virtual void SelectTarget(Entity entity)
    {
        ClearSelection();
        SelectedTarget = entity;
        var targetable = entity.GetComponentInChildren<Targetable>();
        targetable.Target(true, Entity);
    }

    public virtual void ClearSelection(bool selfOnly = false)
    {
        if (!selfOnly && SelectedTarget != null)
        {
            var targetable = SelectedTarget.GetComponentInChildren<Targetable>();
            targetable.Target(false, Entity);
        }
        SelectedTarget = null;
    }

    public virtual Entity GetBestEnemy(SkillData.eTargetStatePreferrence requiredState = SkillData.eTargetStatePreferrence.Any)
    {
        UpdateEntityLists();

        var potentialTargets = EnemyEntities;

        if (requiredState == SkillData.eTargetStatePreferrence.Alive)
        {
            potentialTargets = EnemyEntities.Where((e) => e.Alive).ToList();
        }
        else if (requiredState == SkillData.eTargetStatePreferrence.Dead)
        {
            potentialTargets = EnemyEntities.Where((e) => !e.Alive).ToList();
        }

        if (potentialTargets.Count == 0)
        {
            return null;
        }

        if (potentialTargets.Count > 1 && SelectedTarget == potentialTargets[0])
        {
            return potentialTargets[1];
        }

        return potentialTargets[0];
    }

    public virtual void SelectNextEnemy(SkillData.eTargetStatePreferrence requiredState = SkillData.eTargetStatePreferrence.Any)
    {
        if (SelectedTarget == null)
        {
            SelectTarget(GetBestEnemy(requiredState));
            return;
        }
        if (EnemyEntities.Count == 0)
        {
            return;
        }
        if (EnemyEntities.Count > 1)
        {
            var index = EnemyEntities.IndexOf(SelectedTarget) + 1;
            if (index >= EnemyEntities.Count)
            {
                index = 0;
            }
            SelectTarget(EnemyEntities[index]);
            return;
        }
        SelectTarget(EnemyEntities[0]);
    }

    public virtual Entity GetBestFriend(SkillData.eTargetStatePreferrence requiredState = SkillData.eTargetStatePreferrence.Any, bool selectSelf = false)
    {
        UpdateEntityLists();

        var potentialTargets = FriendlyEntities;

        if (requiredState == SkillData.eTargetStatePreferrence.Alive)
        {
            potentialTargets = FriendlyEntities.Where((e) => e.Alive).ToList();
        }
        else if (requiredState == SkillData.eTargetStatePreferrence.Dead)
        {
            potentialTargets = FriendlyEntities.Where((e) => !e.Alive).ToList();
        }

        if (potentialTargets.Count == 0)
        {
            var selectParent = selectSelf && Entity.EntityData.IsTargetable;
            return selectParent ? Entity : null;
        }
        if (potentialTargets.Count > 1 && SelectedTarget == potentialTargets[0])
        {
            return potentialTargets[1];
        }
        return potentialTargets[0];
    }

    public virtual void SelectNextFriend(SkillData.eTargetStatePreferrence requiredState = SkillData.eTargetStatePreferrence.Any, bool selectSelf = false)
    {
        if (SelectedTarget == null || FriendlyEntities.Count < 1)
        {
            SelectTarget(GetBestFriend(requiredState, selectSelf));
            return;
        }

        if (FriendlyEntities.Count > 1)
        {
            var index = Mathf.Clamp(FriendlyEntities.IndexOf(SelectedTarget) + 1, 0, FriendlyEntities.Count - 1);
            SelectTarget(FriendlyEntities[index]);
            return;
        }
    }

    public virtual void UpdateEntityLists()
    {
        var targeting = Targeting;
        var entityPos = Entity.Origin;
        var detectDist = targeting.DetectDistance * targeting.DetectDistance;
        var detectFov = targeting.DetectFieldOfView;

        // See if any entity got in or out of reach.
        foreach (var target in PotentialTargets)
        {
            if (target == Entity)
            {
                continue;
            }

            var targetPos = target.Origin;
            var dist = (entityPos - targetPos).sqrMagnitude;
            if (!DetectedEntities.Contains(target))
            {
                if (dist <= detectDist)
                {
                    if (detectFov >= 360.0f - Constants.Epsilon)
                    {
                        DetectedEntities.Add(target);
                        if (Entity.EntityData.Skills.EngageOnSight && target.EntityData.CanEngage)
                        {
                            Entity.EntityBattle.Engage(target);
                            target.EntityBattle.Engage(Entity);
                        }
                    }
                    else
                    {
                        var dir1 = (targetPos - entityPos).normalized;
                        var dir2 = Entity.transform.forward;
                        var angle = Vector3.Angle(dir1, dir2);

                        if (angle * 2.0f < detectFov)
                        {
                            DetectedEntities.Add(target);
                            if (Entity.EntityData.Skills.EngageOnSight && target.EntityData.CanEngage)
                            {
                                Entity.EntityBattle.Engage(target);
                                target.EntityBattle.Engage(Entity);
                            }
                        }
                    }
                }
            }
        }

        EnemyEntities.Clear();
        FriendlyEntities.Clear();

        // Go through detected entities and add them to the lists
        foreach(var target in DetectedEntities)
        {
            if (Entity.IsEnemy(target.Faction))
            {
                EnemyEntities.Add(target);
            }
            else if (Entity.IsFriendly(target.Faction))
            {
                FriendlyEntities.Add(target);
            }
        }

        // Sort and filter the lists as needed
        ProcessEnemyEntityList(targeting);
        ProcessFriendlyEntityList(targeting);
    }

    protected virtual void ProcessEnemyEntityList(EntityTargetingData targeting)
    {
        var scores = new Dictionary<string, float>();

        foreach (var target in EnemyEntities)
        {
            scores.Add(target.EntityUID, GetTargetScore(target, targeting.EnemyTargetPriority));
        }

        EnemyEntities.Sort((e1, e2) => scores[e2.EntityUID].CompareTo(scores[e1.EntityUID]));
    }

    protected virtual void ProcessFriendlyEntityList(EntityTargetingData targeting)
    {
        var scores = new Dictionary<string, float>();

        foreach (var target in FriendlyEntities)
        {
            scores.Add(target.EntityUID, GetTargetScore(target, targeting.FriendlyTargetPriority));
        }

        FriendlyEntities.Sort((e1, e2) => scores[e2.EntityUID].CompareTo(scores[e1.EntityUID]));
    }

    float GetTargetScore(Entity target, EntityTargetingData.TargetingPriority targeting)
    {
        if (!target.Alive)
        {
            return 0;
        }

        var score = 0.0f;

        var v = target.Origin - Entity.Origin;
        var dist = v.sqrMagnitude;
        var maxDist = targeting.PreferredDistanceMax * targeting.PreferredDistanceMax;
        var minDist = targeting.PreferredDistanceMin * targeting.PreferredDistanceMin;

        if (dist > maxDist || dist < minDist)
        {
            score -= 5.0f; // Significantly lower the score if not within preferred distance.
        }

        if (targeting.PreferredInFront)
        {
            var dot = Vector3.Dot(Entity.transform.forward, v);
            if (dot < 0.0f)
            {
                score -= 5.0f;
            }
        }

        switch (targeting.TargetPriority)
        {
            case EntityTargetingData.TargetingPriority.eTargetPriority.Any:
            {
                return score;
            }
            case EntityTargetingData.TargetingPriority.eTargetPriority.Nearest:
            {
                return score + 1.0f - dist / maxDist;
            }
            case EntityTargetingData.TargetingPriority.eTargetPriority.Furthest:
            {
                return score + dist / maxDist;
            }
            case EntityTargetingData.TargetingPriority.eTargetPriority.LineOfSight:
            {
                return score + 1.0f - (Vector3.Angle(Entity.transform.forward, v.normalized) - 180.0f) / 180.0f;
            }
        }

        return score;
    }

    public bool FriendlySelected
    {
        get
        {
            if (SelectedTarget == null)
            {
                return false;
            }

            return Entity.IsFriendly(SelectedTarget.Faction);
        }
    }

    public bool EnemySelected
    {
        get
        {
            if (SelectedTarget == null)
            {
                return false;
            }

            return Entity.IsEnemy(SelectedTarget.Faction);
        }
    }
}
