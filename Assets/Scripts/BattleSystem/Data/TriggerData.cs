using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerData
{
    public enum eTrigger
    {
        OnHitOutgoing,      // Succesfully using a payload action.
        OnHitIncoming,      // Having payload applied.
        OnHitMissed,        // Failing to apply a payload action.
        OnDamageDealt,      // Succesfully damaging an entity.
        OnDamageTaken,      // Being damaged by another entity.
        OnRecoveryGiven,    // Succesfully restoring an entity's resource.
        OnRecoveryReceived, // Having a resource restored.
        OnDeath,            // Life resource reaches 0.
        OnKill,             // Killing another entity.
        OnSpawn,            // Fires after setup.
        OnCollisionEnemy,   // Fires on collision with an enemy.
        OnCollisionFriend,  // Fireso on collision with a friend.
        OnCollisionTerrain, // Fires on collision with an object on terrain layer.
        // Other potential triggers:
        // - on status applied/received,
        // - on collision with another entity
    }

    public eTrigger Trigger;                    // Type of trigger.
    public float TriggerChance = 1.0f;          // The odds of an effect triggering. 
    public float Cooldown;                      // A trigger can have a cooldown applied whenever it activates to limit its effects
    public int Limit;                           // A trigger can have a limit set. It will be removed from an entity when that limit is reached. Unlimited if 0. 

    public ActionTimeline Actions;

    public List<string> SkillIDs;               // A trigger may be limited to specific skills 
    public List<string> ResourcesAffected;      // A trigger may be limited to specific resources being affected (for example OnRecovery only triggering when MP is recovered, but not HP)
    public List<string> Flags;                  // For damage-related triggers - applying a payload can return flags, which may be for a trigger to activate its actions
}
