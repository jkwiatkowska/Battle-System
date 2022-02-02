using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    class ProjectileAction
    {
        public float SpeedMultiplier;           // Used to change speed, relative to entity movement speed. 
        public float RotationPerSecond;         // Rotation speed.
        public float RotationY;                 // Rotation in angles, affected by rotation speed.
        public string SkillName;                // Skill for the projectile to use. Null if none. 
        public float Timestamp;                 // Time at which the changes are applied, relative to the projectile spawning.

        public ProjectileAction(ActionProjectile.ProjectileAction sourceAction, float startTime)
        {
            SpeedMultiplier = Random.Range(sourceAction.SpeedMultiplier.x, sourceAction.SpeedMultiplier.y);
            RotationPerSecond = Random.Range(sourceAction.RotationPerSecond.x, sourceAction.RotationPerSecond.y);
            RotationY = Random.Range(sourceAction.RotationY.x, sourceAction.RotationY.y);
            SkillName = sourceAction.SkillName;
            Timestamp = sourceAction.Timestamp + startTime;
        }
    }

    [SerializeField] Collider TriggerCollider;
    public ActionProjectile ProjectileData      { get; protected set; }
    List<ProjectileAction> ProjectileTimeline;
    float StartTime;
    int ActionIndex;
    Vector3 StartPosition;
    Vector3 StartForward;
    Transform TargetTransform;
    Vector3 TargetPosition;
    float RotationY;

    public override void Setup(string entityID, int entityLevel, EntitySummonDetails summonDetails = null)
    {
        base.Setup(entityID, entityLevel, summonDetails);
    }

    public void ProjectileStart(ActionProjectile projectileData, Entity target)
    {
        ProjectileData = projectileData;
        StartPosition = transform.position;
        StartForward = transform.forward;
        StartTime = BattleSystem.Time;
        ActionIndex = 0;
        ProjectileTimeline = new List<ProjectileAction>();

        if (ProjectileData.ProjectileTimeline != null)
        {
            foreach (var action in ProjectileData.ProjectileTimeline)
            {
                ProjectileTimeline.Add(new ProjectileAction(action, StartTime));
            }
        }

        Movement.GravitationalForce = ProjectileData.Gravity;

        switch (projectileData.Target)
        {
            case ActionProjectile.eTarget.None:
            {
                break;
            }
            case ActionProjectile.eTarget.Caster:
            {
                TargetTransform = SummonDetails.Summoner.transform;
                break;
            }
            case ActionProjectile.eTarget.Target:
            {
                TargetTransform = target.transform;
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented projectile target type: {projectileData.Target}");
                break;
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Alive)
        {
            switch(ProjectileData.ProjectileMovementMode)
            {
                case ActionProjectile.eProjectileMovementMode.Free:
                {
                    UpdateFreeMode();
                    break;
                }
                default:
                {
                    Debug.LogError($"Unimplemented projectile mode: {ProjectileData.ProjectileMovementMode}");
                    break;
                }
            }
        }
    }

    void UpdateFreeMode()
    {
        var speedMultiplier = 1.0f;
        var rotationMultiplier = 1.0f;

        if (ProjectileTimeline != null && ProjectileTimeline.Count > 0)
        {
            var currentAction = ProjectileTimeline[ActionIndex];

            // Not the last action on the list
            if (ProjectileTimeline.Count > ActionIndex + 1)
            {
                var nextAction = ProjectileTimeline[ActionIndex + 1];

                // Move to next action on the timeline
                while (nextAction != null && nextAction.Timestamp <= BattleSystem.Time)
                {
                    ActionIndex++;
                    currentAction = nextAction;
                    nextAction = ProjectileTimeline.Count > ActionIndex + 1 ? ProjectileTimeline[ActionIndex + 1] : null;

                    RotationY += currentAction.RotationY;

                    if (!string.IsNullOrEmpty(currentAction.SkillName))
                    {
                        TryUseSkill(currentAction.SkillName);
                    }
                }
            }

            // Last action in timeline
            if (ProjectileTimeline.Count == ActionIndex + 1)
            {
                speedMultiplier = currentAction.SpeedMultiplier;
                rotationMultiplier = currentAction.RotationPerSecond;
            }
            else
            {
                var nextAction = ProjectileTimeline[ActionIndex + 1];

                var t = BattleSystem.Time - currentAction.Timestamp / nextAction.Timestamp - currentAction.Timestamp;
                speedMultiplier = Mathf.Lerp(currentAction.SpeedMultiplier, nextAction.SpeedMultiplier, t);
                rotationMultiplier = Mathf.Lerp(currentAction.RotationPerSecond, nextAction.RotationPerSecond, t);
            }
        }

        if (rotationMultiplier > 0.0f && RotationY != 0.0f)
        {
            Movement.RotateY(rotationMultiplier, ref RotationY);
        }
        Movement.Move(transform.forward, false, speedMultiplier);
    }

    public void ProjectileReaction(ActionProjectile.OnCollisionReaction reaction, Entity entityHit = null)
    {
        switch (reaction.Reaction)
        {
            case ActionProjectile.OnCollisionReaction.eReactionType.PassThrough:
            {
                break;
            }
            case ActionProjectile.OnCollisionReaction.eReactionType.SelfDestruct:
            {
                OnTrigger(TriggerData.eTrigger.OnDeath, this);
                break;
            }
            case ActionProjectile.OnCollisionReaction.eReactionType.Bounce:
            {
                // To do
                break;
            }
            case ActionProjectile.OnCollisionReaction.eReactionType.ExecuteActions:
            {
                var target = entityHit;

                StartCoroutine(reaction.Actions.ExecuteActions(this, target));

                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented projectile reaction: {reaction.Reaction}");
                break;
            }
        }
    }

    protected override void OnDeath()
    {
        base.OnDeath();

        TriggerCollider.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        var entityHit = other.GetComponentInChildren<Entity>();

        if (entityHit != null)
        {
            if (entityHit.IsTargetable && entityHit != SummonDetails.Summoner)
            {
                if (BattleSystem.IsEnemy(EntityUID, entityHit.EntityUID))
                {
                    foreach (var reaction in ProjectileData.OnEnemyHit)
                    {
                        ProjectileReaction(reaction, entityHit);
                    }
                }
                else if (BattleSystem.IsFriendly(EntityUID, entityHit.EntityUID))
                {
                    foreach (var reaction in ProjectileData.OnFriendHit)
                    {
                        ProjectileReaction(reaction, entityHit);
                    }
                }
            }
        }
        else if (BattleSystem.IsOnTerrainLayer(other.gameObject))
        {
            foreach (var reaction in ProjectileData.OnTerrainHit)
            {
                ProjectileReaction(reaction, null);
            }
        }
    }
}
