using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData
{
    public List<string> Categories;                         // This doesn't do anything, but can be used in damage calculations.
    public Dictionary<string, Vector2> BaseAttributes;      // Attributes such as atk, def, hp, crit chance, speed, damage resistance, etc.
                                                            // Used to calculate damage. Minimum and maximum values can be stored.

    public List<string> LifeResources;                      // If any of these resources reaches 0, the entity dies.
    public List<TriggerData> Triggers;                      // Occurences such as death or receiving damage and the actions they cause. 

    public bool IsTargetable;                               // If true, skills can be used on the entity.

    public string Faction;

    public enum eEntityType
    {
        Entity, 
        SummonnedEntity,
        Projectile
    }
    public eEntityType EntityType;                          // Specifies which entity script an entity should use.
    public bool IsAI;                                       // AI entities hold a list of skills that they automatically execute.

    public float Radius;                                    // Radius of an entity, used by area attacks.
    public float Height;                                    // Height of an entity. Used by area attacks and displaying UI elements above it.
    public float OriginHeight;                              // The middle of an entity's height, used by homing projectile attacks. 

    public float MovementSpeed;
    public float RotateSpeed;
    public float JumpHeight;
}
