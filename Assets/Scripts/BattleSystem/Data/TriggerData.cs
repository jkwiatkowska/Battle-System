using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerData
{
    public enum eTrigger
    {
        OnHitOutgoing,      // Succesfully using a payload action
        OnHitIncoming,      // Having payload applied
        OnHitMissed,        // Failing to apply a payload action
        OnDamageDealt,      // Succesfully damaging an entity
        OnDamageReceived,   // Being damaged by another entity
        OnRecoveryDealt,    // Succesfully restoring an entity's depletable
        OnRecoveryReceived, // Having a depletable restored
        OnDeath,            // Life depletable reaches 0
        OnKill,             // Killing another entity
        // Other potential triggers:
        // - on status applied/received,
        // - on collision with another entity
    }

    public eTrigger Trigger;                    // Type of trigger.
    public float Cooldown;                      // A trigger can have a cooldown applied whenever it activates to limit its effects
    public int Limit;                           // A trigger can have a limit set. It will be removed from an entity when that limit is reached. Unlimited if 0. 

    public ActionTimeline Actions;

    public List<string> SkillIDs;               // A trigger may be limited to specific skills 
    public List<string> DepletablesAffected;    // A trigger may be limited to specific depletables being affected (for example OnRecovery only triggering when MP is recovered, but not HP)
    public List<string> Flags;                  // For damage-related triggers - applying a payload can return flags, which may be for a trigger to activate its actions
}
