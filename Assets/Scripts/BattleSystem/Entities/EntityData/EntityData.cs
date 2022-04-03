using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityData
{
    public enum eEntityType
    {
        Entity,
        SummonnedEntity,
        Projectile,
    }
    public eEntityType EntityType;                          // Specifies which entity script an entity should use.

    public EntityTargetingData Targeting;                   // Target priority and detect/disengage distance.
    public EntitySkillsData Skills;                         // Skills used by the entity and how they're used.
    public EntityMovementData Movement;                     // Movement data - defines whether an entity can move and whether it's automatic.

    public List<string> Categories;                         // This doesn't do anything, but can be used in damage calculations.
    public Dictionary<string, Vector2> BaseAttributes;      // Attributes such as atk, def, hp, crit chance, speed, damage resistance, etc.
                                                            // Used to calculate damage. Minimum and maximum values can be stored.
    #region Resources
    public class EntityResource
    {
        public string Resource;
        public Value ChangePerSecondOutOfCombat;
        public Value ChangePerSecondInCombat;
        public EntityResource()
        {
            ChangePerSecondOutOfCombat = new Value();
            ChangePerSecondInCombat = new Value();
        }

        public EntityResource(string resource) : this()
        {
            Resource = resource;
        }
    }
    public Dictionary<string, EntityResource> Resources;    // Resources available to this entity and the rate at which they recover.
    public List<string> LifeResources;                      // If any of these resources reaches 0, the entity dies.
    #endregion

    public bool IsTargetable;                               // If true, skills can be used on the entity.
    public bool CanEngage;                                  // If true, attacking the entity will enter combat.

    public string Faction;

    #region Triggers and Effects
    public List<TriggerData> Triggers;                      // Occurences such as death or receiving damage and the actions they cause. 
    public class EntityStatusEffect
    {
        public string Status;
        public int Stacks;
    }
    public List<EntityStatusEffect> StatusEffects;          // Status effects applied when an entity is set up. 
    #endregion

    public float InterruptResistance;                       // If higher than interruption level of a movement or rotation applied, the effect is ignored.
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

        Resources = new Dictionary<string, EntityResource>();
        LifeResources = new List<string>();

        Triggers = new List<TriggerData>();
        StatusEffects = new List<EntityStatusEffect>();
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
            ValueLowest, 
            ValueHighest,
            Aggro,
        }

        public eTargetPriority TargetPriority;
        public float PreferredDistanceMin;              // Entities further than this will be preferred when selecting a target.
        public float PreferredDistanceMax;              // Entities closer than this will be preferred when selecting a target.
        public bool PreferredInFront;                   // Entities in front will get a higher score regardless of other criteria.
        public ValueComponent Value;                    // For value priorities.

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
        Input,          // Entity uses skills on input from player.
    }

    public eSkillMode SkillMode;

    public class SequenceElement
    {
        public enum eElementType
        {
            Skill,          // A specific skill is used.
            RandomSkill,    // One of the given skills is used.
        }

        public eElementType ElementType;
        public string SkillID;
        public List<RandomSkill> RandomSkills;

        public int UsesMin;         // The minimum number of times this skill is used before the sequence moves on.
        public int UsesMax;         // The maximum number of uses.
        public float ExecuteChance; // The chance of this point in sequence to be executed. 

        public SequenceElement()
        {
            UsesMin = 1;
            UsesMax = 1;
            ExecuteChance = 1.0f;
            RandomSkills = new List<RandomSkill>();
        }

        public SequenceElement(string skill) : this()
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
    public class RandomSkill
    {
        public string SkillID;
        public float Weight;

        public RandomSkill()
        {
            Weight = 1.0f;
        }

        public RandomSkill(string skill) : this()
        {
            SkillID = skill;
        }

        public static string GetSkill(EntityBattle entity, List<RandomSkill> skills)
        {
            if (skills == null || skills.Count < 1)
            {
                return null;
            }

            var options = skills.Where((s) => entity.CanUseSkill(BattleData.GetSkillData(s.SkillID))).ToList();

            if (options.Count < 1)
            {
                return null;
            }

            if (options.Count == 1)
            {
                return options[0].SkillID;
            }

            var totalWeight = options.Sum(s => s.Weight);
            var targetWeight = Random.Range(0.0f, totalWeight);
            var currentWeight = 0.0f;

            foreach (var skill in options)
            {
                currentWeight += skill.Weight;
                if (currentWeight > targetWeight)
                {
                    return skill.SkillID;
                }
            }

            return null;
        }
    }

    public List<SequenceElement> SequenceSkills;    // For sequence mode.
    public List<RandomSkill> RandomSkills;          // For random mode.
    public List<InputSkill> InputSkills;            // For input mode.

    public float SkillDelayMin;                     // Delay in seconds before an entity can use a skill after the last.
    public float SkillDelayMax;                     // Can be a random value between min and max.

    public ActionTimeline AutoAttack;               // When in combat and not casting skills, an entity will repeatedly use these actions.
    public float AutoAttackInterval;                // Interval between the auto attacks.
    public bool AutoAttackRequiredTarget;           // Auto attack will only trigger if an enemy is selected.
    public float AutoAttackRange;                   // Max distance from target for an auto attack to be used.

    public bool EngageOnSight;                      // For auto skill modes.
                                                    // The entity will use its skills as soon as it spots a target they can be used on.

    public bool MoveToTargetIfNotInRange;           // Allows the entity to move toward targets that are too far away.
    public bool RotateToTargetIfNotWithinAngle;     // Allows the entity to rotate toward target if it's required to face it. 

    public bool AutoSelectTargetOnSkillUse;         // If true, the entity will try to target an entity that meets the skill requirement if one isn't selected.

    public EntitySkillsData()
    {
        SequenceSkills = new List<SequenceElement>();
        InputSkills = new List<InputSkill>();
        RandomSkills = new List<RandomSkill>();

        AutoSelectTargetOnSkillUse = true;
        RotateToTargetIfNotWithinAngle = true;

        AutoAttackRequiredTarget = true;
    }
}
