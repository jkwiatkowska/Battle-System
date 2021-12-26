using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SkillSystem
{
    public static IEnumerator UseSkillCoroutine(string skillID, Entity caster, Entity selectedEntity, List<Entity> enemyEntities, List<Entity> friendlyEntities)
    {
        var skill = GameData.GetSkillData(skillID);

        if (skill.ChargeTime > 0)
        {
            StartSkillCharge(skillID, caster);
            yield return new WaitForSeconds(skill.ChargeTime);
        }

        StartSkillCast(skillID, caster);

        var currentTime = 0.0f;

        foreach (var action in skill.SkillTimeline)
        {
            yield return new WaitForSeconds(action.Timestamp - currentTime);
            currentTime = action.Timestamp;

            if (action.ActionType == SkillActionData.eSkillActionType.Direct || action.ActionType == SkillActionData.eSkillActionType.Area)
            {
                var payloadAction = action as PayloadActionData;
                if (payloadAction == null)
                {
                    Debug.LogError($"Type mismatch for payload action {action.ActionID}.");
                }

                var targets = new List<Entity>();

                if (action.ActionType == SkillActionData.eSkillActionType.Direct)
                {
                    var directAction = action as DirectActionData;
                    if (directAction == null)
                    {
                        Debug.LogError($"Type mismatch for direct action {action.ActionID}.");
                    }

                    targets = GetAffectedEntities(directAction, caster, selectedEntity, enemyEntities, friendlyEntities);
                }
                else if (action.ActionType == SkillActionData.eSkillActionType.Area)
                {
                    var areaAction = action as AreaActionData;
                    if (areaAction == null)
                    {
                        Debug.LogError($"Type mismatch for area action {action.ActionID}.");
                    }

                    targets = GetAffectedEntities(areaAction, caster, selectedEntity, enemyEntities, friendlyEntities);
                }

                // Apply payload
                var payloadData = payloadAction.Payload;
                var payload = new Payload(caster, payloadData, skillID);
                foreach (var target in targets)
                {
                    ApplyPayload(payload, target);
                }
            }
            else if (action.ActionType == SkillActionData.eSkillActionType.Summon || action.ActionType == SkillActionData.eSkillActionType.Projectile)
            {
                var summonAction = action as SummonData;
                if (summonAction == null)
                {
                    Debug.LogError($"Type mismatch for summon action {action.ActionID}.");
                }
            }
        }

        yield return new WaitForSeconds(skill.Duration - currentTime);
    }

    static void ApplyPayload(Payload payload, Entity target)
    {
        var outgoingDamage = 0.0f;
        foreach (var component in payload.OutgoingDamage)
        {
            switch(component.ComponentType)
            {
                case PayloadData.PayloadComponent.ePayloadComponentType.FlatValue:
                {
                    outgoingDamage += component.Potency;
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableCurrent:
                {
                    outgoingDamage += component.Potency * target.GetDepletableCurrent(component.Attribute);
                    break;
                }
                case PayloadData.PayloadComponent.ePayloadComponentType.TargetDepletableMax:
                {
                    outgoingDamage += component.Potency * target.GetDepletableMax(component.Attribute);
                    break;
                }
                default:
                {
                    Debug.LogError($"Error when applying payload for skill {payload.SkillID}, payload component was not converted to flat damage in Payload.cs.");
                    break;
                }
            }
        }

        var incomingDamage = GetIncomingDamage(outgoingDamage, target);
    }

    static float GetIncomingDamage(float outgoingDamage, Entity target)
    {
        return outgoingDamage;
    }

    static List<Entity> GetAffectedEntities(DirectActionData action, Entity caster, Entity selectedEntity, List<Entity> enemyEntities, List<Entity> friendlyEntities)
    {
        var targets = new List<Entity>();

        if (action.SkillTargets == DirectActionData.eDirectSkillTargets.SelectedTarget)
        {
            if (action.Target == PayloadActionData.eTarget.FriendlyEntities)
            {
                if (BattleSystem.IsFriendly(caster.EntityData.Faction, selectedEntity.EntityData.Faction))
                {
                    targets.Add(selectedEntity);
                }
                else
                {
                    targets.Add(caster);
                }
            }
            else if (action.Target == PayloadActionData.eTarget.EnemyEntities)
            {
                if (enemyEntities.Contains(selectedEntity))
                {
                    targets.Add(selectedEntity);
                }
                else if (enemyEntities.Count > 0)
                {
                    selectedEntity = enemyEntities[0];
                    targets.Add(selectedEntity);
                }
            }
        }
        else if (action.SkillTargets == DirectActionData.eDirectSkillTargets.AllTargets ||
                 action.SkillTargets == DirectActionData.eDirectSkillTargets.RandomTargets)
        {
            if (action.Target == PayloadActionData.eTarget.FriendlyEntities)
            {
                foreach (var entity in friendlyEntities)
                {
                    targets.Add(entity);
                }
            }
            else if (action.Target == PayloadActionData.eTarget.EnemyEntities)
            {
                foreach (var entity in enemyEntities)
                {
                    targets.Add(entity);
                }
            }
            if (action.SkillTargets == DirectActionData.eDirectSkillTargets.RandomTargets)
            {
                while (targets.Count > action.TargetCount)
                {
                    targets.RemoveAt(Random.Range(0, targets.Count));
                }
            }
        }

        return targets;
    }

    static List<Entity> GetAffectedEntities(AreaActionData action, Entity caster, Entity selectedEntity, List<Entity> enemyEntities, List<Entity> friendlyEntities)
    {
        var targets = new List<Entity>();

        return targets;
    }

    public static void StartSkillCharge(string skillID, Entity caster)
    {

    }

    public static void StartSkillCast(string skillID, Entity caster)
    {

    }

    public static void EndSkillCast(string skillID, Entity caster)
    {

    }

    public static void CancelSkillCast(Entity caster)
    {

    }
}
