using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Entity))]
[CanEditMultipleObjects]
public class EntityEditor : Editor
{
    int Space = 250;
    string ID = "";

    bool ShowAttributes = false;
    bool ShowBaseAttributes = false;
    bool ShowCurrentAttributes = false;

    bool ShowResources = false;
    bool ShowStatusEffects = false;
    bool ShowTriggers = false;
    bool ShowEngagedEntities = false;

    public override bool RequiresConstantRepaint()
    {
        return Application.IsPlaying(target);
    }

    public override void OnInspectorGUI()
    {
        var entity = (Entity)target;
        SelectEntity(entity);
        if (Application.IsPlaying(entity))
        {
            if (BattleGUI.Button("Refresh Entity"))
            {
                entity.RefreshEntity();
            }

            DisplayState(entity);
            DisplayAttributes(entity);
            DisplayResources(entity);
            DisplayStatusEffects(entity);
            DisplayTriggers(entity);
            DisplayEngagedEntities(entity);
        }

        base.OnInspectorGUI();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(entity);
        }
    }

    private void OnSceneGUI()
    {
        Repaint();
    }

    protected virtual void SelectEntity(Entity entity)
    {
        SelectEntity(entity, EntityData.eEntityType.Entity);
    }

    protected void SelectEntity(Entity entity, EntityData.eEntityType entityType)
    {
        if (BattleData.Entities.Count > 0)
        {
            ID = entity.EntityID;
            BattleGUI.SelectEntity(ref ID, entityType, "Entity ID:", Space);

            if (!string.IsNullOrEmpty(ID))
            {
                if (targets.Length > 1)
                {
                    foreach (var t in targets)
                    {
                        if (t is Entity e)
                        {
                            e.UpdateID(ID);
                        }
                    }
                }
                else
                {
                    entity.UpdateID(ID);
                }
            }
        }
        else
        {
            GUILayout.BeginHorizontal();
            BattleGUI.Label("No entities found. Click to load data:", Space);
            if (BattleGUI.Button("Load"))
            {
                BattleSystemDataEditor.LoadData();
            }
            GUILayout.EndHorizontal();
        }
    }

    void DisplayState(Entity entity)
    {
        BattleGUI.StartIndent();
        BattleGUI.Label($"Entity State: {entity.EntityState}");
        BattleGUI.Label($"Entity Skill State: {entity.EntityBattle.SkillState}");
        BattleGUI.EndIndent();
    }

    void DisplayAttributes(Entity entity)
    {
        if (BattleGUI.EditFoldout(ref ShowAttributes, "Attributes"))
        {
            BattleGUI.StartIndent();
            if (entity.BaseAttributes == null || entity.BaseAttributes.Count < 1)
            {
                BattleGUI.Label("(Entity has no attributes)");
            }
            else
            {
                if (BattleGUI.EditFoldout(ref ShowBaseAttributes, "Base Attributes"))
                {
                    var attributes = entity.BaseAttributes;

                    BattleGUI.StartIndent();
                    foreach (var a in attributes)
                    {
                        BattleGUI.Label($"{a.Key}: {a.Value:0.##}");
                    }
                    BattleGUI.EndIndent();
                }
                if (BattleGUI.EditFoldout(ref ShowCurrentAttributes, "Current Attributes"))
                {
                    var attributes = entity.EntityAttributes();

                    BattleGUI.StartIndent();
                    foreach (var a in attributes)
                    {
                        BattleGUI.Label($"{a.Key}: {a.Value:0.##)}");
                    }
                    BattleGUI.EndIndent();
                }    
            }
            BattleGUI.EndIndent();
        }
    }

    void DisplayResources(Entity entity)
    {
        if (BattleGUI.EditFoldout(ref ShowResources, "Resources"))
        {
            BattleGUI.StartIndent();
            if (entity.ResourcesCurrent == null || entity.ResourcesCurrent.Count < 1)
            {
                BattleGUI.Label("(Entity has no resources)");
            }
            else
            {
                var resourcesCurrent = entity.ResourcesCurrent;
                var resourcesMax = entity.ResourcesMax;
                var resources = resourcesMax.Keys;

                BattleGUI.StartIndent();
                foreach (var r in resources)
                {
                    BattleGUI.Label($"{r}: {resourcesCurrent[r]:0.##}/{resourcesMax[r]:0.##}");
                }
                BattleGUI.EndIndent();
            }
            BattleGUI.EndIndent();
        }
    }

    void DisplayStatusEffects(Entity entity)
    {
        if (BattleGUI.EditFoldout(ref ShowStatusEffects, "Status Effects"))
        {
            BattleGUI.StartIndent();
            var statusEffects = entity.StatusEffects;

            if (statusEffects == null || statusEffects.Count < 1)
            {
                BattleGUI.Label("(Entity has no status effects)");
            }
            else
            {
                foreach (var e in statusEffects)
                {
                    BattleGUI.Label($"[{e.Key}]    Stacks: {e.Value.CurrentStacks} " +
                                    $"{(e.Value.Data.Duration > Constants.Epsilon ? $"    Time Left: {(e.Value.EndTime - BattleSystem.Time):0.##}" : "")}");
                }
            }
            BattleGUI.EndIndent();
        }
    }

    void DisplayTriggers(Entity entity)
    {
        if (BattleGUI.EditFoldout(ref ShowTriggers, "Triggers"))
        {
            BattleGUI.StartIndent();
            if (entity.Triggers == null || entity.Triggers.Count < 1)
            {
                BattleGUI.Label("(Entity has no triggers)");
            }
            else
            {
                foreach (var triggerType in entity.Triggers)
                {
                    foreach (var trigger in triggerType.Value)
                    {
                        var cd = trigger.TriggerData.Cooldown - BattleSystem.Time - trigger.LastUsedTime;
                        BattleGUI.Label($"[{triggerType.Key}]    Entity Affected: {trigger.TriggerData.EntityAffected}" +
                                       (trigger.UsesLeft > 0 ? $"   Uses Left: {trigger.UsesLeft}" : "") +
                                       (cd > Constants.Epsilon ? $"    Cooldown: {cd:0.##}" : ""));
                    }
                }
            }

            BattleGUI.EndIndent();
        }
    }

    void DisplayEngagedEntities(Entity entity)
    {
        if (BattleGUI.EditFoldout(ref ShowEngagedEntities, "Engaged Entities"))
        {
            BattleGUI.StartIndent();
            if (!entity.EntityBattle.InCombat)
            {
                BattleGUI.Label("(Entity is not engaged in battle)");
            }
            else
            {
                var entities = entity.EntityBattle.EngagedEntities;

                foreach (var e in entities)
                {
                    var selected = entity.Target.EntityUID == e.Key;
                    BattleGUI.Label($"[{(selected ? "*" : "")}{e.Key}]    Aggro: {e.Value.Aggro:0.##}");
                }
            }
            BattleGUI.EndIndent();
        }
    }
}

[CustomEditor(typeof(EntitySummon))]
public class EntitySummonEditor : EntityEditor
{
    protected override void SelectEntity(Entity entity)
    {
        SelectEntity(entity, EntityData.eEntityType.SummonnedEntity);
    }
}

[CustomEditor(typeof(Projectile))]
public class ProjectileEditor : EntityEditor
{
    protected override void SelectEntity(Entity entity)
    {
        SelectEntity(entity, EntityData.eEntityType.Projectile);
    }
}

[CustomEditor(typeof(EntityPlayer))]
public class EntityPlayerEditor : EntityEditor
{
    protected override void SelectEntity(Entity entity)
    {
        SelectEntity(entity, EntityData.eEntityType.Entity);
    }
}