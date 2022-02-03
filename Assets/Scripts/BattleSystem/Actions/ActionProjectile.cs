using System.Collections.Generic;
using UnityEngine;

public class ActionProjectile : ActionSummon
{
    public enum eProjectileMovementMode
    {
        Free,           // The projectile moves along its forward direction. Movement and direction can be customised in the projectile timeline.
        Homing,         // The projectile turns towards the given target. Movement and turning speed can be customised in the projectile timeline.
        Arched,         // The projectile shoots up and lands at the target position. Movement direction cannot be changed.
        CircleAround,   // The projectile circles around a given entity or position. Speed and direction are affected by rotation settings in projectile timeline.
    }

    public eProjectileMovementMode ProjectileMovementMode;  // This determines the trajectory a projectile takes.

    #region Projectile Triggers
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

    // Some behaviours cancel one another out, but others may be used together
    public List<OnCollisionReaction> OnEnemyHit;
    public List<OnCollisionReaction> OnFriendHit;
    public List<OnCollisionReaction> OnTerrainHit;
    #endregion

    #region Projectile Timeline
    public class ProjectileAction
    {
        public Vector2 SpeedMultiplier;         // Used to change speed, relative to entity movement speed. If values are different, a value in between is chosen at random. 
        public Vector2 RotationPerSecond;       // Used to change rotation speed, can be a random value.
        public Vector2 RotationY;               // Rotation around the projectile's Y axis in angles, at the given rotation speed.
        public string SkillName;                // Skill for the projectile to use. Leave blank if none. 
        public float Timestamp;                 // Time at which the changes are applied, relative to the projectile spawning.
                                                // If there are multiple entries, speed multiplier and direction from previous and next timestamp are weighted.
    }

    public List<ProjectileAction> ProjectileTimeline;   // Defines speed and direction changes, as well as skill use.
    #endregion

    #region Homing Mode
    public enum eTarget
    {
        StaticPosition, // The projectile will move toward a specified position.
        Caster,         // The projectile will move toward caster. This will create a homing effect.
        Target,         // The projectile will move toward target. This will create a homing effect.
    }

    public eTarget Target;                              // Target position for homing mode.
    public TransformData TargetPosition;                // If using custom position for target. Also used by arched mode.
    #endregion

    #region Arched Mode
    public float ArchAngle;                             // Angle along the X axis at which the projectile is shot.
    public float Gravity;                               // Zero for other modes.
    #endregion

    #region Circle Mode
    public enum eAnchor // A projectile can moves relative to its anchor position.
    {
        None,
        SpawnPosition,
        CustomPosition,
        Caster,
        Target
    }

    public eAnchor Anchor;                              // Anchor position affects horizontal movement of the projectile.
    public TransformData AnchorPosition;                // If using custom position for anchor
    #endregion

    public override void Execute(Entity entity, Entity target, ref Dictionary<string, ActionResult> actionResults)
    {
        base.Execute(entity, target, ref actionResults);

        if (!actionResults[ActionID].Success)
        {
            return;
        }

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
