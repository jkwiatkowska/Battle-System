using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityData
{
    public List<string> Affinities;                         // This doesn't do anything, but can be used in damage calculations.
    public Dictionary<string, float> BaseAttributes;        // Attributes such as atk, def, hp, crit chance, speed, damage resistance, etc. Used to calculate damage.

    public List<string> LifeDepletables;                    // If any of these depletables reaches 0, the entity dies.
    public List<TriggerData> Triggers;                      // Occurences such as death or receiving damage and the actions they cause. 

    public bool IsTargetable;                               // If true, skills can be used on the entity.

    public string Faction;

    public bool IsAI;                                       // AI entities hold a list of skills that they automatically execute.

    public float Radius;
    public float Height;

    public float MovementSpeed;
    public float RotateSpeed;
    public float JumpHeight;
}
