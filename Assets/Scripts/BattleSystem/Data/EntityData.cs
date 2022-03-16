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

    // Targeting
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
    public enum eTargetPriority
    {
        Nearest,
        Furthest,
        LineOfSight,
        //Enmity,
    }

    public eTargetPriority TargetPreference;
    public float DetectDistance;
    public float DisengageDistance;
    public float DetectFieldOfView;
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