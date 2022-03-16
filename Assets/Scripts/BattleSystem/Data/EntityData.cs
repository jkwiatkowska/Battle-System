using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData
{
    public enum eEntityType
    {
        Entity,
        SummonnedEntity,
        Projectile
    }
    public eEntityType EntityType;                          // Specifies which entity script an entity should use.

    public EntityTargetingData Targeting;                   // Target priority and detect/disengage distance.
    // Skill use 

    public List<string> Categories;                         // This doesn't do anything, but can be used in damage calculations.
    public Dictionary<string, Vector2> BaseAttributes;      // Attributes such as atk, def, hp, crit chance, speed, damage resistance, etc.
                                                            // Used to calculate damage. Minimum and maximum values can be stored.

    public List<string> Resources;                          // Resources available to this entity.
    public List<string> LifeResources;                      // If any of these resources reaches 0, the entity dies.

    public bool IsTargetable;                               // If true, skills can be used on the entity.

    public string Faction;

    public List<TriggerData> Triggers;                      // Occurences such as death or receiving damage and the actions they cause. 

    public float Radius;                                    // Radius of an entity, used by area attacks.
    public float Height;                                    // Height of an entity. Used by area attacks and displaying UI elements above it.
    public float OriginHeight;                              // The middle of an entity's height, used by homing projectile attacks. 

    public float MovementSpeed;
    public float RotateSpeed;
    public float JumpHeight;

    public EntityData()
    {
        Targeting = new EntityTargetingData();

        Categories = new List<string>();
        BaseAttributes = new Dictionary<string, Vector2>();

        Resources = new List<string>();
        LifeResources = new List<string>();

        Triggers = new List<TriggerData>();
    }
}

public class EntityMovementData
{
    public enum eMovementMode
    {
        Free,           // Can move freely.
        Static,         // No movement.
        AutoTurret,     // Rotate around/toward target.
        GoToPos,        // Entity rotates and moves toward target or given position.
    }

    public eMovementMode MovementMode;

    public float MovementMultiplier;
    public float RotateMultiplier;
    public float JumpHeightMultiplier;

    // Go to pos
    public float RangeFromPos;

}

public class EntityTargetingData
{
    public class TargetingPriority
    {
        public enum eTargetPriority
        {
            Any,
            Nearest,
            Furthest,
            LineOfSight,
            //Enmity,
        }

        public eTargetPriority TargetPriority;
        public float PreferredDistanceMin;              // Entities further than this will be preferred when selecting a target.
        public float PreferredDistanceMax;              // Entities closer than this will be preferred when selecting a target.
        public bool PreferredInFront;                  // Entities in front will get a higher score regardless of other criteria.

        public TargetingPriority()
        {
            PreferredDistanceMin = 0.0f;
            PreferredDistanceMax = 5.0f;
        }
    }

    public TargetingPriority EnemyTargetPriority;       // Targeting preference when targeting enemy entities.
    public TargetingPriority FriendlyTargetPriority;    // Targeting preference when targeting friendly entities.

    public float DetectDistance;                        // A distance at which entities are detected and can be targeted.
    public float DetectFieldOfView;                     // Detection can be limited to a specified angle.

    public float DisengageDistance;                     // If a detected entity moves further than this, it cannot be targeted or attacked.

    public EntityTargetingData()
    {
        EnemyTargetPriority = new TargetingPriority();
        FriendlyTargetPriority = new TargetingPriority();

        DetectDistance = 10.0f;
        DetectFieldOfView = 360.0f;

        DisengageDistance = 15.0f;
    }
}

public class EntitySkillData
{
    public enum eSkillMode
    {
        None,           // Entity doesn't use skills on its own. The skill use can be implemented with custom code.
        Input,          // Entity uses skills on input from player.
        AutoSequence,   // Entity uses skills automatically. It goes through the list in a sequence.
        AutoRandom,     // Entity uses skills from a list randomly.
        AutoBestRange,  // Entity will prioritise skills with the closest range to a suitable target.
    }

    public eSkillMode SkillMode;
    public List<EntitySkill> Skills;

    // Auto
    public bool UseSkillOnSight;    // The entity will use its skills as soon as it spots a target they can be used on.
    public bool MoveToTarget;       // The entity will move toward target if it's too far. Otherwise it will stay idle.
}

public class EntitySkill
{
    public string SkillID;
}