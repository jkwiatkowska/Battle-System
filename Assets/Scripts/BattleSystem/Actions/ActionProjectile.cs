using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionProjectile : ActionSummon
{
    public class OnCollisionReaction
    {
        public enum eReactionType
        {
            PassThrough,
            SelfDestruct,
            Bounce,
            ExecuteActions
        }

        public eReactionType Reaction;
        public ActionTimeline Actions; 
    }

    public class ProjectileAction
    {
        public Vector2 SpeedMultiplier;         // Used to change speed, relative to entity movement speed. If values are different, a value in between is chosen at random. 
        public (Vector3, Vector3) Direction;    // Movement direction relative to the current forward and anchor. If values are different, a value in between is chosen.
        public string SkillName;                // Skill for the projectile to use. Leave blank if none. 
        public TransformData NewOrigin;         // Origin can be changed to a new position defined here. Blank if origin is to stay the same.
        public float Timestamp;                 // Time at which the changes are applied, relative to the projectile spawning.
                                                // If there are multiple entries, speed multiplier and direction from previous and next timestamp are weighted.
    }

    public enum eTarget
    {
        None,           // The projectile will move along its forward.
        Caster,         // The projectile will move toward caster. This will create a homing effect.
        Target,         // The projectile will move toward target. This will create a homing effect. 
        CustomPosition  // The projectile will move towards a specific position
    }

    public enum eAnchor // A projectile moves relative to its anchor position.
    {
        None,
        SpawnPosition,
        CustomPosition,
        Caster,
        Target
    }

    public List<ProjectileAction> ProjectileTimeline;   // Defines speed and direction changes, as well as skill use.

    public eTarget Target;                              // Target position affects the forward movement of the projectile.
    public TransformData TargetPosition;                // If using custom position for target
    public eAnchor Anchor;                              // Anchor position affects horizontal movement of the projectile.
    public TransformData AnchorPosition;                // If using custom position for anchor
    public float Gravity;                               // Zero by default.

    // Some behaviours cancel one another out, but others may be used together
    public List<OnCollisionReaction> OnEnemyHit;
    public List<OnCollisionReaction> OnFriendHit;
    public List<OnCollisionReaction> OnTerrainHit;

    public override void Execute(Entity entity, out ActionResult actionResult, Entity target)
    {
        base.Execute(entity, out actionResult, target);

        var projectile = SummonnedEntity as Projectile;
        if (projectile != null)
        {
            projectile.ProjectileStart(this, target);
        }
        else
        {
            Debug.LogError($"Entity {SummonnedEntity.name} is not a projectile.");
        }
    }
}
