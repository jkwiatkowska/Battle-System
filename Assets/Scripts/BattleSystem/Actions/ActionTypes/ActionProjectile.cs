using System.Collections.Generic;
using UnityEngine;

public class ActionProjectile : ActionSummon
{
    public enum eProjectileMovementMode
    {
        Free,           // The projectile moves along its forward direction. Movement and direction can be customised in the projectile timeline.
        Homing,         // The projectile turns towards the given target. Movement and turning speed can be customised in the projectile timeline.
        Arched,         // The projectile shoots up and lands at the target position. Movement direction cannot be changed.
        Orbit,          // The projectile circles around a given entity or position. Speed and direction are affected by rotation settings in projectile timeline.
    }

    public eProjectileMovementMode ProjectileMovementMode;  // This determines the trajectory a projectile takes.

    #region Projectile Triggers
    public class OnCollisionReaction
    {
        public enum eReactionType
        {
            SelfDestruct,
            Bounce,
            UseSkill,
        }

        public eReactionType Reaction;
        public string SkillID;

        public OnCollisionReaction(eReactionType reaction = eReactionType.UseSkill)
        {
            Reaction = reaction;
        }
    }

    // Some behaviours cancel one another out, but others may be used together
    public List<OnCollisionReaction> OnEnemyHit;
    public List<OnCollisionReaction> OnFriendHit;
    public List<OnCollisionReaction> OnTerrainHit;
    #endregion

    #region Projectile Timeline
    public class ProjectileState
    {
        public Vector2 SpeedMultiplier;         // Used to change speed, relative to entity movement speed. If values are different, a value in between is chosen at random. 
        public Vector2 RotationPerSecond;       // Used to change rotation speed, can be a random value.
        public Vector2 RotationY;               // Rotation around the projectile's Y axis in angles, at the given rotation speed. Used by the free movement mode.
        public string SkillID;                  // If not null, the projectile will attempt to use the skill with this ID.
        public float Timestamp;                 // Time at which the changes are applied, relative to the projectile spawning.
                                                // If there are multiple entries, speed multiplier and direction from previous and next timestamp are interpolated.

        public ProjectileState()
        {
            SpeedMultiplier = new Vector2(1.0f, 1.0f);
            RotationPerSecond = new Vector2(5.0f, 5.0f);
            RotationY = new Vector2(0.0f, 0.0f);
        }

        public ProjectileState(float timestamp):this()
        {
            Timestamp = timestamp;
        }
    }

    public List<ProjectileState> ProjectileTimeline;   // Defines speed and direction changes, as well as skill use.
    #endregion

    #region Target Modes
    public enum eTarget
    {
        StaticPosition, // The projectile will move toward a specified position.
        Caster,         // The projectile will move toward caster.
        Target,         // The projectile will move toward target.
    }

    public eTarget Target;                              // Target position for Homing, Arched and orbit modes.
    public TransformData TargetPosition;                // If using custom position for target.
    #endregion

    #region Arched Mode
    public float ArchAngle;                             // Angle along the X axis at which the projectile is shot. Must be bigger than 0 and smaller than 90.
    public float Gravity;                               // Zero for other modes.
    #endregion

    public ActionProjectile()
    {
        SetTypeDefaults();
    }

    protected override bool SetupSummon(Entity summon, Entity summoner, Entity target, Vector3 position, Vector3 forward)
    {
        var projectile = summon as Projectile;
        if (projectile == null)
        {
            Debug.LogError($"Entity {summon.name} is not a projectile.");
            return false;
        }

        // Set position and transform
        projectile.transform.position = position;
        projectile.transform.forward = forward;

        // Setup
        projectile.Setup(EntityID, summoner.Level, summoner);
        projectile.SummonSetup(this, summoner);
        projectile.ProjectileStart(this, target);

        return true;
    }

    public override void SetTypeDefaults()
    {
        base.SetTypeDefaults();

        ProjectileMovementMode = eProjectileMovementMode.Free;

        OnEnemyHit = new List<OnCollisionReaction>();
        OnFriendHit = new List<OnCollisionReaction>();
        OnTerrainHit = new List<OnCollisionReaction>();

        ProjectileTimeline = new List<ProjectileState>();

        Target = eTarget.Target;
        TargetPosition = new TransformData();

        ArchAngle = 40.0f;
        Gravity = 0.0f;
    }
}
