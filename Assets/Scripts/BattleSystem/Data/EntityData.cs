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
    public EntitySkillsData Skills;                         // Skills used by the entity and how they're used.
    public EntityMovementData Movement;                     // Movement data - defines whether an entity can move and whether it's automatic.

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

    public EntityData()
    {
        Targeting = new EntityTargetingData();
        Movement = new EntityMovementData();
        Skills = new EntitySkillsData();

        Categories = new List<string>();
        BaseAttributes = new Dictionary<string, Vector2>();

        Resources = new List<string>();
        LifeResources = new List<string>();

        Triggers = new List<TriggerData>();
    }
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

public class EntityMovementData
{
    public enum eMovementMode
    {
        Static,         // Entity cannot move at all.
        Turret,         // Entity can rotate.
        Free,           // Entity can move freely.
        Input,          // Player input controls the entity.
    }

    public eMovementMode MovementMode;

    public float MovementSpeed;
    public float MovementSpeedRunMultiplier;
    public bool ConsumeResourceWhenRunning;
    public string RunResource;
    public Value RunResourcePerSecond;

    public float RotateSpeed;
    public float JumpHeight;

    public class MovementInput
    {
        public enum eMovementType
        {
            Forward,
            Backward,
            Left,
            Right,
            RotateLeft,
            RotateRight,
            Jump,
            Dash
        }

        public eMovementType MovementType;
        public KeyCode KeyCode;
    }

    public EntityMovementData()
    {
        RunResourcePerSecond = new Value();
    }
}

public class EntitySkillsData
{
    public enum eSkillMode
    {
        None,           // Entity doesn't use skills on its own. The skill use can be implemented with custom code.
        AutoSequence,   // Entity uses skills automatically. It goes through the list in a sequence.
        AutoRandom,     // Entity uses skills from a list randomly.
        AutoBestRange,  // Entity will prioritise skills with the closest range to a suitable target.
        Input,          // Entity uses skills on input from player.
    }

    public eSkillMode SkillMode;

    public class SequenceSkill
    {
        public string SkillID;
        public int UsesMin;
        public int UsesMax;

        public SequenceSkill()
        {
            UsesMin = 1;
            UsesMax = 1;
        }

        public SequenceSkill(string skill) : this()
        {
            SkillID = skill;
        }
    }

    public class InputSkill
    {
        public string SkillID;
        public KeyCode KeyCode;
        public bool HoldToCharge;

        public InputSkill()
        {
            HoldToCharge = true;
        }

        public InputSkill(string skill) : this()
        {
            SkillID = skill;
        }
    }

    public List<string> Skills;                 // For random and range modes.
    public List<SequenceSkill> SequenceSkills;  // For sequence mode.
    public List<InputSkill> InputSkills;        // For input mode.

    public ActionTimeline AutoAttack;           // When in combat and not casting skills, an entity will repeatedly use these actions.
    public float AutoAttackInterval;            // Interval between the auto attacks.

    public bool UseSkillOnSight;    // For auto skill modes.
                                    // The entity will use its skills as soon as it spots a target they can be used on.

    public EntitySkillsData()
    {
        Skills = new List<string>();
        SequenceSkills = new List<SequenceSkill>();
        InputSkills = new List<InputSkill>();
        AutoAttack = new ActionTimeline();
    }
}
