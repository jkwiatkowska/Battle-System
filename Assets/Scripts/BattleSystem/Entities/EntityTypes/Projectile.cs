using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : EntitySummon
{
    class ProjectileState
    {
        public float SpeedMultiplier;           // Used to change speed, relative to entity movement speed. 
        public float RotationPerSecond;         // Rotation speed.
        public float RotationY;                 // Rotation in angles, affected by rotation speed.
        public float Timestamp;                 // Time at which the changes are applied, relative to the projectile spawning.

        public ProjectileState(ActionProjectile.ProjectileState sourceAction, float startTime)
        {
            SpeedMultiplier = Random.Range(sourceAction.SpeedMultiplier.x, sourceAction.SpeedMultiplier.y);
            RotationPerSecond = Random.Range(sourceAction.RotationPerSecond.x, sourceAction.RotationPerSecond.y);
            RotationY = Random.Range(sourceAction.RotationY.x, sourceAction.RotationY.y);
            Timestamp = sourceAction.Timestamp + startTime;
        }
    }

    // Shared
    [SerializeField] Collider TriggerCollider;
    public ActionProjectile ProjectileData      { get; protected set; }
    List<ProjectileState> ProjectileTimeline;
    float StartTime;
    int ActionIndex;
    Entity TargetEntity;
    Vector3 TargetPosition;

    // Free
    float RotationY;

    // Orbit
    Vector3 RelativePosition;

    public void ProjectileStart(ActionProjectile projectileData, Entity target)
    {
        ProjectileData = projectileData;
        StartTime = BattleSystem.Time;
        ActionIndex = 0;
        ProjectileTimeline = new List<ProjectileState>();

        if (ProjectileData.ProjectileTimeline != null)
        {
            foreach (var action in ProjectileData.ProjectileTimeline)
            {
                ProjectileTimeline.Add(new ProjectileState(action, StartTime));
            }
        }

        Movement.GravitationalForce = ProjectileData.Gravity;

        switch (projectileData.Target)
        {
            case ActionProjectile.eTarget.StaticPosition:
            {
                projectileData.TargetPosition.TryGetTransformFromData(Summoner, target, out TargetPosition, out _);
                break;
            }
            case ActionProjectile.eTarget.Caster:
            {
                TargetEntity = Summoner;
                TargetPosition = TargetEntity.Origin;
                break;
            }
            case ActionProjectile.eTarget.Target:
            {
                TargetEntity = target;
                TargetPosition = TargetEntity.Origin;
                break;
            }
            default:
            {
                Debug.LogError($"Unimplemented projectile target type: {projectileData.Target}");
                break;
            }
        }

        if (projectileData.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            Movement.Launch(TargetPosition, projectileData.ArchAngle);
        }
        else if (projectileData.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Orbit)
        {
            RelativePosition = transform.position - TargetPosition;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Alive)
        {
            if (TargetEntity != null)
            {
                TargetPosition = TargetEntity.Origin;
            }

            if (ProjectileData.ProjectileMovementMode != ActionProjectile.eProjectileMovementMode.Arched)
            {
                var speedMultiplier = 1.0f;
                var rotationPerSecond = 0.0f;

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
                        }
                    }

                    // Last action in timeline
                    if (ProjectileTimeline.Count == ActionIndex + 1)
                    {
                        speedMultiplier = currentAction.SpeedMultiplier;
                        rotationPerSecond = currentAction.RotationPerSecond;
                    }
                    else
                    {
                        var nextAction = ProjectileTimeline[ActionIndex + 1];

                        var t = BattleSystem.Time - currentAction.Timestamp / nextAction.Timestamp - currentAction.Timestamp;
                        speedMultiplier = Mathf.Lerp(currentAction.SpeedMultiplier, nextAction.SpeedMultiplier, t);
                        rotationPerSecond = Mathf.Lerp(currentAction.RotationPerSecond, nextAction.RotationPerSecond, t);
                    }
                }

                switch (ProjectileData.ProjectileMovementMode)
                {
                    case ActionProjectile.eProjectileMovementMode.Free:
                    {
                        if (rotationPerSecond > 0.0f && RotationY != 0.0f)
                        {
                            Movement.RotateY(rotationPerSecond, ref RotationY);
                        }
                        Movement.Move(transform.forward, false, speedMultiplier);
                        break;
                    }
                    case ActionProjectile.eProjectileMovementMode.Homing:
                    {
                        Movement.RotateTowardPosition(TargetPosition, rotationPerSecond);
                        Movement.Move(transform.forward, false, speedMultiplier);
                        break;
                    }
                    case ActionProjectile.eProjectileMovementMode.Orbit:
                    {
                        var previousPos = RelativePosition;

                        var movement = MovementEntity.GetEntityMovement(this, Time.fixedDeltaTime, speedMultiplier) * RelativePosition.normalized;
                        RelativePosition += movement;

                        transform.position = TargetPosition + RelativePosition;
                        transform.RotateAround(TargetPosition, Vector3.up, rotationPerSecond * Time.deltaTime);

                        RelativePosition = transform.position - TargetPosition;
                        transform.rotation = Quaternion.LookRotation(RelativePosition - previousPos);

                        break;
                    }
                    default:
                    {
                        Debug.LogError($"Unimplemented projectile mode: {ProjectileData.ProjectileMovementMode}");
                        break;
                    }
                }
            }
            else
            {
                transform.rotation = Quaternion.LookRotation(Movement.Velocity);
            }
        }
    }

    public void ProjectileReaction(ActionProjectile.OnCollisionReaction reaction, Entity entityHit = null)
    {
        switch (reaction.Reaction)
        {
            case ActionProjectile.OnCollisionReaction.eReactionType.SelfDestruct:
            {
                OnDeath();
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

    public override void OnDeath(Entity source = null, PayloadResult payloadResult = null)
    {
        TriggerCollider.enabled = false;
        Movement.GravitationalForce = 0.0f;
        Movement.Velocity = Vector3.zero;

        base.OnDeath(source, payloadResult);
    }

    protected override void OnCollisionEnemy(Entity entity)
    {
        base.OnCollisionEnemy(entity);

        foreach (var reaction in ProjectileData.OnEnemyHit)
        {
            ProjectileReaction(reaction, entity);
        }
    }

    protected override void OnCollisionFriend(Entity entity)
    {
        base.OnCollisionFriend(entity);

        foreach (var reaction in ProjectileData.OnFriendHit)
        {
            ProjectileReaction(reaction, entity);
        }
    }

    protected override void OnCollisionTerrain(Collider collider)
    {
        base.OnCollisionTerrain(collider);

    }
}
