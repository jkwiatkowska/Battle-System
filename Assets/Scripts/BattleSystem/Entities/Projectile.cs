using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity
{
    class ProjectileAction
    {
        public float SpeedMultiplier;           // Used to change speed, relative to entity movement speed. 
        public Vector3 Direction;               // Movement direction relative to the current forward and anchor.
        public string SkillName;                // Skill for the projectile to use. Null if none. 
        public TransformData NewOrigin;         // Origin can be changed to a new position defined here. Null if origin is to stay the same.
        public float Timestamp;                 // Time at which the changes are applied, relative to the projectile spawning.

        public ProjectileAction(ActionProjectile.ProjectileAction sourceAction, float startTime)
        {
            SpeedMultiplier = Random.Range(sourceAction.SpeedMultiplier.x, sourceAction.SpeedMultiplier.y);
            Direction = new Vector3(Random.Range(sourceAction.Direction.Item1.x, sourceAction.Direction.Item2.x),
                                    Random.Range(sourceAction.Direction.Item1.y, sourceAction.Direction.Item2.y),
                                    Random.Range(sourceAction.Direction.Item1.z, sourceAction.Direction.Item2.z));
            SkillName = sourceAction.SkillName;
            NewOrigin = sourceAction.NewOrigin;
            Timestamp = sourceAction.Timestamp + startTime;
        }
    }

    [SerializeField] Collider TriggerCollider;
    public ActionProjectile ProjectileData      { get; protected set; }
    List<ProjectileAction> ProjectileTimeline;
    float StartTime;
    int ActionIndex;
    Vector3 StartPosition;
    Transform TargetTransform;
    Vector3 TargetPosition;

    public override void Setup(string entityID, int entityLevel, EntitySummonDetails summonDetails = null)
    {
        base.Setup(entityID, entityLevel, summonDetails);
    }

    public void ProjectileStart(ActionProjectile projectileData, Entity target)
    {
        ProjectileData = projectileData;
        StartPosition = transform.position;
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
            case ActionProjectile.eTarget.CustomPosition:
            {
                ProjectileData.TargetPosition.TryGetTransformFromData(SummonDetails.Summoner, target, out var position, out var forward);
                TargetPosition = position;
                break;
            }
            default:
            {
                Debug.LogError($"Unsupported projectile target type: {projectileData.Target}");
                break;
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.Update();

        if (Alive)
        {
            var direction = transform.forward;
            var speedMultiplier = 1.0f;

            if (ProjectileTimeline.Count > 0)
            {
                var currentAction = ProjectileTimeline[ActionIndex];

                if (currentAction.Timestamp >= BattleSystem.Time)
                {
                    // Not the last action on the list
                    if (ProjectileTimeline.Count > ActionIndex + 1)
                    {
                        var nextAction = ProjectileTimeline[ActionIndex + 1];

                        // Move to next action on the timeline
                        while (nextAction != null && nextAction.Timestamp >= BattleSystem.Time)
                        {
                            ActionIndex++;
                            currentAction = nextAction;
                            nextAction = ProjectileTimeline.Count > ActionIndex + 1 ? ProjectileTimeline[ActionIndex + 1] : null;

                            // To do: execute skill and set origin if needed.
                        }
                    }

                    // Last action in timeline
                    if (ProjectileTimeline.Count == ActionIndex + 1)
                    {
                        speedMultiplier = currentAction.SpeedMultiplier;
                        direction = currentAction.Direction;
                    }
                    else
                    {
                        var nextAction = ProjectileTimeline[ActionIndex + 1];

                        var t = BattleSystem.Time - currentAction.Timestamp / nextAction.Timestamp - currentAction.Timestamp;
                        speedMultiplier = Mathf.Lerp(currentAction.SpeedMultiplier, nextAction.SpeedMultiplier, t);
                        direction = Vector3.Lerp(currentAction.Direction, nextAction.Direction, t);
                    }
                }
            }

            Movement.Move(direction, speedMultiplier);
        }
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
                Debug.LogError($"Unsupported projectile reaction: {reaction.Reaction}");
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

        if (entityHit != null && entityHit.IsTargetable && entityHit != SummonDetails.Summoner)
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
        else if (BattleSystem.IsOnTerrainLayer(other.gameObject))
        {
            foreach (var reaction in ProjectileData.OnTerrainHit)
            {
                ProjectileReaction(reaction, null);
            }
        }
    }
}
