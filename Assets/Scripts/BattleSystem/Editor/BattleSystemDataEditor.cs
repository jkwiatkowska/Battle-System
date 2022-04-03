using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BattleSystemDataEditor : EditorWindow
{
    int Tab = 0;
    Vector2 ScrollPos;
    bool ShowHelp;
    const string PathPlayerPrefs = "BattleDataPath";
    static string Path = "Data/BattleData";
    const int Space = BattleGUI.Space;

    #region Editor variables
    #region GameData
    bool ShowAttributes = false;
    List<string> EntityAttributes = new List<string>();
    MultiplierAttributeData Multipliers = new MultiplierAttributeData();
    bool ShowMultipliers = false;
    bool ShowDamageDealtMultipliers = false;
    bool ShowDamageReceivedMultipliers = false;
    bool ShowCategoryDamageMultipliers = false;
    bool ShowCategoryDamageReceivedMultipliers = false;
    bool ShowInterruptResistMultipliers = false;
    bool ShowMovementMultipliers = false;
    bool ShowRotationMultipliers = false;
    bool ShowJumpMultipliers = false;
    string NewMultiplierCategory = "";
    string NewMultiplierPayloadFlag = "";

    string NewAttribute = "";

    bool ShowResources = false;
    class EditorResource
    {
        public string Name;
        public Value Value;

        public EditorResource()
        {
            Name = "";
            Value = new Value();
        }

        public EditorResource(string name, Value value)
        {
            Name = name;
            Value = value;
        }
    }
    List<EditorResource> EntityResources = new List<EditorResource>();
    EditorResource NewResource = new EditorResource();

    bool ShowCategories = false;
    bool ShowPayloadCategories = false;
    List<string> PayloadCategories = new List<string>();
    string NewPayloadCategory = "";

    bool ShowEntityCategories = false;
    List<string> EntityCategories = new List<string>();
    string NewEntityCategory = "";

    bool ShowPayloadFlags = false;
    List<string> PayloadFlags = new List<string>();
    string NewPayloadFlag = "";

    bool ShowAggro = false;
    AggroData Aggro = new AggroData();

    bool ShowFactions = false;

    class EditorFaction
    {
        public FactionData Data;
        public string NewFriendlyFaction = "";
        public string NewEnemyFaction = "";

        public EditorFaction(FactionData data)
        {
            Data = data.Copy();
        }
    }
    List<EditorFaction> Factions = new List<EditorFaction>();
    string NewFaction = "";
    #endregion

    #region Skill Data
    bool ShowSkillData = false;
    class EditorSkill
    {
        public bool ShowSkill = false;
        public string SkillID = "";
        public SkillData SkillData = new SkillData();
        public Action.eActionType NewTimelineAction;
        public bool ShowTimeline = false;
        public Action.eActionType NewChargeTimelineAction;
        public bool ShowChargeTimeline = false;
        public bool HasChargeTime = false;
        public bool ShowCharge = false;
        public bool ShowTarget = false;

        public EditorSkill(SkillData skillData = null)
        {
            SkillData = BattleGUI.Copy(skillData);
            SkillID = SkillData.SkillID;

            HasChargeTime = SkillData.HasChargeTime;

            if (HasChargeTime && SkillData.SkillChargeData.PreChargeTimeline == null)
            {
                SkillData.SkillChargeData.PreChargeTimeline = new ActionTimeline();
            }

            foreach (var action in SkillData.SkillTimeline)
            {
                if (action is ActionPayload payloadAction)
                {
                    EditorPayloadAction.AddPayloadAction(payloadAction);
                }
            }
        }
    }

    List<EditorSkill> Skills = new List<EditorSkill>();
    string NewSkill = "";


    bool ShowSkillGroups = false;
    class EditorSkillGroup
    {
        public string GroupID = "";
        public List<string> Skills = new List<string>();
        public string NewSkill = "";

        public EditorSkillGroup(string id = "", List<string> list = null)
        {
            GroupID = id;
            if (list != null)
            {
                Skills = BattleGUI.CopyList(list);
            }
        }
    }
    List<EditorSkillGroup> SkillGroups = new List<EditorSkillGroup>();
    string NewSkillGroup;

    class EditorPayload
    {
        public string NewCategory = "";
        public string NewAttribute = "";
        public string NewFlag = "";
        public string NewStatusApply = "";
        public string NewStatusRemoveStack = "";
        public string NewEntityCategory = "";
        public (bool, string) NewStatusClear = (false, "");
        public bool ShowTimeline = false;
        public EditorPayload AlternatePayload = null;

        public bool ShowCategories = false;
        public bool ShowPayloadDamage = false;
        public bool ShowAggro = false;
        public bool ShowEffectiveness = false;
        public bool ShowIgnoredAttributes = false;
        public bool ShowTag = false;
        public bool ShowFlags = false;
        public bool ShowEffects = false;
        public bool ShowConditions = false;
        public bool ShowOtherEffects = false;
        public bool ShowTransformChanges = false;
        public bool ShowMovement = false;
        public bool ShowRotation = false;

        public EditorPayload(PayloadData payloadData)
        {
            if (payloadData.AlternatePayload != null)
            {
                AlternatePayload = new EditorPayload(payloadData.AlternatePayload);
            }
        }
    }

    class EditorPayloadAction : List<EditorPayload>
    {
        public bool ShowPayloads;
        public bool ShowAreas = false;

        public EditorPayloadAction(ActionPayload action)
        {
            ShowPayloads = false;

            for (int i = 0; i < action.PayloadData.Count; i++)
            {
                Add(new EditorPayload(action.PayloadData[i]));
            }
        }

        public static string ActionKey(Action action)
        {
            var key = "";
            if (!string.IsNullOrEmpty(action.SkillID))
            {
                key += action.SkillID;
            }
            key += action.ActionID;

            return key;
        }

        public static void AddPayloadAction(ActionPayload action)
        {
            EditorPayloads[ActionKey(action)] = new EditorPayloadAction(action);
        }

        public static void RemovePayloadAction(Action action)
        {
            EditorPayloads.Remove(ActionKey(action));
        }

        public static EditorPayloadAction GetEditorPayloadAction(Action action)
        {
            var key = ActionKey(action);
            if (EditorPayloads.ContainsKey(key))
            {
                return EditorPayloads[key];
            }
            else
            {
                return null;
            }
        }

        public static EditorPayload GetEditorPayload(Action action, int index)
        {
            var key = ActionKey(action);
            if (EditorPayloads.ContainsKey(key) &&
                EditorPayloads[key].Count > index)
            {
                return EditorPayloads[key][index];
            }
            else
            {
                return null;
            }
        }
    }

    static Dictionary<string, EditorPayloadAction> EditorPayloads = new Dictionary<string, EditorPayloadAction>();

    string NewString = "";
    Action.eActionType NewAction;
    bool[] ShowValues = Enumerable.Repeat(false, 3072).ToArray();
    ActionProjectile.OnCollisionReaction.eReactionType NewReaction;

    ActionCondition.eActionCondition NewActionCondition;

    #endregion

    #region Effect Data
    class EditorStatusGroup
    {
        public string GroupID = "";
        public List<string> Statuses = new List<string>();
        public string NewStatus = "";

        public EditorStatusGroup(string id = "", List<string> list = null)
        {
            GroupID = id;
            if (list != null)
            {
                Statuses = BattleGUI.CopyList(list);
            }
        }
    }

    class EditorStatusEffect
    {
        public StatusEffectData Data = new StatusEffectData();
        public bool Show = false;
        public List<EditorPayload> OnInterval = new List<EditorPayload>();
        public List<EditorPayload> OnCleared = new List<EditorPayload>();
        public List<EditorPayload> OnExpired = new List<EditorPayload>();
        public bool ShowOnInterval = false;
        public bool ShowOnCleared = false;
        public bool ShowOnExpired = false;

        public EditorStatusEffect(StatusEffectData data)
        {
            Data = BattleGUI.Copy(data);

            if (Data.OnInterval == null)
            {
                Data.OnInterval = new List<(PayloadData PayloadData, float Interval)>();
            }
            OnInterval = new List<EditorPayload>();
            for (int i = 0; i < Data.OnInterval.Count; i++)
            {
                OnInterval.Add(new EditorPayload(Data.OnInterval[i].PayloadData));
            }

            if (Data.OnCleared == null)
            {
                Data.OnCleared = new List<PayloadData>();
            }
            OnCleared = new List<EditorPayload>();
            for (int i = 0; i < Data.OnCleared.Count; i++)
            {
                OnCleared.Add(new EditorPayload(Data.OnCleared[i]));
            }

            if (Data.OnExpired == null)
            {
                Data.OnExpired = new List<PayloadData>();
            }
            OnExpired = new List<EditorPayload>();
            for (int i = 0; i < Data.OnExpired.Count; i++)
            {
                OnExpired.Add(new EditorPayload(Data.OnExpired[i]));
            }
        }
    }

    bool ShowStatusGroups = false;
    bool ShowStatusEffects = false;
    bool ShowTriggerConditions = false;

    string NewStatusGroup = "";
    string NewStatusEffect = "";
    string NewCategoryMultiplier = "";
    Effect.eEffectType NewEffect;

    List<EditorStatusGroup> StatusGroups = new List<EditorStatusGroup>();
    List<EditorStatusEffect> StatusEffects = new List<EditorStatusEffect>();
    #endregion

    #region Entity Data
    string NewEntity;
    bool ShowEntities;

    class EditorEntity
    {
        public string EntityID;
        public string NewEntityID;
        public bool ShowEntity;
        public EntityData Data;

        public bool ShowCategories;
        public bool ShowAttributes;
        public bool ShowResources;
        public bool ShowTriggers;
        public bool ShowPhysicalProperties;

        public string NewCategory;
        public string NewAttribute;
        public string NewResource;
        public string NewLifeResource;
        public string NewSkill;

        public bool ShowTargeting;
        public bool ShowSkills;
        public bool ShowMovement;
        public bool ShowStatusEffects;

        public TriggerData.eTrigger NewTrigger;
        public string NewStatusEffect;

        public EditorEntity(string id, EntityData data)
        {
            EntityID = id;
            NewEntityID = id;
            Data = BattleGUI.Copy(data);

            if (Data.Categories == null)
            {
                Data.Categories = new List<string>();
            }

            if (Data.BaseAttributes == null)
            {
                Data.BaseAttributes = new Dictionary<string, Vector2>();
            }

            if (Data.Resources == null)
            {
                Data.Resources = new Dictionary<string, EntityData.EntityResource>();
            }

            if (Data.LifeResources == null)
            {
                Data.LifeResources = new List<string>();
            }

            if (Data.Triggers == null)
            {
                Data.Triggers = new List<TriggerData>();
            }
        }
    }

    List<EditorEntity> Entities = new List<EditorEntity>();

    #endregion
    #endregion

    #region GUI
    [MenuItem("Tools/Battle System Data")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BattleSystemDataEditor));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        BattleGUI.Label("Resources/", 70);
        Path = GUILayout.TextField(Path, GUILayout.Width(300f));
        PlayerPrefs.SetString(PathPlayerPrefs, Path);
        BattleGUI.Label($".json", 30);

        if (GUILayout.Button("Save"))
        {
            Save();
        }

        if (GUILayout.Button("Load"))
        {
            Load();
        }

        GUILayout.EndHorizontal();

        Tab = GUILayout.Toolbar(Tab, new string[] { "Game Data", "Skill Data", "Status Effect Data", "Entity Data" });

        GUILayout.BeginHorizontal();
        BattleGUI.Label("", 5);
        ShowHelp = EditorGUILayout.Toggle(ShowHelp, GUILayout.Width(12));
        BattleGUI.Label("Show Help");
        GUILayout.EndHorizontal();

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

        var notLoaded = Entities.Count != BattleData.Entities.Count;
        if (notLoaded)
        {
            UpdateValues();
        }

        switch (Tab)
        {
            case 0:
            {
                EditGameData();
                break;
            }
            case 1:
            {
                EditSkillData();
                break;
            }
            case 2:
            {
                EditStatusEffectData();
                break;
            }
            case 3:
            {
                EditEntityData();
                break;
            }
        }

        EditorGUILayout.EndScrollView();
    }
    #endregion

    #region Save and Update
    public static void LoadData()
    {
        GUI.FocusControl(null);

        var pathSaved = PlayerPrefs.HasKey(PathPlayerPrefs) || string.IsNullOrEmpty(PlayerPrefs.GetString(PathPlayerPrefs));
        if (!pathSaved)
        {
            PlayerPrefs.SetString(PathPlayerPrefs, Path);
        }
        Path = PlayerPrefs.GetString(PathPlayerPrefs);
        BattleData.LoadData(Path);
    }

    void Load()
    {
        LoadData();
        UpdateValues();
    }

    void Save()
    {
        GUI.FocusControl(null);

        if (!string.IsNullOrEmpty(Path))
        {
            BattleData.SaveData(Path);
        }
        UpdateValues();
    }

    void UpdateValues()
    {
        ShowValues = Enumerable.Repeat(false, 3072).ToArray();

        EntityAttributes = new List<string>();
        foreach (var attribute in BattleData.EntityAttributes)
        {
            EntityAttributes.Add(attribute);
        }

        Multipliers = BattleGUI.Copy(BattleData.Multipliers);

        EntityResources = new List<EditorResource>();
        foreach (var resource in BattleData.EntityResources)
        {
            EntityResources.Add(new EditorResource(resource.Key, resource.Value.Copy()));
        }

        PayloadCategories = new List<string>();
        foreach (var category in BattleData.PayloadCategories)
        {
            PayloadCategories.Add(category);
        }

        EntityCategories = new List<string>();
        foreach (var category in BattleData.EntityCategories)
        {
            EntityCategories.Add(category);
        }

        PayloadFlags = new List<string>();
        foreach (var flag in BattleData.PayloadFlags)
        {
            PayloadFlags.Add(flag);
        }

        Factions = new List<EditorFaction>();
        foreach (var faction in BattleData.Factions)
        {
            Factions.Add(new EditorFaction(faction.Value));
        }

        Aggro = BattleGUI.Copy(BattleData.Aggro);

        SkillGroups = new List<EditorSkillGroup>();
        foreach (var skillGroup in BattleData.SkillGroups)
        {
            SkillGroups.Add(new EditorSkillGroup(skillGroup.Key, skillGroup.Value));
        }

        Skills = new List<EditorSkill>();
        foreach (var skill in BattleData.Skills)
        {
            Skills.Add(new EditorSkill(skill.Value));
        }

        StatusGroups = new List<EditorStatusGroup>();
        foreach (var group in BattleData.StatusEffectGroups)
        {
            StatusGroups.Add(new EditorStatusGroup(group.Key, group.Value));
        }

        StatusEffects = new List<EditorStatusEffect>();
        foreach (var status in BattleData.StatusEffects)
        {
            StatusEffects.Add(new EditorStatusEffect(status.Value));
        }

        Entities = new List<EditorEntity>();
        foreach (var entity in BattleData.Entities)
        {
            Entities.Add(new EditorEntity(entity.Key, entity.Value));
        }
    }
    #endregion

    #region GameData
    void EditGameData()
    {
        EditAttributes();
        BattleGUI.EditorDrawLine();

        EditResources();
        BattleGUI.EditorDrawLine();

        EditCategories();
        BattleGUI.EditorDrawLine();

        EditPayloadFlags();
        BattleGUI.EditorDrawLine();

        EditAggro();
        BattleGUI.EditorDrawLine();
    }

    void EditAttributes()
    {
        if (BattleGUI.EditFoldout(ref ShowAttributes, "Entity Attributes:"))
        {
            if (ShowHelp)
            {
                BattleGUI.Help("Entity Attributes can be used to customise Values. Values apply to a variety of areas in the system, such as damage dealt, " +
                                "buffs granted or the max value of a resource.\n " +
                                "Attributes can also be applied to the functions in Formulae.cs to further customise how damage is calculated and customise" +
                                " other entity properties such as movement speed, jump height, skill cast speed or status effect duration.\n" +
                                "Entity Attributes can be affected by status effects. When using attributes to define values it's possible to use the base value " +
                                "(without attribute changes) or the current value (with attribute changes).");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < EntityAttributes.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EntityAttributes[i] = GUILayout.TextField(EntityAttributes[i], GUILayout.Width(203));
                if (BattleGUI.Rename())
                {
                    BattleData.EntityAttributes[i] = EntityAttributes[i];
                }

                var remove = BattleGUI.Remove();
                GUILayout.EndHorizontal();

                if (remove)
                {
                    EntityAttributes.RemoveAt(i);
                    BattleData.EntityAttributes.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            GUILayout.BeginHorizontal();
            BattleGUI.Label($"New Attribute: ", 90);
            NewAttribute = GUILayout.TextField(NewAttribute, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewAttribute) &&
                !EntityAttributes.Contains(NewAttribute))
            {
                EntityAttributes.Add(NewAttribute);
                BattleData.EntityAttributes.Add(NewAttribute);
                NewAttribute = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();

            EditMultipliers();
        }
    }

    BattleGUI.eReturnResult EditMultiplier(MultiplierAttribute multiplier)
    {
        GUILayout.BeginHorizontal();
        BattleGUI.SelectAttribute(ref multiplier.Attribute, "� Attribute:", 100);
        var remove = BattleGUI.Remove();
        GUILayout.EndHorizontal();

        if (remove)
        {
            return BattleGUI.eReturnResult.Remove;
        }

        BattleGUI.StartIndent();
        var hasChanceAttribute = !string.IsNullOrEmpty(multiplier.ChanceAttribute);
        BattleGUI.EditBool(ref hasChanceAttribute, "Has Chance Attribute");
        if (hasChanceAttribute)
        {
            BattleGUI.SelectAttribute(ref multiplier.ChanceAttribute, "Chance Attribute:", Space, makeHorizontal: true);
        }
        else
        {
            multiplier.ChanceAttribute = "";
        }
        BattleGUI.EndIndent();

        return BattleGUI.eReturnResult.None;
    }

    BattleGUI.eReturnResult EditDamageMultiplier(DamageMultiplierAttribute multiplier)
    {
        var result = EditMultiplier(multiplier);

        if (result != BattleGUI.eReturnResult.Remove)
        {
            BattleGUI.StartIndent();
            if (multiplier.PayloadFlagsRequired == null)
            {
                multiplier.PayloadFlagsRequired = new List<string>();
            }
            BattleGUI.EditListString(ref NewMultiplierPayloadFlag, multiplier.PayloadFlagsRequired, BattleData.PayloadFlags, 
                                     "Required Payload Flags:", "(No Payload Flags)", "Add Flag:");
            BattleGUI.EditString(ref multiplier.SuccessFlag, "Success Flag", Space);
            BattleGUI.EndIndent();
        }
        return result;
    }

    MultiplierAttribute NewMultiplier(string attribute)
    {
        return new MultiplierAttribute(attribute);
    }

    DamageMultiplierAttribute NewDamageMultiplier(string attribute)
    {
        return new DamageMultiplierAttribute(attribute);
    }

    void EditCategoryDamageMultiplierDict(Dictionary<string, List<DamageMultiplierAttribute>> dict, string label, string listLabel, ref bool show, ref string newElement)
    {
        if (BattleGUI.EditFoldout(ref show, label))
        {
            BattleGUI.StartIndent();
            if (BattleData.EntityCategories.Count > 1)
            {
                foreach (var category in BattleData.PayloadCategories)
                {
                    if (!dict.ContainsKey(category))
                    {
                        dict[category] = new List<DamageMultiplierAttribute>();
                    }

                    EditCategoryDamageMultiplierList(dict[category], category + listLabel, ref newElement);
                }
            }
            else
            {
                BattleGUI.Label("(No Categories)");
            }
            BattleGUI.EndIndent();
        }
    }

    void EditCategoryMultiplierList(List<MultiplierAttribute> list, string label, ref string newElement)
    {
        var options = BattleData.EntityAttributes.Where((a) => !list.Exists((m) => m.Attribute.Equals(a))).ToList();
        BattleGUI.EditList(ref newElement, list, options, EditMultiplier, NewMultiplier, label, "(No Attributes)", "Add Attribute:");
    }

    void EditCategoryDamageMultiplierList(List<DamageMultiplierAttribute> list, string label, ref string newElement)
    {
        var options = BattleData.EntityAttributes.Where((a) => !list.Exists((m) => m.Attribute.Equals(a))).ToList();
        BattleGUI.EditList(ref newElement, list, options, EditDamageMultiplier, NewDamageMultiplier, label, "(No Attributes)", "Add Attribute:");
    }

    void EditMultipliers()
    {
        if (BattleGUI.EditFoldout(ref ShowMultipliers, "Multiplier Attributes"))
        {
            BattleGUI.StartIndent();
            if (BattleGUI.SaveChanges())
            {
                BattleData.Instance.MultiplierAttributeData = BattleGUI.Copy(Multipliers);
            }

            if (BattleGUI.EditFoldout(ref ShowDamageDealtMultipliers, "Damage Dealt Multipliers"))
            {
                EditCategoryDamageMultiplierList(Multipliers.DamageDealtMultipliers, label: "", ref NewMultiplierCategory);
            }

            if (BattleGUI.EditFoldout(ref ShowDamageReceivedMultipliers, "Damage Received Multipliers"))
            {
                EditCategoryDamageMultiplierList(Multipliers.DamageReceivedMultipliers, label: "", ref NewMultiplierCategory);
            }

            EditCategoryDamageMultiplierDict(Multipliers.CategoryDamageDealtMultipliers, "Category Damage Dealt Multipliers",
                                             " Multipliers:", ref ShowCategoryDamageMultipliers, ref NewMultiplierCategory);

            EditCategoryDamageMultiplierDict(Multipliers.CategoryDamageReceivedMultipliers, "Category Damage Received Multipliers",
                                             " Multipliers:", ref ShowCategoryDamageReceivedMultipliers, ref NewMultiplierCategory);


            if (BattleGUI.EditFoldout(ref ShowInterruptResistMultipliers, "Interrupt Resistance Multipliers"))
            {
                EditCategoryMultiplierList(Multipliers.InterruptResistanceMultipliers, label: "", ref NewMultiplierCategory);
            }

            if (BattleGUI.EditFoldout(ref ShowMovementMultipliers, "Movement Speed Multipliers"))
            {
                EditCategoryMultiplierList(Multipliers.MovementSpeedMultipliers, label: "", ref NewMultiplierCategory);
            }

            if (BattleGUI.EditFoldout(ref ShowRotationMultipliers, "Rotation Speed Multipliers"))
            {
                EditCategoryMultiplierList(Multipliers.RotationSpeedMultipliers, label: "", ref NewMultiplierCategory);
            }

            if (BattleGUI.EditFoldout(ref ShowJumpMultipliers, "Jump Height Multipliers"))
            {
                EditCategoryMultiplierList(Multipliers.JumpHeightMultipliers, label: "", ref NewMultiplierCategory);
            }
            BattleGUI.EndIndent();
        }
    }

    void EditResources()
    {
        ShowResources = EditorGUILayout.Foldout(ShowResources, "Entity Resources");
        if (ShowResources)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("Resources are properties of an Entity that can change as a result of using Actions. " +
                     "Example resources include HP, MP, Shield and Stamina.\n" +
                     "When defining a resource, its max value has to be specified. The resource will not go above this value.");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < EntityResources.Count; i++)
            {
                BattleGUI.EditorDrawLine();
                GUILayout.BeginHorizontal();
                BattleGUI.Label("Resource: ", 74);
                var oldName = EntityResources[i].Name;
                EntityResources[i].Name = GUILayout.TextField(EntityResources[i].Name, GUILayout.Width(200));
                if (BattleGUI.SaveChanges())
                {
                    GUI.FocusControl(null);
                    var value = EntityResources[i].Value.Copy();

                    if (oldName != EntityResources[i].Name)
                    {
                        if (!BattleData.EntityResources.ContainsKey(EntityResources[i].Name))
                        {
                            BattleData.EntityResources.Remove(oldName);
                            BattleData.EntityResources.Add(EntityResources[i].Name, value);
                        }
                        else
                        {
                            EntityResources[i].Name = oldName;
                            BattleData.EntityResources[EntityResources[i].Name] = value;
                        }
                    }
                    else
                    {
                        BattleData.EntityResources[EntityResources[i].Name] = value;
                    }

                }

                var remove = BattleGUI.Remove();
                GUILayout.EndHorizontal();

                if (remove)
                {
                    EntityResources.RemoveAt(i);
                    BattleData.EntityResources.Remove(EntityResources[i].Name);
                    i--;
                    continue;
                }

                EditValue(EntityResources[i].Value, ValueComponent.eValueType.Resource, $"Max { EntityResources[i].Name.ToUpper()} Value:");
            }

            BattleGUI.EditorDrawLine();

            GUILayout.BeginHorizontal();
            BattleGUI.Label("New Resource: ", 88);
            NewResource.Name = GUILayout.TextField(NewResource.Name, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewResource.Name) &&
                !BattleData.EntityResources.ContainsKey(NewResource.Name))
            {
                EntityResources.Add(new EditorResource(NewResource.Name, NewResource.Value.Copy()));
                BattleData.EntityResources.Add(NewResource.Name, NewResource.Value.Copy());
                NewResource.Name = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    void EditCategories()
    {
        if (BattleGUI.EditFoldout(ref ShowCategories, "Categories:"))
        {
            if (ShowHelp)
            {
                BattleGUI.Help("Categories are custom properties that can describe Entities and Payloads." +
                     "These properties can be used to customise triggers and status effects.\n" +
                     "Example properties include elements such as fire and water, damage types " +
                     "such as piercing and blunt and skill types such as damage, healing and buff.");
            }


            BattleGUI.StartIndent();
            if (BattleGUI.EditFoldout(ref ShowPayloadCategories, "Payload Categories:"))
            {
                BattleGUI.StartIndent();
                for (int i = 0; i < PayloadCategories.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    PayloadCategories[i] = GUILayout.TextField(PayloadCategories[i], GUILayout.Width(203));
                    if (BattleGUI.Rename())
                    {
                        BattleData.PayloadCategories[i] = PayloadCategories[i];
                    }

                    var remove = BattleGUI.Remove();
                    GUILayout.EndHorizontal();

                    if (remove)
                    {
                        PayloadCategories.RemoveAt(i);
                        BattleData.PayloadCategories.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                GUILayout.BeginHorizontal();
                BattleGUI.Label("New Category: ", 100);
                NewPayloadCategory = GUILayout.TextField(NewPayloadCategory, GUILayout.Width(200));
                if (BattleGUI.Add() && !string.IsNullOrEmpty(NewPayloadCategory) &&
                    !BattleData.PayloadCategories.Contains(NewPayloadCategory))
                {
                    PayloadCategories.Add(NewPayloadCategory);
                    BattleData.PayloadCategories.Add(NewPayloadCategory);
                    NewPayloadCategory = "";
                }
                GUILayout.EndHorizontal();
                BattleGUI.EndIndent();
            }

            if (BattleGUI.EditFoldout(ref ShowEntityCategories, "Entity Categories:"))
            {
                BattleGUI.StartIndent();
                for (int i = 0; i < EntityCategories.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    EntityCategories[i] = GUILayout.TextField(EntityCategories[i], GUILayout.Width(203));
                    if (BattleGUI.Rename())
                    {
                        BattleData.EntityCategories[i] = EntityCategories[i];
                    }

                    var remove = BattleGUI.Remove();
                    GUILayout.EndHorizontal();

                    if (remove)
                    {
                        EntityCategories.RemoveAt(i);
                        BattleData.EntityCategories.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
                GUILayout.BeginHorizontal();
                BattleGUI.Label("New Category: ", 100);
                NewEntityCategory = GUILayout.TextField(NewEntityCategory, GUILayout.Width(200));
                if (BattleGUI.Add() && !string.IsNullOrEmpty(NewEntityCategory) &&
                    !BattleData.EntityCategories.Contains(NewEntityCategory))
                {
                    EntityCategories.Add(NewEntityCategory);
                    BattleData.EntityCategories.Add(NewEntityCategory);
                    NewEntityCategory = "";
                }
                GUILayout.EndHorizontal();
                BattleGUI.EndIndent();
            }
            BattleGUI.EndIndent();
        }
    }

    void EditPayloadFlags()
    {
        ShowPayloadFlags = EditorGUILayout.Foldout(ShowPayloadFlags, "Payload Flags");
        if (ShowPayloadFlags)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("Payload flags can be used to customise damage or healing applied through Payloads in Formulae.cs.\n" + 
                     "(A Payload defines all changes applied to an Entity that a Payload Action is used on, such as attribute " + 
                     "changes and status effects.)");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < BattleData.PayloadFlags.Count; i++)
            {
                GUILayout.BeginHorizontal();
                PayloadFlags[i] = GUILayout.TextField(PayloadFlags[i], GUILayout.Width(203));
                if (BattleGUI.Rename())
                {
                    BattleData.PayloadFlags[i] = PayloadFlags[i];
                }

                var remove = BattleGUI.Remove();
                GUILayout.EndHorizontal();

                if (remove)
                {
                    PayloadFlags.RemoveAt(i);
                    BattleData.PayloadFlags.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            GUILayout.BeginHorizontal();
            BattleGUI.Label("New Payload Flag: ", 120);
            NewPayloadFlag = GUILayout.TextField(NewPayloadFlag, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewPayloadFlag) &&
                !BattleData.PayloadFlags.Contains(NewPayloadFlag))
            {
                PayloadFlags.Add(NewPayloadFlag);
                BattleData.PayloadFlags.Add(NewPayloadFlag);
                NewPayloadFlag = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    void EditAggro()
    {
        if (BattleGUI.EditFoldout(ref ShowAggro, "Aggro Data:"))
        {
            if (ShowHelp)
            {
                BattleGUI.Help("Entities can select their targets based on distance, angle, resources or generated aggro. " +
                     "A constraint to the aggro value can be defined here, as well as the change (for example drain) to all aggro values per second." +
                     "The change can be multiplied by one of an entity's attributes to change the speed at which it increases or decreases for different entities.");
            }

            BattleGUI.StartIndent();
            if (BattleGUI.SaveChanges())
            {
                BattleData.Instance.AggroData = BattleGUI.Copy(Aggro);
            }
            BattleGUI.EditFloat(ref Aggro.MaxAggro, "Max Aggro:", Space);
            EditAggroChange(Aggro.AggroChangePerSecond, "Aggro Change per second:");
            BattleGUI.EndIndent();
        }
    }

    void EditAggroChange(AggroData.AggroChange change, string label)
    {
        BattleGUI.Label(label);

        BattleGUI.StartIndent();
        EditValue(change.Change, ValueComponent.eValueType.Aggro, "Aggro Value:");
        BattleGUI.EditEnum(ref change.ChangeMultiplier, "Aggro multiplier:", Space);
        var attributeMult = !string.IsNullOrEmpty(change.MultiplierAttribute);
        BattleGUI.EditBool(ref attributeMult, "Multiply by Attribute");
        if (attributeMult)
        {
            BattleGUI.StartIndent();
            BattleGUI.SelectAttribute(ref change.MultiplierAttribute, "Attribute multiplier:", Space, makeHorizontal: true);
            BattleGUI.EndIndent();
        }
        else
        {
            change.MultiplierAttribute = "";
        }
        BattleGUI.EndIndent();
    }
    #endregion

    #region Skill Data
    void EditSkillData()
    {
        EditSkillGroups();
        BattleGUI.EditorDrawLine();

        EditSkills();
        BattleGUI.EditorDrawLine();
    }

    void EditSkillGroups()
    {
        ShowSkillGroups = EditorGUILayout.Foldout(ShowSkillGroups, "Skill Groups");
        if (ShowSkillGroups)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < BattleData.SkillGroups.Count; i++)
            {
                BattleGUI.EditorDrawLine();
                GUILayout.BeginHorizontal();
                BattleGUI.Label("Skill Group: ", 80);

                var oldName = SkillGroups[i].GroupID;
                SkillGroups[i].GroupID = GUILayout.TextField(SkillGroups[i].GroupID, GUILayout.Width(200));
                if (BattleGUI.SaveChanges())
                {
                    GUI.FocusControl(null);
                    var value = BattleGUI.CopyList(SkillGroups[i].Skills);

                    if (oldName != SkillGroups[i].GroupID)
                    {
                        if (!BattleData.SkillGroups.ContainsKey(SkillGroups[i].GroupID))
                        {
                            BattleData.SkillGroups.Remove(oldName);
                        }
                        else
                        {
                            SkillGroups[i].GroupID = oldName;
                        }
                    }
                    BattleData.SkillGroups[SkillGroups[i].GroupID] = value;
                }

                if (BattleGUI.Remove())
                {
                    SkillGroups.RemoveAt(i);
                    BattleData.SkillGroups.Remove(SkillGroups[i].GroupID);
                }
                GUILayout.EndHorizontal();

                BattleGUI.EditListString(ref SkillGroups[i].NewSkill, SkillGroups[i].Skills, BattleData.Skills.Keys.ToList(),
                               "", "(No Skills in the Skill Group)", "Add Skill:");
            }

            // New skill group
            GUILayout.BeginHorizontal();
            BattleGUI.Label("New Skill Group: ", 128);
            NewSkillGroup = GUILayout.TextField(NewSkillGroup, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewSkillGroup) &&
                !BattleData.SkillGroups.ContainsKey(NewSkillGroup))
            {
                SkillGroups.Add(new EditorSkillGroup(NewSkillGroup));
                BattleData.SkillGroups.Add(NewSkillGroup, new List<string>());
                NewSkillGroup = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    void EditSkills()
    {
        ShowSkillData = EditorGUILayout.Foldout(ShowSkillData, "Skills");
        if (ShowSkillData)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < Skills.Count; i++)
            {
                GUILayout.BeginHorizontal();
                BattleGUI.Label("", 10);
                var skill = Skills[i];
                skill.ShowSkill = EditorGUILayout.Foldout(skill.ShowSkill, skill.SkillData.SkillID);
                GUILayout.EndHorizontal();

                if (skill.ShowSkill)
                {
                    BattleGUI.StartIndent();
                    BattleGUI.EditorDrawLine();
                    GUILayout.BeginHorizontal();

                    // ID
                    BattleGUI.Label("Skill ID: ", 70);
                    skill.SkillID = GUILayout.TextField(skill.SkillID, GUILayout.Width(200));

                    // Save/Remove
                    if (BattleGUI.SaveChanges())
                    {
                        GUI.FocusControl(null);

                        if (!skill.HasChargeTime)
                        {
                            skill.SkillData.SkillChargeData = null;
                        }
                        else
                        {
                            skill.SkillData.SkillChargeData.PreChargeTimeline.Sort((a1, a2) => a1.Timestamp.CompareTo(a2.Timestamp));
                        }

                        skill.SkillData.SkillTimeline.Sort((a1, a2) => a1.Timestamp.CompareTo(a2.Timestamp));

                        if (skill.SkillData.SkillID != skill.SkillID)
                        {
                            if (!BattleData.Skills.ContainsKey(skill.SkillID))
                            {
                                BattleData.Skills.Remove(skill.SkillData.SkillID);
                                skill.SkillData.SkillID = skill.SkillID;
                            }
                            else
                            {
                                skill.SkillID = skill.SkillData.SkillID;
                            }
                        }

                        var value = BattleGUI.Copy(skill.SkillData);
                        BattleData.Skills[skill.SkillData.SkillID] = value;
                    }

                    if (BattleGUI.Remove())
                    {
                        Skills.RemoveAt(i);
                        BattleData.Skills.Remove(skill.SkillData.SkillID);
                    }
                    GUILayout.EndHorizontal();

                    // Values
                    BattleGUI.EditBool(ref skill.SkillData.Interruptible, "Is Interruptible");

                    if (BattleGUI.EditFoldout(ref skill.ShowCharge, "Charge Time"))
                    {
                        BattleGUI.EditBool(ref skill.HasChargeTime, "Has Charge Time");
                        if (skill.HasChargeTime)
                        {
                            BattleGUI.StartIndent();
                            if (skill.SkillData.SkillChargeData == null)
                            {
                                skill.SkillData.SkillChargeData = new SkillChargeData();
                            }
                            EditSkillChargeData(skill.SkillData.SkillChargeData, ref skill.NewChargeTimelineAction,
                                                ref skill.ShowChargeTimeline, skill.SkillID);
                            BattleGUI.EndIndent();
                        }
                    }

                    // Actions
                    EditActionTimeline(skill.SkillData.SkillTimeline, ref skill.NewTimelineAction,
                                       ref skill.ShowTimeline, "Skill Timeline:", skillID: skill.SkillID, showIndex: 2500);

                    // Target
                    if (BattleGUI.EditFoldout(ref skill.ShowTarget, "Skill Target"))
                    {
                        BattleGUI.StartIndent();
                        BattleGUI.EditBool(ref skill.SkillData.NeedsTarget, "Target Required");
                        BattleGUI.EditEnum(ref skill.SkillData.PreferredTarget, $"Preferred Target: ", Space);
                        BattleGUI.EditEnum(ref skill.SkillData.PreferredTargetState, $"Preferred Target State: ", Space);
                        BattleGUI.EditFloat(ref skill.SkillData.Range, "Skill Range:", Space, 150);
                        BattleGUI.EditFloatSlider(ref skill.SkillData.MaxAngleFromTarget, "Max Angle from Target:", 0.0f, 180.0f, Space, 150);
                        if (skill.SkillData.NeedsTarget)
                        {
                            BattleGUI.EditBool(ref skill.SkillData.RequireLineOfSight, "Line of Sight Required");
                        }
                        BattleGUI.EndIndent();
                    }
                    BattleGUI.EditorDrawLine();
                    BattleGUI.EndIndent();
                }
            }

            // New skill
            GUILayout.BeginHorizontal();
            BattleGUI.EditString(ref NewSkill, "New Skill: ", 80, 200, false);
            if (BattleGUI.Add() && !BattleData.Skills.ContainsKey(NewSkill))
            {
                var newSkill = new SkillData(NewSkill);
                Skills.Add(new EditorSkill(newSkill));
                BattleData.Skills.Add(NewSkill, newSkill);

                NewSkill = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }

    }
    #endregion

    #region Status Effect Data
    void EditStatusEffectData()
    {
        // Status Effect Groups
        EditStatusEffectGroups();

        // Status Effects
        EditStatusEffects();
    }

    void EditStatusEffectGroups()
    {
        ShowStatusGroups = EditorGUILayout.Foldout(ShowStatusGroups, "Status Effect Groups");
        if (ShowStatusGroups)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < BattleData.StatusEffectGroups.Count; i++)
            {
                BattleGUI.EditorDrawLine();
                GUILayout.BeginHorizontal();
                BattleGUI.Label("Status Effect Group: ", 120);

                var oldName = StatusGroups[i].GroupID;
                StatusGroups[i].GroupID = GUILayout.TextField(StatusGroups[i].GroupID, GUILayout.Width(200));
                if (BattleGUI.SaveChanges())
                {
                    GUI.FocusControl(null);
                    var value = BattleGUI.CopyList(StatusGroups[i].Statuses);

                    if (oldName != StatusGroups[i].GroupID)
                    {
                        if (!BattleData.StatusEffectGroups.ContainsKey(StatusGroups[i].GroupID))
                        {
                            BattleData.StatusEffectGroups.Remove(oldName);
                        }
                        else
                        {
                            StatusGroups[i].GroupID = oldName;
                        }
                    }
                    BattleData.StatusEffectGroups[StatusGroups[i].GroupID] = value;
                }

                if (BattleGUI.Remove())
                {
                    StatusGroups.RemoveAt(i);
                    BattleData.StatusEffectGroups.Remove(StatusGroups[i].GroupID);
                }
                GUILayout.EndHorizontal();

                BattleGUI.EditListString(ref StatusGroups[i].NewStatus, StatusGroups[i].Statuses, BattleData.StatusEffects.Keys.ToList(),
                               "", "(No Status Effects in the Status Effect Group)", "Add Status Effect:");
            }

            // New status effect group
            GUILayout.BeginHorizontal();
            BattleGUI.Label("New Status Effect Group: ", 165);
            NewStatusGroup = GUILayout.TextField(NewStatusGroup, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewStatusGroup) &&
                !BattleData.StatusEffectGroups.ContainsKey(NewStatusGroup))
            {
                StatusGroups.Add(new EditorStatusGroup(NewStatusGroup));
                BattleData.StatusEffectGroups.Add(NewStatusGroup, new List<string>());
                NewStatusGroup = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    void EditStatusEffects()
    {
        ShowStatusEffects = EditorGUILayout.Foldout(ShowStatusEffects, "Status Effects");
        if (ShowStatusEffects)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < StatusEffects.Count; i++)
            {
                var status = StatusEffects[i];
                if (BattleGUI.EditFoldout(ref status.Show, status.Data.StatusID))
                {
                    BattleGUI.EditorDrawLine();
                    GUILayout.BeginHorizontal();

                    // ID
                    BattleGUI.Label("Status ID: ", 70);
                    var newID = status.Data.StatusID;
                    newID = GUILayout.TextField(newID, GUILayout.Width(200));

                    // Save/Remove
                    if (BattleGUI.SaveChanges())
                    {
                        GUI.FocusControl(null);

                        var value = BattleGUI.Copy(status.Data);

                        if (status.Data.StatusID != newID)
                        {
                            if (!BattleData.StatusEffects.ContainsKey(newID))
                            {
                                BattleData.StatusEffects.Remove(status.Data.StatusID);
                                status.Data.StatusID = newID;
                            }
                        }
                        BattleData.StatusEffects[status.Data.StatusID] = value;
                    }

                    if (BattleGUI.Remove())
                    {
                        StatusEffects.RemoveAt(i);
                        BattleData.StatusEffects.Remove(status.Data.StatusID);
                        i--;
                        continue;
                    }
                    GUILayout.EndHorizontal();

                    BattleGUI.StartIndent();
                    BattleGUI.EditInt(ref status.Data.MaxStacks, "Max Stacks:", Space);
                    BattleGUI.EditFloat(ref status.Data.Duration, "Duration:", Space);
                    BattleGUI.EditBool(ref status.Data.StackGainResetsDuration, "Stack Gain resets Duration");

                    EditEffects(status);

                    EditPayloadFloatList(status.Data.OnInterval, ref status.ShowOnInterval, status.OnInterval, "On Interval:", "Interval:");
                    EditPayloadList(status.Data.OnCleared, ref status.ShowOnCleared, status.OnCleared, "On Cleared:");
                    EditPayloadList(status.Data.OnExpired, ref status.ShowOnExpired, status.OnExpired, "On Expired:");

                    BattleGUI.EditorDrawLine();
                    BattleGUI.EndIndent();
                }
            }

            // New status effect
            GUILayout.BeginHorizontal();
            BattleGUI.EditString(ref NewStatusEffect, "New Status Effect: ", Space, 200, makeHorizontal: false);
            if (BattleGUI.Add() && !BattleData.StatusEffects.ContainsKey(NewStatusEffect))
            {
                var newStatus = new StatusEffectData(NewStatusEffect);
                StatusEffects.Add(new EditorStatusEffect(newStatus));
                BattleData.StatusEffects.Add(NewStatusEffect, newStatus);

                NewStatusEffect = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    Dictionary<Effect.eEffectType, string> EffectHelp = new Dictionary<Effect.eEffectType, string>()
    {
        [Effect.eEffectType.AttributeChange] = "",
        [Effect.eEffectType.Convert]         = "",
        [Effect.eEffectType.Immunity]        = "",
        [Effect.eEffectType.Lock]            = "",
        [Effect.eEffectType.ResourceGuard]   = "",
        [Effect.eEffectType.Shield]          = "",
        [Effect.eEffectType.Trigger]         = "",
    };

    void EditEffects(EditorStatusEffect status)
    {
        if (status.Data.Effects.Count == 0)
        {
            BattleGUI.Label("(No Effects.)");
        }

        for (int i = 0; i < status.Data.Effects.Count; i++)
        {
            var remove = !EditEffect(status.Data.Effects[i]);

            if (remove)
            {
                status.Data.Effects.RemoveAt(i);
                i--;
                continue;
            }
        }

        GUILayout.BeginHorizontal();
        BattleGUI.EditEnum(ref NewEffect, "New Effect:", Space, makeHorizontal: false);
        if (BattleGUI.Add())
        {
            var newEffect = Effect.MakeNew(NewEffect);
            status.Data.Effects.Add(newEffect);
        }
        GUILayout.EndHorizontal();
       BattleGUI.EditorDrawLine();;
    }

    bool EditEffect(Effect effect)
    {
        BattleGUI.EditorDrawLine();

        GUILayout.BeginHorizontal();
        BattleGUI.Label($"{effect.EffectType} Effect", Space);
        var remove = BattleGUI.Remove();
        GUILayout.EndHorizontal();

        if (remove)
        {
            return false;
        }

        if (ShowHelp && EffectHelp.ContainsKey(effect.EffectType))
        {
            BattleGUI.Help(EffectHelp[effect.EffectType]);
        }

        BattleGUI.StartIndent();

        BattleGUI.EditInt(ref effect.StacksRequiredMin, "Min Stacks Required:", Space);
        if (effect.StacksRequiredMin < 1)
        {
            effect.StacksRequiredMin = 1;
        }


        BattleGUI.EditInt(ref effect.StacksRequiredMax, "Max Stacks Required:", Space);
        if (effect.StacksRequiredMax < effect.StacksRequiredMin)
        {
            effect.StacksRequiredMax = effect.StacksRequiredMin;
        }

        switch (effect.EffectType)
        {
            case Effect.eEffectType.AttributeChange:
            {
                if (!(effect is EffectAttributeChange e))
                {
                    return false;
                }

                BattleGUI.SelectAttribute(ref e.Attribute);

                EditValue(e.Value, ValueComponent.eValueType.NonAction, "Change:");

                var maxValue = e.MaxValue != null;
                BattleGUI.EditBool(ref maxValue, "Limit Value");
                if (maxValue)
                {
                    if (e.MaxValue == null)
                    {
                        e.MaxValue = new Value();
                    }

                    EditValue(e.MaxValue, ValueComponent.eValueType.NonAction, "Max Change:");
                }
                else
                {
                    e.MaxValue = null;
                }

                GUILayout.BeginHorizontal();
                BattleGUI.EditEnum(ref e.PayloadTargetType, "Change Affects:", Space, 90, makeHorizontal: false);

                switch (e.PayloadTargetType)
                {
                    case Effect.ePayloadFilter.Action:
                    {
                        BattleGUI.EditString(ref e.PayloadTarget, "Action ID:", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Category:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadTarget, BattleData.PayloadCategories.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Skill:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadTarget, BattleData.Skills.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.SkillGroup:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadTarget, BattleData.SkillGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Status:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadTarget, BattleData.StatusEffects.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.StatusGroup:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadTarget, BattleData.StatusEffectGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                }
                GUILayout.EndHorizontal();
                break;
            }
            case Effect.eEffectType.Convert:
            {
                if (!(effect is EffectConvert e))
                {
                    return false;
                }

                break;
            }
            case Effect.eEffectType.Immunity:
            {
                if (!(effect is EffectImmunity e))
                {
                    return false;
                }

                GUILayout.BeginHorizontal();
                BattleGUI.EditEnum(ref e.PayloadFilter, "Immunity To:", Space, 90, makeHorizontal: false);

                switch (e.PayloadFilter)
                {
                    case Effect.ePayloadFilter.Action:
                    {
                        BattleGUI.EditString(ref e.PayloadName, "Action ID:", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Category:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadName, BattleData.PayloadCategories.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Skill:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadName, BattleData.Skills.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.SkillGroup:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadName, BattleData.SkillGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Status:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadName, BattleData.StatusEffects.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.StatusGroup:
                    {
                        BattleGUI.SelectStringFromList(ref e.PayloadName, BattleData.StatusEffectGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                }
                GUILayout.EndHorizontal();

                BattleGUI.EditInt(ref e.Limit, "Max Hits Resisted:", Space);
                BattleGUI.EditBool(ref e.EndStatusOnEffectEnd, "End Status when Limit Reached");
                break;
            }
            case Effect.eEffectType.Lock:
            {
                if (!(effect is EffectLock e))
                {
                    return false;
                }

                GUILayout.BeginHorizontal();
                BattleGUI.EditEnum(ref e.LockType, "Lock Type:", Space, 90, makeHorizontal: false);
                if (e.LockType == EffectLock.eLockType.Skill)
                {
                    BattleGUI.SelectStringFromList(ref e.Skill, BattleData.Skills.Keys.ToList(),
                                         "", makeHorizontal: false);
                }
                else if (e.LockType == EffectLock.eLockType.SkillsGroup)
                {
                    BattleGUI.SelectStringFromList(ref e.Skill, BattleData.SkillGroups.Keys.ToList(),
                     "", makeHorizontal: false);
                }

                GUILayout.EndHorizontal();
                break;
            }
            case Effect.eEffectType.ResourceGuard:
            {
                if (!(effect is EffectResourceGuard e))
                {
                    return false;
                }

                BattleGUI.SelectResource(ref e.Resource, "Guarded Resource:", Space);
                var hasMin = e.MinValue != null;
                BattleGUI.EditBool(ref hasMin, "Min Value");
                if (hasMin)
                {
                    if (e.MinValue == null)
                    {
                        e.MinValue = new Value();
                        e.MinValue.Add(new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 1.0f));
                    }
                    EditValue(e.MinValue, ValueComponent.eValueType.NonAction);
                }
                else
                {
                    e.MinValue = null;
                }

                BattleGUI.SelectResource(ref e.Resource, "Guarded Resource:", Space);
                var hasMax = e.MaxValue != null;
                BattleGUI.EditBool(ref hasMax, "Max Value");
                if (hasMax)
                {
                    if (e.MaxValue == null)
                    {
                        e.MaxValue = new Value();
                        e.MaxValue.Add(new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, 1.0f));
                    }
                    EditValue(e.MaxValue, ValueComponent.eValueType.NonAction);
                }
                else
                {
                    e.MaxValue = null;
                }

                BattleGUI.EditInt(ref e.Limit, "Max Hits Guarded:", Space);
                BattleGUI.EditBool(ref e.EndStatusOnEffectEnd, "End Status when Limit Reached");

                break;
            }
            case Effect.eEffectType.Shield:
            {
                if (!(effect is EffectShield e))
                {
                    return false;
                }

                BattleGUI.SelectResource(ref e.ShieldResource, "Shielding Resource:", Space);
                BattleGUI.SelectResource(ref e.ShieldedResource, "Shielded Resource:", Space);

                EditValue(e.ShieldResourceToGrant, ValueComponent.eValueType.NonAction, "Shield Resource Granted:");
                var limitAbsorption = e.MaxDamageAbsorbed != null;
                BattleGUI.EditBool(ref limitAbsorption, "Limit Absorbtion");
                if (limitAbsorption)
                {
                    if (e.MaxDamageAbsorbed == null)
                    {
                        e.MaxDamageAbsorbed = new Value();
                    }
                    EditValue(e.MaxDamageAbsorbed, ValueComponent.eValueType.NonAction, "Max Damage Absorbed:");
                }
                else
                {
                    e.MaxDamageAbsorbed = null;
                }
                BattleGUI.EditBool(ref e.SetMaxShieldResource, "Set Granted Resource as Max (shield-exclusive resources)");

                BattleGUI.EditFloat(ref e.DamageMultiplier, "Damage Absorption Multiplier", Space + 50);
                if (e.CategoryMultipliers == null)
                {
                    e.CategoryMultipliers = new Dictionary<string, float>();
                }
                BattleGUI.EditFloatDict(e.CategoryMultipliers, "Category-Specific Damage Absorption Multiplier:", BattleData.PayloadCategories,
                              ref NewCategoryMultiplier, "", Space, "New Category Multiplier:", Space);

                BattleGUI.EditInt(ref e.Priority, "Shield Priority:", Space);

                BattleGUI.EditInt(ref e.Limit, "Max Hits Absorbed:", Space);
                BattleGUI.EditBool(ref e.EndStatusOnEffectEnd, "End Status when Limit Reached");
                break;
            }
            case Effect.eEffectType.Trigger:
            {
                if (!(effect is EffectTrigger e))
                {
                    return false;
                }

                EditTrigger(e.TriggerData, "Trigger:");
                break;
            }
        }
        BattleGUI.EndIndent();
        return true;
    }
    #endregion

    #region Entity Data
    void EditEntityData()
    {
        EditFactions();
        BattleGUI.EditorDrawLine();

        EditEntities();
        BattleGUI.EditorDrawLine();
    }

    void EditEntities()
    {
        ShowEntities = EditorGUILayout.Foldout(ShowEntities, "Entities");
        if (ShowEntities)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < BattleData.Entities.Count; i++)
            {
                var entity = Entities[i];

                entity.ShowEntity = EditorGUILayout.Foldout(entity.ShowEntity, entity.EntityID);
                if (entity.ShowEntity)
                {
                    EditorGUILayout.BeginHorizontal();
                    BattleGUI.EditString(ref entity.NewEntityID, "Entity ID:", 60, makeHorizontal: false);

                    if (BattleGUI.Button("Save Changes", 100))
                    {
                        var value = BattleGUI.Copy(entity.Data);

                        if (entity.NewEntityID != entity.EntityID && !BattleData.Entities.ContainsKey(entity.NewEntityID))
                        {
                            BattleData.Entities.Remove(entity.EntityID);
                            entity.EntityID = entity.NewEntityID;
                        }
                        else
                        {
                            entity.NewEntityID = entity.EntityID;
                        }

                        BattleData.Entities[entity.EntityID] = value;
                    }

                    if (BattleGUI.Copy())
                    {
                        var value = BattleGUI.Copy(entity.Data);
                        var copyNo = 2;
                        var id = Entities[i].EntityID + copyNo;
                        while(BattleData.Entities.ContainsKey(id))
                        {
                            copyNo++;
                            id = Entities[i].EntityID + copyNo;
                        }

                        BattleData.Entities.Add(id, value);
                        Entities.Add(new EditorEntity(id, value));
                    }

                    var remove = BattleGUI.Remove();
                    EditorGUILayout.EndHorizontal();

                    if (remove)
                    {
                        BattleData.Entities.Remove(Entities[i].EntityID);
                        Entities.RemoveAt(i);
                        i--;
                        continue;
                    }

                    BattleGUI.StartIndent();
                    EditEntity(entity);
                    BattleGUI.EndIndent();
                }
            }

            GUILayout.BeginHorizontal();
            BattleGUI.Label("New Entity: ", 80);
            NewEntity = GUILayout.TextField(NewEntity, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewEntity) &&
                !BattleData.Entities.ContainsKey(NewEntity))
            {
                var newEntityData = new EntityData();
                Entities.Add(new EditorEntity(NewEntity, newEntityData));
                BattleData.Entities.Add(NewEntity, newEntityData);
                NewEntity = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    void EditEntity(EditorEntity entity)
    {
        BattleGUI.EditEnum(ref entity.Data.EntityType, "Entity Type:", Space);

        BattleGUI.SelectFaction(ref entity.Data.Faction, "Faction:", Space);
        BattleGUI.EditBool(ref entity.Data.IsTargetable, "Targetable");
        BattleGUI.EditBool(ref entity.Data.CanEngage, "Engage on Attack");

        // Skills/battle
        if (BattleGUI.EditFoldout(ref entity.ShowSkills, "Skills"))
        {
            BattleGUI.StartIndent();
            BattleGUI.EditEnum(ref entity.Data.Skills.SkillMode, "Skill Use Mode:", Space);

            BattleGUI.EditFloat(ref entity.Data.Skills.SkillDelayMin, "Skill Delay Min:", Space);
            BattleGUI.EditFloat(ref entity.Data.Skills.SkillDelayMax, "Skill Delay Max:", Space);

            if (entity.Data.Skills.SkillDelayMin < 0.0f)
            {
                entity.Data.Skills.SkillDelayMin = 0.0f;
            }

            if (entity.Data.Skills.SkillDelayMax < entity.Data.Skills.SkillDelayMin)
            {
                entity.Data.Skills.SkillDelayMax = entity.Data.Skills.SkillDelayMin;
            }

            BattleGUI.EditBool(ref entity.Data.Skills.MoveToTargetIfNotInRange, "Move to Target if out of Skill Range");
            BattleGUI.EditBool(ref entity.Data.Skills.RotateToTargetIfNotWithinAngle, "Rotate to Target if not within Skill Angle");
            BattleGUI.EditBool(ref entity.Data.Skills.AutoSelectTargetOnSkillUse, "Automatically select Target on Skill Use");

            if (entity.Data.Skills.SkillMode == EntitySkillsData.eSkillMode.AutoRandom ||
                entity.Data.Skills.SkillMode == EntitySkillsData.eSkillMode.AutoSequence)
            {
                BattleGUI.EditBool(ref entity.Data.Skills.EngageOnSight, "Engage On Sight");
                if (entity.Data.Skills.EngageOnSight)
                {
                    BattleGUI.EditBool(ref entity.Data.Skills.CheckLineOfSight, "Check Line of Sight");
                }

                if (entity.Data.Skills.SkillMode == EntitySkillsData.eSkillMode.AutoRandom)
                {
                    BattleGUI.EditList(ref entity.NewSkill, entity.Data.Skills.RandomSkills, BattleData.Skills.Keys.ToList(),
                             EditRandomSkill, NewRandomSkill, "Enity Skills:", "(No Skills)", "Add Skill:");
                }
                else if (entity.Data.Skills.SkillMode == EntitySkillsData.eSkillMode.AutoSequence)
                {
                    BattleGUI.EditList(ref entity.NewSkill, entity.Data.Skills.SequenceSkills, BattleData.Skills.Keys.ToList(),
                             EditSequenceSkill, NewSequenceSkill, "Enity Skills:", "(No Skills)", "Add Skill:");
                }
            }
            else if (entity.Data.Skills.SkillMode == EntitySkillsData.eSkillMode.Input)
            {
                BattleGUI.EditList(ref entity.NewSkill, entity.Data.Skills.InputSkills, BattleData.Skills.Keys.ToList(),
                        EditInputSkill, NewInputSkill, "Enity Skills:", "(No Skills)", "Add Skill:");
            }

            var hasAutoAttack = entity.Data.Skills.AutoAttack != null;
            BattleGUI.EditBool(ref hasAutoAttack, "Auto Attack");
            if (hasAutoAttack)
            {
                if (entity.Data.Skills.AutoAttack == null)
                {
                    entity.Data.Skills.AutoAttack = new ActionTimeline();
                }
                BattleGUI.StartIndent();
                BattleGUI.EditFloat(ref entity.Data.Skills.AutoAttackInterval, "Auto Attack Inteval:", Space);
                BattleGUI.EditBool(ref entity.Data.Skills.AutoAttackRequiredTarget, "Auto Attack requires Enemy Target");
                if (entity.Data.Skills.AutoAttackRequiredTarget)
                {
                    BattleGUI.StartIndent();
                    BattleGUI.EditFloat(ref entity.Data.Skills.AutoAttackRange, "Auto Attack Range:", Space);
                    BattleGUI.EditBool(ref entity.Data.Skills.AutoAttackRequiresLineOfSight, "Auto Attack requires Line of Sight");
                    BattleGUI.EndIndent();
                }
                EditActionTimeline(entity.Data.Skills.AutoAttack, ref NewAction, ref ShowValues[0], "Auto Attack Timeline", showIndex: 2000);
                BattleGUI.EndIndent();
            }
            else
            {
                entity.Data.Skills.AutoAttack = null;
            }
            BattleGUI.EndIndent();
        }

        // Targeting
        if (BattleGUI.EditFoldout(ref entity.ShowTargeting, "Targeting"))
        {
            BattleGUI.StartIndent();
            BattleGUI.EditEnum(ref entity.Data.Targeting.EnemyTargetPriority.TargetPriority, "Enemy Target Priority:", Space);
            BattleGUI.StartIndent();
            if (entity.Data.Targeting.EnemyTargetPriority.TargetPriority == EntityTargetingData.TargetingPriority.eTargetPriority.ValueLowest ||
                entity.Data.Targeting.EnemyTargetPriority.TargetPriority == EntityTargetingData.TargetingPriority.eTargetPriority.ValueHighest)
            {
                EditValueComponent(entity.Data.Targeting.EnemyTargetPriority.Value, ValueComponent.eValueType.TargetingPriority, false);
            }

            BattleGUI.EditFloat(ref entity.Data.Targeting.EnemyTargetPriority.PreferredDistanceMin, "Preferred Distance Min:", Space);
            BattleGUI.EditFloat(ref entity.Data.Targeting.EnemyTargetPriority.PreferredDistanceMax, "Preferred Distance Max:", Space);
            BattleGUI.EditBool(ref entity.Data.Targeting.EnemyTargetPriority.PreferredInFront, "Preferred In Front");
            BattleGUI.EndIndent();

            BattleGUI.EditEnum(ref entity.Data.Targeting.FriendlyTargetPriority.TargetPriority, "Friendly Target Priority:", Space);
            BattleGUI.StartIndent();
            if (entity.Data.Targeting.FriendlyTargetPriority.TargetPriority == EntityTargetingData.TargetingPriority.eTargetPriority.ValueLowest ||
                entity.Data.Targeting.FriendlyTargetPriority.TargetPriority == EntityTargetingData.TargetingPriority.eTargetPriority.ValueHighest)
            {
                EditValueComponent(entity.Data.Targeting.FriendlyTargetPriority.Value, ValueComponent.eValueType.TargetingPriority, false);
            }

            BattleGUI.EditFloat(ref entity.Data.Targeting.FriendlyTargetPriority.PreferredDistanceMin, "Preferred Distance Min:", Space);
            BattleGUI.EditFloat(ref entity.Data.Targeting.FriendlyTargetPriority.PreferredDistanceMax, "Preferred Distance Max:", Space);
            BattleGUI.EditBool(ref entity.Data.Targeting.FriendlyTargetPriority.PreferredInFront, "Preferred In Front");
            BattleGUI.EndIndent();

            BattleGUI.EditFloat(ref entity.Data.Targeting.DetectDistance, "Detection Distance:", Space);
            BattleGUI.EditFloatSlider(ref entity.Data.Targeting.DetectFieldOfView, "Detection Field Of View:", 0.0f, 360.0f, Space);

            BattleGUI.EditFloat(ref entity.Data.Targeting.DisengageDistance, "Disengage Distance:", Space);
            BattleGUI.EndIndent();
        }   

        // Movement
        if (BattleGUI.EditFoldout(ref entity.ShowMovement, "Movement"))
        {
            BattleGUI.StartIndent();
            BattleGUI.EditFloat(ref entity.Data.InterruptResistance, "Interruption Resistance:", Space);
            BattleGUI.EditFloat(ref entity.Data.Movement.MovementSpeed, "Movement Speed:", Space);
            BattleGUI.EditFloat(ref entity.Data.Movement.MovementSpeedRunMultiplier, "Running Speed Multiplier:", Space);
            BattleGUI.EditBool(ref entity.Data.Movement.ConsumeResourceWhenRunning, "Consume Resource When Running");
            if (entity.Data.Movement.ConsumeResourceWhenRunning)
            {
                BattleGUI.StartIndent();
                BattleGUI.SelectResource(ref entity.Data.Movement.RunResource, "Resource:", Space);
                if (entity.Data.Movement.RunResourcePerSecond == null)
                {
                    entity.Data.Movement.RunResourcePerSecond = new Value();
                }
                EditValue(entity.Data.Movement.RunResourcePerSecond, ValueComponent.eValueType.CasterOnly, $"{entity.Data.Movement.RunResource} Drained per Second:");
                BattleGUI.EndIndent();
            }
            BattleGUI.EditFloat(ref entity.Data.Movement.RotateSpeed, "Rotate Speed:", Space);
            BattleGUI.EditFloat(ref entity.Data.Movement.JumpHeight, "Jump Height:", Space);
            BattleGUI.EndIndent();
        }

        // Properties
        if (BattleGUI.EditFoldout(ref entity.ShowPhysicalProperties, "Physical Properties"))
        {
            BattleGUI.StartIndent();
            BattleGUI.EditFloat(ref entity.Data.Radius, "Radius:", Space);
            BattleGUI.EditFloat(ref entity.Data.Height, "Height:", Space);
            BattleGUI.EditFloat(ref entity.Data.OriginHeight, "Origin Height:", Space);
            BattleGUI.EndIndent();
        }

        // Categories
        if (BattleGUI.EditFoldout(ref entity.ShowCategories, "Entity Categories"))
        {
            BattleGUI.EditListString(ref entity.NewCategory, entity.Data.Categories, BattleData.EntityCategories,
                           "", "(No Categories)", "Add Category:");
        }

        EditAttributeDict(ref entity.NewAttribute, entity.Data.BaseAttributes, ref entity.ShowAttributes, "Base Attributes");

        // Resources
        if (BattleGUI.EditFoldout(ref entity.ShowResources, "Entity Resources"))
        {
            BattleGUI.StartIndent();
            var resourceList = entity.Data.Resources.Values.ToList();
            BattleGUI.EditList(ref entity.NewResource, resourceList, BattleData.EntityResources.Keys.Where((r) => !entity.Data.Resources.ContainsKey(r)).ToList(),
                     EditEntityResource, NewEntityResource, "Resources:", "(No Resources)", "Add Resource:");
            entity.Data.Resources.Clear();
            foreach(var resource in resourceList)
            {
                entity.Data.Resources.Add(resource.Resource, resource);
            }
            BattleGUI.EditListString(ref entity.NewLifeResource, entity.Data.LifeResources, BattleData.EntityResources.Keys.ToList(),
                           "Life Resources:", "(No Life Resources)", "Add Life Resource:");
            BattleGUI.EndIndent();
        }

        // Triggers
        EditTriggerList(ref entity.NewTrigger, entity.Data.Triggers, ref entity.ShowTriggers, "Entity Triggers");

        // Status Effects
        if (BattleGUI.EditFoldout(ref entity.ShowStatusEffects, "Status Effects"))
        {
            BattleGUI.EditList(ref entity.NewStatusEffect, entity.Data.StatusEffects, BattleData.StatusEffects.Keys.ToList(), EditEntityStatus, NewEntityStatus,
                           "", "(No Status Effects)", "Add Status Effect:");
        }
    }

    BattleGUI.eReturnResult EditEntityStatus(EntityData.EntityStatusEffect status)
    {
        EditorGUILayout.BeginHorizontal();
        BattleGUI.SelectStatus(ref status.Status, "Status Effect:", 90, makeHorizontal: false);
        BattleGUI.EditInt(ref status.Stacks, "Stacks:", 60, makeHorizontal: false);
        var remove = BattleGUI.Remove();
        EditorGUILayout.EndHorizontal();

        if (remove)
        {
            return BattleGUI.eReturnResult.Remove;
        }
        return BattleGUI.eReturnResult.None;
    }

    EntityData.EntityStatusEffect NewEntityStatus(string status)
    {
        return new EntityData.EntityStatusEffect()
        {
            Status = status,
            Stacks = 1
        };
    }

    BattleGUI.eReturnResult EditEntityResource(EntityData.EntityResource resource)
    {
        EditorGUILayout.BeginHorizontal();
        BattleGUI.SelectResource(ref resource.Resource, "Resource:", 90, makeHorizontal: false);
        var copy = BattleGUI.Copy();
        var remove = BattleGUI.Remove();
        EditorGUILayout.EndHorizontal();

        EditValue(resource.ChangePerSecondOutOfCombat, ValueComponent.eValueType.CasterOnly, "Out of Combat Recovery/Drain per second:");
        EditValue(resource.ChangePerSecondInCombat, ValueComponent.eValueType.CasterOnly, "In Combat Recovery/Drain per second:");

        if (copy)
        {
            return BattleGUI.eReturnResult.Copy;
        }
        else if (remove)
        {
            return BattleGUI.eReturnResult.Remove;
        }
        return BattleGUI.eReturnResult.None;
    }

    EntityData.EntityResource NewEntityResource(string resource)
    {
        return new EntityData.EntityResource(resource);
    }

    #region Entity Skills
    BattleGUI.eReturnResult EditSequenceSkill(EntitySkillsData.SequenceElement skill)
    {
        EditorGUILayout.BeginHorizontal();
        BattleGUI.EditEnum(ref skill.ElementType, "Sequence Element Type:", Space, 100, makeHorizontal: false);
        var copy = BattleGUI.Copy();
        var remove = BattleGUI.Remove();
        EditorGUILayout.EndHorizontal();

        BattleGUI.StartIndent();
        if (skill.ElementType == EntitySkillsData.SequenceElement.eElementType.Skill)
        {
            BattleGUI.SelectSkill(ref skill.SkillID, "Skill ID:", 100, true);
        }
        else if (skill.ElementType == EntitySkillsData.SequenceElement.eElementType.RandomSkill)
        {
            BattleGUI.EditList(ref NewSkill, skill.RandomSkills, BattleData.Skills.Keys.ToList(),
                     EditRandomSkill, NewRandomSkill, "Possible Skills:", "(No Skills)", "Add Skill:");
        }

        EditorGUILayout.BeginHorizontal();
        BattleGUI.EditInt(ref skill.UsesMin, "Uses Min:", 60, makeHorizontal: false);
        BattleGUI.EditInt(ref skill.UsesMax, "Uses Max:", 60, makeHorizontal: false);
        EditorGUILayout.EndHorizontal();
        BattleGUI.EditFloat(ref skill.ExecuteChance, "Execute Chance:", 100);
        BattleGUI.EndIndent();

        if (copy)
        {
            return BattleGUI.eReturnResult.Copy;
        }
        else if (remove)
        {
            return BattleGUI.eReturnResult.Remove;
        }
        return BattleGUI.eReturnResult.None;
    }
    EntitySkillsData.SequenceElement NewSequenceSkill(string skillID)
    {
        return new EntitySkillsData.SequenceElement(skillID);
    }

    BattleGUI.eReturnResult EditRandomSkill(EntitySkillsData.RandomSkill skill)
    {
        EditorGUILayout.BeginHorizontal();
        BattleGUI.SelectSkill(ref skill.SkillID, "Skill ID:", 80);
        var copy = BattleGUI.Copy();
        var remove = BattleGUI.Remove();
        EditorGUILayout.EndHorizontal();

        BattleGUI.StartIndent();
        BattleGUI.EditFloat(ref skill.Weight, "Weight:", 80);
        BattleGUI.EndIndent();

        if (copy)
        {
            return BattleGUI.eReturnResult.Copy;
        }
        else if (remove)
        {
            return BattleGUI.eReturnResult.Remove;
        }
        return BattleGUI.eReturnResult.None;
    }

    EntitySkillsData.RandomSkill NewRandomSkill(string skillID)
    {
        return new EntitySkillsData.RandomSkill(skillID);
    }

    BattleGUI.eReturnResult EditInputSkill(EntitySkillsData.InputSkill skill)
    {
        EditorGUILayout.BeginHorizontal();
        BattleGUI.SelectSkill(ref skill.SkillID, "Skill ID:", 70);
        var copy = BattleGUI.Copy();
        var remove = BattleGUI.Remove();
        EditorGUILayout.EndHorizontal();

        BattleGUI.StartIndent();
        EditorGUILayout.BeginHorizontal();
        BattleGUI.EditEnum(ref skill.KeyCode, $"{skill.SkillID} Input:", Space, makeHorizontal: false);
        if (BattleData.GetSkillData(skill.SkillID).HasChargeTime)
        {
            BattleGUI.EditBool(ref skill.HoldToCharge, "Hold to Charge", makeHorizontal: false);
        }
        EditorGUILayout.EndHorizontal();
        BattleGUI.EndIndent();

        if (copy)
        {
            return BattleGUI.eReturnResult.Copy;
        }
        else if (remove)
        {
            return BattleGUI.eReturnResult.Remove;
        }

        return BattleGUI.eReturnResult.None;
    }

    EntitySkillsData.InputSkill NewInputSkill(string skillID)
    {
        return new EntitySkillsData.InputSkill(skillID);
    }

    void EditAttributeDict(ref string newAttribute, Dictionary<string, Vector2> dict, ref bool show, string label)
    {
        show = EditorGUILayout.Foldout(show, label);
        if (show)
        {
            BattleGUI.StartIndent();
            var keys = dict.Keys.ToList();
            for (int i = 0; i < dict.Count; i++)
            {
                var attribute = keys[i];
                var value = dict[attribute];

                GUILayout.BeginHorizontal();
                BattleGUI.EditFloat(ref value.x, $"Min {attribute}:", 130, makeHorizontal: false);
                BattleGUI.EditFloat(ref value.y, $"Max {attribute}:", 130, makeHorizontal: false);

                if (keys[i] != attribute && !dict.ContainsKey(attribute))
                {
                    dict.Remove(keys[i]);
                }
                else
                {
                    attribute = keys[i];
                }

                dict[attribute] = value;

                if (BattleGUI.Remove())
                {
                    dict.Remove(keys[i]);
                    i--;
                    keys = dict.Keys.ToList();
                }
                GUILayout.EndHorizontal();
            }

            var options = BattleData.EntityAttributes.Where(a => !dict.ContainsKey(a)).ToList();
            if (options.Count > 0)
            {
                GUILayout.BeginHorizontal();
                BattleGUI.SelectStringFromList(ref newAttribute, BattleData.EntityAttributes.Where(a => !dict.ContainsKey(a)).ToList(), "Add Attribute:", 130, makeHorizontal: false);
                if (BattleGUI.Add())
                {
                    dict.Add(newAttribute, Vector2.one);
                }
                GUILayout.EndHorizontal();
            }
            BattleGUI.EndIndent();
        }
    }
    #endregion

    #region Factions
    void EditFactions()
    {
        ShowFactions = EditorGUILayout.Foldout(ShowFactions, "Entity Factions");
        if (ShowFactions)
        {
            if (ShowHelp)
            {
                BattleGUI.Help("Each Entity is part of a faction. The lists of friendly and enemy Entities help " + 
                     "determine which Entities can be affected by Actions used by an Entity, for example " + 
                     "a healing spell can be set to only affect friendly entities.");
            }

            BattleGUI.StartIndent();
            for (int i = 0; i < Factions.Count; i++)
            {
                BattleGUI.EditorDrawLine();
                GUILayout.BeginHorizontal();
                BattleGUI.Label("Faction: ", 60);

                var oldName = Factions[i].Data.FactionID;
                Factions[i].Data.FactionID = GUILayout.TextField(Factions[i].Data.FactionID, GUILayout.Width(200));
                if (BattleGUI.SaveChanges())
                {
                    GUI.FocusControl(null);
                    var value = Factions[i].Data.Copy();

                    if (oldName != Factions[i].Data.FactionID)
                    {
                        if (!BattleData.Factions.ContainsKey(Factions[i].Data.FactionID))
                        {
                            BattleData.Factions.Remove(oldName);
                        }
                        else
                        {
                            Factions[i].Data.FactionID = oldName;
                        }
                    }
                    BattleData.Factions[Factions[i].Data.FactionID] = value;
                }

                if (BattleGUI.Copy())
                {
                    var value = BattleGUI.Copy(Factions[i].Data);
                    var copyNo = 2;
                    var id = Factions[i].Data.FactionID + copyNo;
                    while (BattleData.Factions.ContainsKey(id))
                    {
                        copyNo++;
                        id = Factions[i].Data.FactionID + copyNo;
                    }

                    value.FactionID = id;
                    BattleData.Factions.Add(id, value);
                    Factions.Add(new EditorFaction(value));
                }

                if (BattleGUI.Remove())
                {
                    Factions.RemoveAt(i);
                    BattleData.EntityResources.Remove(Factions[i].Data.FactionID);
                }
                GUILayout.EndHorizontal();

                EditFactionList(Factions[i].Data, Factions[i].Data.FriendlyFactions, ref Factions[i].NewFriendlyFaction,
                                "Friendly Factions:", "Add Friendly Faction:", "(No Friendly Factions)");
                EditFactionList(Factions[i].Data, Factions[i].Data.EnemyFactions, ref Factions[i].NewEnemyFaction,
                                "Enemy Factions:", "Add Enemy Faction:", "(No Enemy Factions)");
            }

           BattleGUI.EditorDrawLine();;
            GUILayout.BeginHorizontal();
            BattleGUI.Label("New Faction: ", 90);
            NewFaction = GUILayout.TextField(NewFaction, GUILayout.Width(200));
            if (BattleGUI.Add() && !string.IsNullOrEmpty(NewFaction) &&
                !BattleData.Factions.ContainsKey(NewFaction))
            {
                var newFactionData = new FactionData(NewFaction);
                Factions.Add(new EditorFaction(newFactionData));
                BattleData.Factions.Add(NewFaction, newFactionData);
                NewFaction = "";
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }

    void EditFactionList(FactionData faction, List<string> factions, ref string newFaction, string label = "", string addLabel = "", string noLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label);
        }

        BattleGUI.StartIndent();
        if (factions.Count > 0)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                BattleGUI.Label($"� {factions[i]}", 100);
                if (BattleGUI.Remove())
                {
                    factions.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            BattleGUI.Label(noLabel);
        }

        var exclude = new List<List<string>>() { faction.FriendlyFactions, faction.EnemyFactions };
        if (exclude.Count < Factions.Count)
        {
            var options = BattleData.Factions.Keys.ToList();
            foreach (var list in exclude)
            {
                foreach (var f in list)
                {
                    if (options.Contains(f))
                    {
                        options.Remove(f);
                    }
                }
            }

            if (options.Count > 0)
            {
                GUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(addLabel))
                {
                    BattleGUI.Label($"{addLabel}", 140);
                }

                var copy = newFaction; // This is needed for the lambda expression to work.
                var index = options.FindIndex(0, a => a.Equals(copy));
                if (index < 0)
                {
                    index = 0;
                }
                newFaction = options[EditorGUILayout.Popup(index, options.ToArray(),
                            GUILayout.Width(70))];

                if (BattleGUI.Button("+", 20) && newFaction != null)
                {
                    factions.Add(newFaction);
                }

                GUILayout.EndHorizontal();
            }
        }
        BattleGUI.EndIndent();
    }


    #endregion
    #endregion

    #region Components
    #region Action Components
    Dictionary<Action.eActionType, string> ActionHelp = new Dictionary<Action.eActionType, string>()
    {
        [Action.eActionType.ApplyCooldown] = "",
        [Action.eActionType.CollectCost] = "",
        [Action.eActionType.Destroy] = "",
        [Action.eActionType.LoopBack] = "",
        [Action.eActionType.Message] = "",
        [Action.eActionType.PayloadArea] = "",
        [Action.eActionType.PayloadDirect] = "",
        [Action.eActionType.SpawnEntity] = "",
        [Action.eActionType.SpawnProjectile] = "",
        [Action.eActionType.SetAnimation] = "",
    };
    BattleGUI.eReturnResult EditAction(Action action, ref bool show)
    {
        if (BattleGUI.EditFoldout(ref show, $"{action.ActionType} Action : {action.Timestamp}"))
        {
            GUILayout.BeginHorizontal();
            var remove = BattleGUI.Remove();
            var copy = BattleGUI.Copy();
            GUILayout.EndHorizontal();

            if (remove)
            {
                return BattleGUI.eReturnResult.Remove;
            }
            if (copy)
            {
                return BattleGUI.eReturnResult.Copy;
            }

            if (ShowHelp && ActionHelp.ContainsKey(action.ActionType))
            {
                BattleGUI.Help(ActionHelp[action.ActionType]);
            }

            BattleGUI.StartIndent();

            BattleGUI.EditString(ref action.ActionID, "Action ID:", Space);
            BattleGUI.EditFloat(ref action.Timestamp, "Timestamp:", Space);

            switch (action.ActionType)
            {
                case Action.eActionType.ApplyCooldown:
                {
                    if (!(action is ActionCooldown a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    BattleGUI.EditFloat(ref a.Cooldown, "Cooldown:", Space, 120);

                    BattleGUI.EditEnum(ref a.ChangeMode, "Change Mode:", Space, 120);
                    BattleGUI.EditEnum(ref a.CooldownTarget, "Cooldown Target:", Space, 120);

                    GUILayout.BeginHorizontal();
                    if (a.CooldownTarget == ActionCooldown.eCooldownTarget.Skill)
                    {
                        BattleGUI.SelectSkill(ref a.CooldownTargetName, "Cooldown Skill ID:", Space);
                    }
                    else if (a.CooldownTarget == ActionCooldown.eCooldownTarget.SkillGroup)
                    {
                        EditSkillGroup(ref a.CooldownTargetName, "Cooldown Skill Group Name:", Space);
                    }

                    GUILayout.EndHorizontal();

                    break;
                }
                case Action.eActionType.CollectCost:
                {
                    if (!(action is ActionCostCollection a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    BattleGUI.SelectResource(ref a.ResourceName, "Resource Collected: ", Space);

                    EditValue(a.Cost, ValueComponent.eValueType.CasterOnly, "Cost:");
                    var maxValue = a.MaxCost != null;
                    BattleGUI.EditBool(ref maxValue, "Limit Cost");

                    if (maxValue)
                    {
                        if (a.MaxCost == null)
                        {
                            a.MaxCost = new Value();
                        }
                        EditValue(a.MaxCost, ValueComponent.eValueType.CasterOnly, "Max Cost:");
                    }
                    else
                    {
                        a.MaxCost = null;
                    }

                    BattleGUI.EditBool(ref a.Optional, "Is Optional");

                    break;
                }
                case Action.eActionType.Destroy:
                {
                    if (!(action is ActionDestroy a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    BattleGUI.EditEnum(ref a.EntityToDestroy, "Entity to Destroy:");

                    break;
                }
                case Action.eActionType.DoNothing:
                {
                    if (!(action is ActionDoNothing a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    break;
                }
                case Action.eActionType.LoopBack:
                {
                    if (!(action is ActionLoopBack a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    BattleGUI.EditFloatSlider(ref a.GoToTimestamp, "Go To Timestamp:", 0.0f, a.Timestamp, Space);
                    BattleGUI.EditInt(ref a.Loops, "Loops:", Space);

                    break;
                }
                case Action.eActionType.Message:
                {
                    if (!(action is ActionMessage a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    BattleGUI.EditString(ref a.MessageString, "Message Text:", Space, 300);
                    BattleGUI.EditColor(ref a.MessageColor, "Message Colour:", Space);

                    break;
                }
                case Action.eActionType.PayloadArea:
                {
                    if (!(action is ActionPayloadArea a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    var editorAction = EditorPayloadAction.GetEditorPayloadAction(a);
                    if (editorAction == null)
                    {
                        EditorPayloadAction.AddPayloadAction(a);
                        return BattleGUI.eReturnResult.None;
                    }

                    EditPayloadAction(a, editorAction);
                    EditAreas(a.AreasAffected, "Areas Affected By Payload", ref editorAction.ShowAreas);

                    break;
                }
                case Action.eActionType.PayloadDirect:
                {
                    if (!(action is ActionPayloadDirect a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    EditPayloadAction(a, EditorPayloadAction.GetEditorPayloadAction(a));
                    BattleGUI.EditEnum(ref a.ActionTargets, "Payload Targets:", Space);
                    if (a.ActionTargets == ActionPayloadDirect.eDirectActionTargets.TaggedEntity)
                    {
                        BattleGUI.EditString(ref a.EntityTag, "Target Tag:", Space);
                    }

                    break;
                }
                case Action.eActionType.SpawnProjectile:
                {
                    if (!(action is ActionProjectile a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    EditActionProjectile(a);

                    break;
                }
                case Action.eActionType.SpawnEntity:
                {
                    if (!(action is ActionSummon a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    EditActionSummon(a);

                    break;
                }
                case Action.eActionType.SetAnimation:
                {
                    if (!(action is ActionAnimationSet a))
                    {
                        return BattleGUI.eReturnResult.Remove;
                    }

                    break;
                }
            }

            EditActionConditions(action);
            BattleGUI.EndIndent();
        }
        return BattleGUI.eReturnResult.None;
    }

    #region Summon Actions
    void EditActionSummon(ActionSummon a, EntityData.eEntityType summonType = EntityData.eEntityType.SummonnedEntity)
    {
        BattleGUI.SelectEntity(ref a.EntityID, summonType, "Summonned Entity:", Space);

        if (a.SummonAtPosition == null)
        {
            a.SummonAtPosition = new TransformData();
        }
        EditPosition(a.SummonAtPosition, "Summon At Position:");

        BattleGUI.EditFloat(ref a.SummonDuration, "Summon Duration:", Space);
        BattleGUI.EditInt(ref a.SummonLimit, $"Max Summonned {a.EntityID}s:", Space);

        // Shared attributes
        var options = BattleData.Entities[a.EntityID].BaseAttributes.Keys.Where(k => !a.SharedAttributes.Keys.Contains(k)).ToList();
        BattleGUI.EditFloatDict(a.SharedAttributes, "Inherited Attributes:", options, ref NewString, ": ", Space, $"Add Attribute:", Space);

        BattleGUI.EditBool(ref a.LifeLink, "Kill Entity When Summoner Dies");
        BattleGUI.EditBool(ref a.InheritFaction, "Inherit Summoner's Faction");

        // Summon behaviour
        if (summonType == EntityData.eEntityType.SummonnedEntity)
        {
            var follow = a.PreferredDistanceFromSummoner > Constants.Epsilon;
            BattleGUI.EditBool(ref follow, "Follow Summoner");

            if (follow)
            {
                if (a.PreferredDistanceFromSummoner < Constants.Epsilon)
                {
                    a.PreferredDistanceFromSummoner = 5.0f;
                }
                BattleGUI.EditFloat(ref a.PreferredDistanceFromSummoner, "Preferred Distance from Summoner:", Space + 70);
            }
            BattleGUI.EditFloat(ref a.MaxDistanceFromSummoner, "Max Distance from Summoner:", Space + 70);
            BattleGUI.EditEnum(ref a.OnSummonerOutOfRange, "On Summoner out of range:", Space + 70);
        }
    }

    void EditActionProjectile(ActionProjectile a)
    {
        EditActionSummon(a, EntityData.eEntityType.Projectile);

        var newMode = a.ProjectileMovementMode;
        BattleGUI.EditEnum(ref newMode, "Projectile Movement Mode:", 200);
        if (a.ProjectileMovementMode != newMode)
        {
            if (newMode == ActionProjectile.eProjectileMovementMode.Homing)
            {
                a.Gravity = Constants.Gravity;
            }
            else if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Homing)
            {
                a.Gravity = 0.0f;
            }

            a.ProjectileMovementMode = newMode;
        }

        // Mode-specific values
        BattleGUI.StartIndent();
        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Homing ||
            a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            BattleGUI.EditEnum(ref a.Target, "Moving Toward:", 200);
            if (a.Target == ActionProjectile.eTarget.StaticPosition)
            {
                if (a.TargetPosition == null)
                {
                    a.TargetPosition = new TransformData();
                }
                BattleGUI.StartIndent();
                EditPosition(a.TargetPosition, "Position:");
                BattleGUI.EndIndent();
            }
        }

        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            BattleGUI.EditFloatSlider(ref a.ArchAngle, "Arch Angle:", 1.0f, 85.0f, 200);
            BattleGUI.EditFloat(ref a.Gravity, "Gravity (Affects Speed):", 200);
        }

        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Orbit)
        {
            BattleGUI.EditEnum(ref a.Anchor, "Orbit Anchor:", 200);
            if (a.Anchor == ActionProjectile.eAnchor.CustomPosition)
            {
                if (a.AnchorPosition == null)
                {
                    a.AnchorPosition = new TransformData();
                }
                BattleGUI.StartIndent();
                EditPosition(a.AnchorPosition, "Position:");
                BattleGUI.EndIndent();
            }
        }

        // Projectile Timeline
        if (a.ProjectileMovementMode != ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditProjectileTimeline(a);
        }
        BattleGUI.EndIndent();

        // Projectile Triggers
        EditCollisionReactions(a.OnEnemyHit, "On Enemy Hit:", a.SkillID);
        EditCollisionReactions(a.OnFriendHit, "On Friend Hit:", a.SkillID);
        EditCollisionReactions(a.OnTerrainHit, "On Terrain Hit:", a.SkillID);
    }

    void EditCollisionReactions(List<ActionProjectile.OnCollisionReaction> reactions, string label, string skillID = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label);
        }

        BattleGUI.StartIndent();
        if (reactions.Count == 0)
        {
            BattleGUI.Label("No Reactions");
        }

        for (int i = 0; i < reactions.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            BattleGUI.EditEnum(ref reactions[i].Reaction, "", makeHorizontal: false);
            var remove = BattleGUI.Remove();
            EditorGUILayout.EndHorizontal();

            if (remove)
            {
                reactions.RemoveAt(i);
                i--;
                continue;
            }

            if (reactions[i].Reaction == ActionProjectile.OnCollisionReaction.eReactionType.ExecuteActions)
            {
                EditActionTimeline(reactions[i].Actions, ref NewAction, ref ShowValues[i], "Projectile Action Timeline:", skillID, showIndex: 1500);
            }
        }

        GUILayout.BeginHorizontal();
        BattleGUI.EditEnum(ref NewReaction, "New Collision Reaction: ", 150, 200, false);
        if (BattleGUI.Add())
        {
            reactions.Add(new ActionProjectile.OnCollisionReaction(NewReaction));
        }
        EditorGUILayout.EndHorizontal();
        BattleGUI.EndIndent();
    }

    void EditProjectileTimeline(ActionProjectile a)
    {
        GUILayout.BeginHorizontal();
        BattleGUI.Label("Projectile Timeline", 200);
        if (a.ProjectileTimeline.Count > 1 && BattleGUI.Button("Sort"))
        {
            GUI.FocusControl(null);
            a.ProjectileTimeline.Sort((s1, s2) => s1.Timestamp.CompareTo(s2.Timestamp));
        }
        GUILayout.EndHorizontal();

        BattleGUI.StartIndent();
        for (int i = 0; i < a.ProjectileTimeline.Count; i++)
        {
            GUILayout.BeginHorizontal();
            BattleGUI.Label($"State {i}", 100);
            if (BattleGUI.Copy())
            {
                a.ProjectileTimeline.Add(BattleGUI.Copy(a.ProjectileTimeline[i]));
            }

            var remove = BattleGUI.Remove();
            GUILayout.EndHorizontal();

            if (remove)
            {
                a.ProjectileTimeline.RemoveAt(i);
                i--;
                continue;
            }

            EditProjectileTimelineState(a.ProjectileTimeline[i], a.ProjectileMovementMode);
        }

        GUILayout.BeginHorizontal();
        if (BattleGUI.Button("Add Projectile State", 160))
        {
            a.ProjectileTimeline.Add(new ActionProjectile.ProjectileState(a.ProjectileTimeline.Count > 0 ?
                                     a.ProjectileTimeline[a.ProjectileTimeline.Count - 1].Timestamp : 0.0f));
        }
        GUILayout.EndHorizontal();
        BattleGUI.EndIndent();
    }

    void EditProjectileTimelineState(ActionProjectile.ProjectileState state, ActionProjectile.eProjectileMovementMode mode)
    {
        BattleGUI.StartIndent();
        BattleGUI.EditFloat(ref state.Timestamp, "Timestamp:", 80);
        BattleGUI.EditVector2(ref state.SpeedMultiplier, "Speed (Random between X and Y):");
        BattleGUI.EditVector2(ref state.RotationPerSecond, "Rotation Speed (Random between X and Y):");
        if (mode == ActionProjectile.eProjectileMovementMode.Free)
        {
            BattleGUI.EditVector2(ref state.RotationY, "Rotate By Degrees (Random between X and Y):");
        }
        BattleGUI.EndIndent();
    }   
    #endregion

    void EditActionTimeline(ActionTimeline timeline, ref Action.eActionType newAction, ref bool showTimeline, string title = "", string skillID = "", int showIndex = 300)
    {
        if (BattleGUI.EditFoldout(ref showTimeline, title))
        {
            if (timeline == null)
            {
                timeline = new ActionTimeline();
            }

            var sort = (timeline.Count > 1 && BattleGUI.Button("Sort", 90));

            for (int i = 0; i < timeline.Count; i++)
            {
                // Show and edit an action and check if remove button was pressed.
                var result = EditAction(timeline[i], ref ShowValues[showIndex+i]);
                if (result == BattleGUI.eReturnResult.Remove)
                {
                    timeline.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (result == BattleGUI.eReturnResult.Copy)
                {
                    var copy = BattleGUI.CopyAction(timeline[i]);
                    timeline.Add(copy);
                    sort = true;

                    if (newAction == Action.eActionType.PayloadArea || newAction == Action.eActionType.PayloadDirect)
                    {
                        EditorPayloadAction.AddPayloadAction(copy as ActionPayload);
                    }
                }
            }

            if (sort)
            {
                GUI.FocusControl(null);
                timeline.Sort((a1, a2) => a1.Timestamp.CompareTo(a2.Timestamp));
            }

           BattleGUI.EditorDrawLine();;

            // New action
            GUILayout.BeginHorizontal();
            BattleGUI.EditEnum(ref newAction, "New Action: ", 188, 200, false);
            if (BattleGUI.Add())
            {
                var a = Action.MakeAction(newAction, skillID);
                a.ActionID = newAction.ToString() + "Action";
                if (timeline.Count > 0)
                {
                    a.Timestamp = timeline.Count > 0 ? timeline[timeline.Count - 1].Timestamp : 0.0f;
                }
                a.SkillID = skillID;
                timeline.Add(a);

                if (newAction == Action.eActionType.PayloadArea || newAction == Action.eActionType.PayloadDirect)
                {
                    EditorPayloadAction.AddPayloadAction(a as ActionPayload);
                }
            }
            GUILayout.EndHorizontal();

            if (BattleGUI.Button("Hide Timeline", 120))
            {
                showTimeline = false;
            }
           BattleGUI.EditorDrawLine();;
        }
    }

    void EditActionConditions(Action action)
    {
        if (action.ActionConditions == null)
        {
            action.ActionConditions = new List<ActionCondition>();
        }

        BattleGUI.Label("Action Conditions:");
        BattleGUI.StartIndent();

        if (action.ActionConditions.Count == 0)
        {
            BattleGUI.Label("No Action Conditions");
        }

        for (int i = 0; i < action.ActionConditions.Count; i++)
        {
            var condition = action.ActionConditions[i];

            GUILayout.BeginHorizontal();
            BattleGUI.EditEnum(ref condition.Condition, "Condition:", Space, makeHorizontal: false);
            var remove = BattleGUI.Remove();
            GUILayout.EndHorizontal();

            if (remove)
            {
                action.ActionConditions.RemoveAt(i);
                i--;
                continue;
            }

            BattleGUI.StartIndent();
            if (condition.Condition == ActionCondition.eActionCondition.ValueBelow ||
                condition.Condition == ActionCondition.eActionCondition.ValueAbove)
            {
                BattleGUI.EditEnum(ref condition.ConditionValueType, "Value Type:", Space);
                GUILayout.BeginHorizontal();
                var text = (condition.Condition == ActionCondition.eActionCondition.ValueBelow ? " <" : " >");

                switch(condition.ConditionValueType)
                {
                    case ActionCondition.eConditionValueType.ActionResult:
                    {
                        BattleGUI.EditString(ref condition.ConditionTarget, "Action:", Space, makeHorizontal: false);
                        BattleGUI.EditFloat(ref condition.ConditionValueBoundary, "Value" + text, 60, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.ChargeRatio:
                    {
                        BattleGUI.EditFloatSlider(ref condition.ConditionValueBoundary, "Skill Charge Ratio" + text, 0.0f, 1.0f, 120, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.DistanceFromTarget:
                    {
                        BattleGUI.EditFloat(ref condition.ConditionValueBoundary, "Distance" + text, Space + 40, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.RandomValue:
                    {
                        BattleGUI.EditFloatSlider(ref condition.ConditionValueBoundary, "Random Value" + text, 0.0f, 1.0f, 120, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.ResourceRatio:
                    {
                        BattleGUI.SelectResource(ref condition.ConditionTarget, "", makeHorizontal: false);
                        BattleGUI.EditFloatSlider(ref condition.ConditionValueBoundary, text, 0.0f, 1.0f, 40, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.ResourceCurrent:
                    {
                        BattleGUI.SelectResource(ref condition.ConditionTarget, "", makeHorizontal: false);
                        BattleGUI.EditFloat(ref condition.ConditionValueBoundary, text, 40, makeHorizontal: false);
                        break;
                    }
                }

                GUILayout.EndHorizontal();
            }
            else if (condition.Condition == ActionCondition.eActionCondition.ActionSuccess || 
                     condition.Condition == ActionCondition.eActionCondition.ActionFail)
            {
                BattleGUI.EditString(ref condition.ConditionTarget, "Action:", Space);
            }
            else if (condition.Condition == ActionCondition.eActionCondition.HasStatus)
            {
                BattleGUI.SelectStatus(ref condition.ConditionTarget, "Status Effect", Space);
                BattleGUI.EditInt(ref condition.MinStatusStacks, "Min Stacks:", Space);
            }
            BattleGUI.EndIndent();
        }

        GUILayout.BeginHorizontal();
        BattleGUI.EditEnum(ref NewActionCondition, "New Action Condition:", Space, makeHorizontal: false);
        if (BattleGUI.Add())
        {
            action.ActionConditions.Add(new ActionCondition(NewActionCondition));
        }
        GUILayout.EndHorizontal();
        BattleGUI.EndIndent();
    }

    void EditArea(ActionPayloadArea.Area area)
    {
        BattleGUI.StartIndent();
        var newShape = area.Shape;
        BattleGUI.EditEnum(ref newShape, "Shape:", 150);
        if (newShape != area.Shape)
        {
            if (area.Shape == ActionPayloadArea.Area.eShape.Cube)
            {
                area.Dimensions.z = area.Dimensions.y;
                area.Dimensions.y = 360.0f;
                area.InnerDimensions.y = 0.0f;
            }
            if (newShape == ActionPayloadArea.Area.eShape.Cube)
            {
                area.Dimensions.y = area.Dimensions.x;
            }

            area.Shape = newShape;
        }

        if (area.Shape == ActionPayloadArea.Area.eShape.Cylinder || area.Shape == ActionPayloadArea.Area.eShape.Sphere)
        {
            BattleGUI.EditFloat(ref area.Dimensions.x, "Radius:", 100);
            BattleGUI.EditFloat(ref area.InnerDimensions.x, "Inner Radius:", 100);

            BattleGUI.EditFloatSlider(ref area.Dimensions.y, "Cone Angle:", 0.0f, 360.0f, 100);
            BattleGUI.EditFloatSlider(ref area.InnerDimensions.y, "Inner Cone Angle:", 0.0f, 360.0f, 100);
        }
        if (area.Shape == ActionPayloadArea.Area.eShape.Cube)
        {
            BattleGUI.EditVector3(ref area.Dimensions, "Dimensions:");
            var inner = new Vector3(area.InnerDimensions.x, 0.0f, area.InnerDimensions.y);
            BattleGUI.EditVector3(ref inner, "Inner Dimensions:");
            area.InnerDimensions.x = inner.x;
            area.InnerDimensions.y = inner.z;
        }
        if (area.Shape == ActionPayloadArea.Area.eShape.Cube || area.Shape == ActionPayloadArea.Area.eShape.Cylinder)
        {
            BattleGUI.EditFloat(ref area.Dimensions.z, "Height:", 100);
        }

        if (area.AreaTransform == null)
        {
            area.AreaTransform = new TransformData();
        }
        EditPosition(area.AreaTransform, "Area Position:");
        BattleGUI.EndIndent();
    }

    void EditAreas(List<ActionPayloadArea.Area> areas, string label, ref bool show)
    {
        if (BattleGUI.EditFoldout(ref show, label))
        {
            BattleGUI.StartIndent();
            for (int i = 0; i < areas.Count; i++)
            {
                GUILayout.BeginHorizontal();
                BattleGUI.Label($"Area [{i}]", 50);

                if (BattleGUI.Remove())
                {
                    areas.RemoveAt(i);
                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();

                EditArea(areas[i]);
            }

            GUILayout.BeginHorizontal();
            if (BattleGUI.Button("Add Area", 90))
            {
                areas.Add(new ActionPayloadArea.Area());
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();
        }
    }
    #endregion

    #region Action Movement/Rotation
    void EditActionMovement(ActionMovement movement, string label, ref bool show, ref bool hasMovement, ref bool stopMovement)
    {
        if (BattleGUI.EditFoldout(ref show, label))
        {
            BattleGUI.EditBool(ref stopMovement, "Stop Current Movement");

            BattleGUI.EditBool(ref hasMovement, "Apply Movement");

            if (hasMovement && movement != null)
            {
                BattleGUI.StartIndent();
                BattleGUI.EditEnum(ref movement.MovementType, "Movement Type:", Space);
                if (movement.MovementType == ActionMovement.eMovementType.MoveToPosition ||
                    movement.MovementType == ActionMovement.eMovementType.LaunchToPosition ||
                    movement.MovementType == ActionMovement.eMovementType.TeleportToPosition)
                {
                    EditPosition(movement.TargetPosition, "Target Position:");

                    if (movement.MovementType == ActionMovement.eMovementType.MoveToPosition)
                    {
                        BattleGUI.EditBool(ref movement.HorizontalMovementOnly, "Horizontal Movement Only");
                    }

                    if (movement.MovementType == ActionMovement.eMovementType.LaunchToPosition)
                    {
                        BattleGUI.EditFloatSlider(ref movement.LaunchAngle, "Launch Angle:", 1.0f, 85.0f, 200);
                    }
                }

                if (movement.MovementType == ActionMovement.eMovementType.MoveInDirection)
                {
                    EditDirection(movement.TargetPosition.Direction, "Forward Direction:");
                }

                if (movement.MovementType != ActionMovement.eMovementType.TeleportToPosition)
                {
                    BattleGUI.EditEnum(ref movement.FaceDirection, "Face Direction");
                    BattleGUI.EditFloat(ref movement.Speed, "Movement Speed:", Space);

                    if (movement.MovementType != ActionMovement.eMovementType.LaunchToPosition)
                    {
                        BattleGUI.EditFloat(ref movement.SpeedChangeOverTime, "Speed Change per sec:", Space);
                        if (movement.SpeedChangeOverTime > Constants.Epsilon || movement.SpeedChangeOverTime < -Constants.Epsilon)
                        {
                            BattleGUI.EditFloat(ref movement.MinSpeed, "Movement Min Speed:", Space);
                            BattleGUI.EditFloat(ref movement.MaxSpeed, "Movement Max Speed:", Space);
                        }

                        BattleGUI.EditFloat(ref movement.MaxDuration, "Movement Duration:", Space);
                    }
                }

                BattleGUI.EditFloat(ref movement.InterruptionLevel, "Interruption Level", Space);
                BattleGUI.EditInt(ref movement.Priority, "Priority", Space);

                BattleGUI.EndIndent();
            }
        }
    }

    void EditActionRotation(ActionRotation rotation, string label, ref bool show, ref bool hasRotation, ref bool stopRotation)
    {
        if (BattleGUI.EditFoldout(ref show, label))
        {
            BattleGUI.EditBool(ref stopRotation, "Stop Current Rotation");

            BattleGUI.EditBool(ref hasRotation, "Apply Rotation");

            if (hasRotation && rotation != null)
            {
                BattleGUI.StartIndent();
                BattleGUI.EditEnum(ref rotation.RotationType, "Rotation Type:", Space);

                if (rotation.RotationType == ActionRotation.eRotationType.RotateToDirection ||
                    rotation.RotationType == ActionRotation.eRotationType.SetRotation)
                {
                    EditDirection(rotation.Direction, "Direction:");
                }

                if (rotation.RotationType != ActionRotation.eRotationType.SetRotation)
                {
                    BattleGUI.EditFloat(ref rotation.Speed, "Rotation Speed:", Space);
                    BattleGUI.EditFloat(ref rotation.SpeedChangeOverTime, "Speed Change per sec:", Space);
                    if (rotation.SpeedChangeOverTime > Constants.Epsilon || rotation.SpeedChangeOverTime < -Constants.Epsilon)
                    {
                        BattleGUI.EditFloat(ref rotation.MinSpeed, "Movement Min Speed:", Space);
                        BattleGUI.EditFloat(ref rotation.MaxSpeed, "Movement Max Speed:", Space);
                    }
                    BattleGUI.EditFloat(ref rotation.MaxDuration, "Max Rotation Duration:", Space);
                }

                BattleGUI.EditFloat(ref rotation.InterruptionLevel, "Interruption Level", Space);
                BattleGUI.EditInt(ref rotation.Priority, "Priority", Space);
                BattleGUI.EndIndent();
            }
        }
    }
    #endregion

    #region Transform
    void EditPosition(TransformData transform, string label)
    {
        BattleGUI.Label(label);

        BattleGUI.StartIndent();
        BattleGUI.EditEnum(ref transform.PositionOrigin, "Position Origin:", Space);
        if (transform.PositionOrigin == TransformData.ePositionOrigin.EntityPosition ||
            transform.PositionOrigin == TransformData.ePositionOrigin.EntityOrigin)
        {
            EditTransformTargetEntity(transform.TargetEntity, "Entity:");
        }

        BattleGUI.EditVector3(ref transform.PositionOffset, "Position Offset: ");
        BattleGUI.EditVector3(ref transform.RandomPositionOffset, "Random Position Offset: ");
        EditDirection(transform.Direction, "Offset Direction:");

        BattleGUI.EndIndent();
    }

    void EditDirection(DirectionData direction, string label)
    {
        BattleGUI.Label(label);

        BattleGUI.StartIndent();
        BattleGUI.EditEnum(ref direction.DirectionSource, "Direction Source:", Space);
        if (direction.DirectionSource == DirectionData.eDirectionSource.EntityForward)
        {
            EditTransformTargetEntity(direction.EntityFrom, "Entity:");
        }
        else if (direction.DirectionSource == DirectionData.eDirectionSource.EntityToEntity)
        {
            EditTransformTargetEntity(direction.EntityFrom, "From:");
            EditTransformTargetEntity(direction.EntityTo, "To:");
        }

        BattleGUI.EditFloat(ref direction.DirectionOffset, "Direction Offset: ", Space);
        BattleGUI.EditFloat(ref direction.RandomDirectionOffset, "Random Direction Offset: ", Space);
        BattleGUI.EndIndent();
    }

    void EditTransformTargetEntity(TransformTargetEntity targetEntity, string label)
    {
        BattleGUI.EditEnum(ref targetEntity.EntityTarget, label, Space);

        if (targetEntity.EntityTarget == TransformTargetEntity.eEntity.Tagged)
        {
            BattleGUI.EditString(ref targetEntity.EntityTag, "Entity Tag:", Space);
            BattleGUI.EditEnum(ref targetEntity.TaggedTargetPriority, "Tagged Entity Priority:", Space);
        }
    }
    #endregion

    #region Payload Components
    void EditPayload(PayloadData payload, EditorPayload editorPayload, bool isSkill)
    {
        BattleGUI.StartIndent();

        // Categories
        if (BattleGUI.EditFoldout(ref editorPayload.ShowCategories, "Payload Categories"))
        {
            BattleGUI.StartIndent();
            BattleGUI.EditListString(ref editorPayload.NewCategory, payload.Categories, BattleGUI.CopyList(BattleData.PayloadCategories),
                                     "", "(No Payload Categories)", "Add Payload Category:");
            BattleGUI.EndIndent();
        }

        // Damage/Recovery
        BattleGUI.EditorDrawLine();
        if (BattleGUI.EditFoldout(ref editorPayload.ShowPayloadDamage, "Payload Damage"))
        {
            BattleGUI.StartIndent();
            EditValue(payload.PayloadValue, isSkill ? ValueComponent.eValueType.SkillAction : ValueComponent.eValueType.NonAction);

            var maxValue = payload.PayloadValueMax != null;
            BattleGUI.EditBool(ref maxValue, "Max Payload Value");

            if (maxValue)
            {
                if (payload.PayloadValueMax == null)
                {
                    payload.PayloadValueMax = new Value();
                }
                EditValue(payload.PayloadValueMax, isSkill ? ValueComponent.eValueType.SkillAction : ValueComponent.eValueType.NonAction, "Max Payload Damage:");
            }
            else
            {
                payload.PayloadValueMax = null;
            }

            BattleGUI.SelectResource(ref payload.ResourceAffected, "Resource Affected: ", 150);
            BattleGUI.EditBool(ref payload.IgnoreShield, "Ignore Shield");

            // Effectiveness
            if (BattleGUI.EditFoldout(ref editorPayload.ShowEffectiveness, "Effectiveness against Target Category"))
            {
                if (payload.CategoryMult == null)
                {
                    payload.CategoryMult = new Dictionary<string, float>();
                }
                BattleGUI.EditFloatDict(payload.CategoryMult, "", BattleData.EntityCategories,
                              ref editorPayload.NewEntityCategory, ": ", Space, "Add Category:", Space);
            }

            // Ignored attributes
            if (BattleGUI.EditFoldout(ref editorPayload.ShowIgnoredAttributes, "Attributes Ignored"))
            {
                if (payload.CasterAttributesIgnored == null)
                {
                    payload.CasterAttributesIgnored = new List<string>();
                }
                BattleGUI.EditListString(ref editorPayload.NewAttribute, payload.CasterAttributesIgnored, BattleGUI.CopyList(BattleData.EntityAttributes),
                                         "Caster Attributes Ignored:", "(No Ignored Attributes)", "Add Ignored Attribute:");
                if (payload.TargetAttributesIgnored == null)
                {
                    payload.TargetAttributesIgnored = new List<string>();
                }
                BattleGUI.EditListString(ref editorPayload.NewAttribute, payload.TargetAttributesIgnored, BattleGUI.CopyList(BattleData.EntityAttributes),
                                         "Target Attributes Ignored:", "(No Ignored Attributes)", "Add Ignored Attribute:");
            }

            // Flags
            if (BattleGUI.EditFoldout(ref editorPayload.ShowFlags, "Payload Flags"))
            {
                BattleGUI.EditListString(ref editorPayload.NewFlag, payload.Flags, BattleGUI.CopyList(BattleData.PayloadFlags),
                           "", "(No Payload Flags)", "Add Payload Flag:");
            }
            BattleGUI.EndIndent();
        }

        // Other effects
        BattleGUI.EditorDrawLine();
        if (BattleGUI.EditFoldout(ref editorPayload.ShowOtherEffects, "Other Effects"))
        {
            BattleGUI.StartIndent();
            // Kill/revive
            BattleGUI.EditBool(ref payload.Instakill, "Instakill");
            if (payload.Instakill)
            {
                payload.Revive = false;
            }

            BattleGUI.EditBool(ref payload.Revive, "Revive");
            if (payload.Revive)
            {
                payload.Instakill = false;
            }

            // Aggro
            if (BattleGUI.EditFoldout(ref editorPayload.ShowAggro, "Aggro"))
            {
                var aggro = payload.Aggro != null;
                BattleGUI.EditBool(ref aggro, "Generate Aggro");
                if (aggro)
                {
                    if (payload.Aggro == null)
                    {
                        payload.Aggro = new AggroData.AggroChange();
                    }
                    EditAggroChange(payload.Aggro, "Aggro Generated: ");
                    BattleGUI.StartIndent();
                    BattleGUI.EditBool(ref payload.MultiplyAggroByPayloadValue, "Multiply Aggro by Damage");
                    BattleGUI.EndIndent();
                }
                else
                {
                    payload.Aggro = null;
                }
            }

            // Statuses
            if (BattleGUI.EditFoldout(ref editorPayload.ShowEffects, "Status Effects"))
            {
                if (payload.ApplyStatus == null)
                {
                    payload.ApplyStatus = new List<(string StatusID, int Stacks)>();
                }
                EditStatusStackList(payload.ApplyStatus, ref editorPayload.NewStatusApply,
                                    "Applied Status Effects:", "(No Status Effects)", "Add Status Effect:");
                if (payload.RemoveStatusStacks == null)
                {
                    payload.RemoveStatusStacks = new List<(string StatusID, int Stacks)>();
                }
                EditStatusStackList(payload.RemoveStatusStacks, ref editorPayload.NewStatusRemoveStack,
                            "Removed Status Effects:", "(No Status Effects)", "Add Status Effect:");
                if (payload.ClearStatus == null)
                {
                    payload.ClearStatus = new List<(bool StatusGroup, string StatusID)>();
                }
                EditStatusList(payload.ClearStatus, ref editorPayload.NewStatusClear,
                        "Cleared Status Effects:", "(No Status Effects)", "Add:");
            }

            // Tag
            if (BattleGUI.EditFoldout(ref editorPayload.ShowTag, "Tag"))
            {
                var tag = payload.Tag != null;
                BattleGUI.EditBool(ref tag, "Tag");
                if (tag)
                {
                    if (payload.Tag == null)
                    {
                        payload.Tag = new TagData();
                    }
                    BattleGUI.StartIndent();
                    BattleGUI.EditString(ref payload.Tag.TagID, "Tag ID: ", 150, 150);
                    BattleGUI.EditInt(ref payload.Tag.TagLimit, "Max tagged: ", 150);
                    BattleGUI.EditFloat(ref payload.Tag.TagDuration, "Tag Duration: ", 150);
                    BattleGUI.EndIndent();
                }
                else
                {
                    payload.Tag = null;
                }
            }

            // Movement
            if (BattleGUI.EditFoldout(ref editorPayload.ShowTransformChanges, "Position/Direction Change"))
            {
                // Rotation
                var hasRotation = payload.Rotation != null;
                EditActionRotation(payload.Rotation, "Rotation", ref editorPayload.ShowRotation, ref hasRotation, ref payload.StopCurrentRotation);

                if (hasRotation && payload.Rotation == null)
                {
                    payload.Rotation = new ActionRotation();
                }
                else if (!hasRotation)
                {
                    payload.Rotation = null;
                }

                // Movement
                var hasMovement = payload.Movement != null;
                EditActionMovement(payload.Movement, "Movement", ref editorPayload.ShowMovement, ref hasMovement, ref payload.StopCurrentMovement);

                if (hasMovement && payload.Movement == null)
                {
                    payload.Movement = new ActionMovement();
                }
                else if (!hasMovement)
                {
                    payload.Movement = null;
                }
            }
            BattleGUI.EndIndent();
        }

        // Payload conditions
        BattleGUI.EditorDrawLine();
        if (BattleGUI.EditFoldout(ref editorPayload.ShowConditions, "Payload Conditions"))
        {
            BattleGUI.StartIndent();
            BattleGUI.EditFloatSlider(ref payload.SuccessChance, "Success Chance: ", 0.0f, 1.0f, 120, 150);

            var hasPayloadCondition = payload.PayloadCondition != null;
            BattleGUI.EditBool(ref hasPayloadCondition, "Payload has Condition");
            if (hasPayloadCondition)
            {
                if (payload.PayloadCondition == null)
                {
                    payload.PayloadCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
                }
                EditPayloadCondition(payload.PayloadCondition);

                // Alternate payload
                var hasAlternatePayload = payload.AlternatePayload != null;
                BattleGUI.EditBool(ref hasAlternatePayload, "Alternate Payload");
                if (hasAlternatePayload)
                {
                    if (payload.AlternatePayload == null)
                    {
                        var newPayload = new PayloadData();
                        payload.AlternatePayload = newPayload;
                        editorPayload.AlternatePayload = new EditorPayload(newPayload);
                    }
                    EditPayload(payload.AlternatePayload, editorPayload.AlternatePayload, isSkill);
                }
                else
                {
                    editorPayload.AlternatePayload = null;
                    payload.AlternatePayload = null;
                }
            }
            else
            {
                payload.PayloadCondition = null;
            }
            BattleGUI.EndIndent();
        }
        BattleGUI.EndIndent();
        BattleGUI.EditorDrawLine();
    }

    void EditPayloadAction(ActionPayload action, EditorPayloadAction editorAction)
    {
        BattleGUI.EditEnum(ref action.Target, "Targets Affected: ", Space);
        BattleGUI.EditEnum(ref action.TargetState, "Required Target State: ", Space);

        if (editorAction == null)
        {
            EditorPayloadAction.AddPayloadAction(action);
            return;
        }

        EditPayloadList(action.PayloadData, ref editorAction.ShowPayloads, editorAction, "Payloads:", action);

        BattleGUI.EditInt(ref action.TargetLimit, "Max Targets Affected:", Space);
        if (action.TargetLimit > 0)
        {
            BattleGUI.EditEnum(ref action.TargetPriority, "Target Priority: ", Space);
            if (action.TargetPriority == ActionPayload.eTargetPriority.ResourceCurrentHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceCurrentLowest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceMaxHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceMaxLowest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceRatioHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceRatioLowest)
            {
                BattleGUI.StartIndent();
                BattleGUI.SelectResource(ref action.Resource, "Resource: ", Space);
                BattleGUI.EndIndent();
            }
        }
    }

    void EditPayloadList(List<PayloadData> payloadData, ref bool show, List<EditorPayload> editorPayloads, string label, Action action = null)
    {
        show = EditorGUILayout.Foldout(show, label);
        if (show)
        {
            BattleGUI.StartIndent();
            for (int i = 0; i < payloadData.Count; i++)
            {
                BattleGUI.EditorDrawLine();

                GUILayout.BeginHorizontal();
                BattleGUI.Label($"Payload {i}", 100);
                if (BattleGUI.Button("Remove Payload", Space))
                {
                    if (action != null)
                    {
                        EditorPayloads[EditorPayloadAction.ActionKey(action)].RemoveAt(i);
                    }
                    payloadData.RemoveAt(i);

                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();

                if (editorPayloads == null)
                {
                    editorPayloads = new List<EditorPayload>();

                    for (int j = 0; j < payloadData.Count; j++)
                    {
                        editorPayloads.Add(new EditorPayload(payloadData[j]));
                    }
                }

                var isSkill = action != null && !string.IsNullOrEmpty(action.SkillID);
                EditPayload(payloadData[i], editorPayloads[i], isSkill);
            }

            // New payload
            GUILayout.BeginHorizontal();
            if (BattleGUI.Button("Add New Payload", Space))
            {
                var newPayload = new PayloadData();
                payloadData.Add(newPayload);
                if (action != null)
                {
                    EditorPayloads[EditorPayloadAction.ActionKey(action)].Add(new EditorPayload(newPayload));
                }
            }
            GUILayout.EndHorizontal();
            BattleGUI.EndIndent();

            if (payloadData.Count > 0 && BattleGUI.Button("Hide Payloads", 120))
            {
                show = false;
            }
        }
    }

    void EditPayloadFloatList(List<(PayloadData, float)> list, ref bool show, List<EditorPayload> editorPayloads, string label, string floatFieldLabel)
    {
        show = EditorGUILayout.Foldout(show, label);
        if (show)
        {
            for (int i = 0; i < list.Count; i++)
            {
                BattleGUI.EditorDrawLine();

                BattleGUI.StartIndent();
                GUILayout.BeginHorizontal();
                BattleGUI.Label($"Payload {i}", 100);
                if (BattleGUI.Button("Remove Payload", Space))
                {
                    list.RemoveAt(i);

                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();

                if (editorPayloads == null)
                {
                    editorPayloads = new List<EditorPayload>();
                }

                for (int j = 0; j < list.Count; j++)
                {
                    editorPayloads.Add(new EditorPayload(list[i].Item1));
                }

                var payload = list[i].Item1;
                var floatValue = list[i].Item2;

                BattleGUI.StartIndent();
                BattleGUI.EditFloat(ref floatValue, floatFieldLabel, Space);
                BattleGUI.EndIndent();
                EditPayload(payload, editorPayloads[i], false);

                list[i] = (payload, floatValue);

                BattleGUI.EndIndent();
            }

            // New payload
            GUILayout.BeginHorizontal();
            if (BattleGUI.Button("Add New Payload", Space))
            {
                var newPayload = new PayloadData();
                list.Add((newPayload, 1.0f));
            }
            GUILayout.EndHorizontal();

            if (list.Count > 0 && BattleGUI.Button("Hide Payloads", 120))
            {
                show = false;
            }
        }
    }

    void EditPayloadCondition(PayloadCondition condition)
    {
        var newConditionType = condition.ConditionType;
        BattleGUI.EditEnum(ref newConditionType, "Condition Type:");
        BattleGUI.EditBool(ref condition.ExpectedResult, "Required Result: " + (condition.ExpectedResult ? "Success" : "Failure"));

        if (newConditionType != condition.ConditionType)
        {
            condition.SetCondition(newConditionType);
        }

        switch (condition.ConditionType)
        {
            case PayloadCondition.ePayloadConditionType.AngleBetweenDirections:
            {
                BattleGUI.EditEnum(ref condition.Direction1, "Direction 1:", 100);
                BattleGUI.EditEnum(ref condition.Direction2, "Direction 2:", 100);

                BattleGUI.EditFloatSlider(ref condition.Range.x, "Angle Min:", 0.0f, 100);
                BattleGUI.EditFloatSlider(ref condition.Range.y, "Angle Max:", 0.0f, 100);
                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetHasStatus:
            {
                BattleGUI.SelectStatus(ref condition.StatusID, "Status Effect:", makeHorizontal: true);
                if (BattleData.StatusEffects.ContainsKey(condition.StatusID))
                {
                    BattleGUI.EditIntSlider(ref condition.MinStatusStacks, "Min. Status Stacks:", 1, BattleData.StatusEffects[condition.StatusID].MaxStacks);
                }
                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetWithinDistance:
            {
                BattleGUI.EditFloat(ref condition.Range.x, "Distance Min:");
                BattleGUI.EditFloat(ref condition.Range.y, "Distance Max:");

                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetResourceRatioWithinRange:
            {
                BattleGUI.EditFloatSlider(ref condition.Range.x, "Distance Min:", 0.0f, 1.0f);
                BattleGUI.EditFloatSlider(ref condition.Range.y, "Distance Max:", 0.0f, 1.0f);

                break;
            }
        }

        var hasAndCondition = condition.AndCondition != null;
        BattleGUI.EditBool(ref hasAndCondition, "AND Condition");
        if (hasAndCondition)
        {
            if (condition.AndCondition == null)
            {
                condition.AndCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            BattleGUI.StartIndent();
            EditPayloadCondition(condition.AndCondition);
            BattleGUI.EndIndent();
        }
        else
        {
            condition.AndCondition = null;
        }

        var hasOrCondition = condition.OrCondition != null;
        BattleGUI.EditBool(ref hasOrCondition, "OR Condition");
        if (hasOrCondition)
        {
            if (condition.OrCondition == null)
            {
                condition.OrCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            BattleGUI.StartIndent();
            EditPayloadCondition(condition.OrCondition);
            BattleGUI.EndIndent();
        }
        else
        {
            condition.OrCondition = null;
        }
    }
    #endregion

    #region Triggers
    void EditTriggerList(ref TriggerData.eTrigger newTrigger, List<TriggerData> list, ref bool show, string label)
    {
        show = EditorGUILayout.Foldout(show, label);
        if (show)
        {
            BattleGUI.StartIndent();
            for (int i = 0; i < list.Count; i++)
            {
                var result = EditTrigger(list[i], $"Trigger {i}", i, listElement: true);
                if (result == BattleGUI.eReturnResult.Remove)
                {
                    list.RemoveAt(i);
                }
                else if (result == BattleGUI.eReturnResult.Copy)
                {
                    var copy = BattleGUI.Copy(list[i]);
                    list.Add(copy);
                }
            }
            BattleGUI.EndIndent();

            GUILayout.BeginHorizontal();
            BattleGUI.EditEnum(ref newTrigger, "New Trigger:", Space, makeHorizontal: false);
            if (BattleGUI.Add())
            {
                list.Add(new TriggerData(newTrigger));
            }
            GUILayout.EndHorizontal();
        }
    }

    BattleGUI.eReturnResult EditTrigger(TriggerData trigger, string label, int index = 0, bool listElement = false)
    {
        if (trigger == null)
        {
            trigger = new TriggerData();
        }

        if (BattleGUI.EditFoldout(ref ShowValues[index], label))
        {
            GUILayout.BeginHorizontal();
            BattleGUI.Label(label, Space);
            var copy = (listElement && BattleGUI.Copy());
            var remove = (listElement && BattleGUI.Remove());
            GUILayout.EndHorizontal();

            if (remove)
            {
                return BattleGUI.eReturnResult.Remove;
            }
            else if (copy)
            {
                return BattleGUI.eReturnResult.Copy;
            }

            BattleGUI.StartIndent();
            BattleGUI.EditEnum(ref trigger.Trigger, "Trigger:", Space);
            BattleGUI.EditEnum(ref trigger.EntityAffected, "Entity Affected:", Space);

            EditTriggerConditions(trigger.Trigger, trigger.Conditions, "Trigger Conditions:", ref ShowTriggerConditions);

            BattleGUI.EditFloat(ref trigger.Cooldown, "Trigger Cooldown", Space);
            BattleGUI.EditInt(ref trigger.Limit, "Trigger Limit", Space);

            BattleGUI.EditFloatSlider(ref trigger.TriggerChance, "Trigger Chance:", 0.0f, 1.0f, Space);

            EditActionTimeline(trigger.Actions, ref NewAction, ref ShowValues[index+100], "On Trigger:", showIndex: index * 100);

            BattleGUI.EndIndent();
        }
        return BattleGUI.eReturnResult.None;
    }

    void EditTriggerConditions(TriggerData.eTrigger trigger, List<TriggerData.TriggerCondition> conditions, string label, ref bool show)
    {
        if (conditions == null)
        {
            conditions = new List<TriggerData.TriggerCondition>();
        }

        if (BattleGUI.EditFoldout(ref show, label))
        {
            BattleGUI.StartIndent();
            for (int i = 0; i < conditions.Count; i++)
            {
                var remove = !EditTriggerCondition(conditions[i], trigger);
                if (remove)
                {
                    conditions.RemoveAt(i);
                    i--;
                }
            }

            if (BattleGUI.Add())
            {
                conditions.Add(new TriggerData.TriggerCondition());
            }
            BattleGUI.EndIndent();
        }
    }

    bool EditTriggerCondition(TriggerData.TriggerCondition condition, TriggerData.eTrigger trigger, bool removable = true)
    {
        BattleGUI.StartIndent();
        GUILayout.BeginHorizontal();
        BattleGUI.EditEnum(ref condition.ConditionType, condition.AvailableConditions(trigger), "Condition:", makeHorizontal: false);
        if (removable && BattleGUI.Remove())
        {
            return false;
        }
        GUILayout.EndHorizontal();
        BattleGUI.EditBool(ref condition.DesiredOutcome, $"Desired Outcome: {(condition.DesiredOutcome ? "Success" : "Fail")}");

        switch (condition.ConditionType)
        {
            case TriggerData.TriggerCondition.eConditionType.CausedBySkill:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.Skills.Keys.ToList(), "Skill: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedBySkillGroup:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.SkillGroups.Keys.ToList(), "Skill Group: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedByAction:
            {
                BattleGUI.EditString(ref condition.StringValue, "Action ID:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.PayloadCategory:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.PayloadCategories, "Category: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.PayloadFlag:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.PayloadFlags, "Payload Flag: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.ResultFlag:
            {
                BattleGUI.EditString(ref condition.StringValue, "Result Flag:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedByStatus:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Effect: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedByStatusGroup:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Group: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.ResourceAffected:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntityResourceMin:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                BattleGUI.EditFloat(ref condition.FloatValue, "Min Value:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceResourceMin:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                BattleGUI.EditFloat(ref condition.FloatValue, "Min Value:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntityResourceRatioMin:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                BattleGUI.EditFloatSlider(ref condition.FloatValue, "Min Ratio:", 0.0f, 1.0f, Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceResourceRatioMin:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                BattleGUI.EditFloatSlider(ref condition.FloatValue, "Min Ratio:", 0.0f, 1.0f, Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.PayloadResultMin:
            {
                BattleGUI.EditFloat(ref condition.FloatValue, "Min Resource Change:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.ActionResultMin:
            {
                BattleGUI.EditFloat(ref condition.FloatValue, "Min Action Result:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.NumTargetsAffectedMin:
            {
                BattleGUI.EditInt(ref condition.IntValue, "Min Targets Affected:");
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntityHasStatus:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Effect: ", Space);
                BattleGUI.EditInt(ref condition.IntValue, "Min Stacks:");
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceHasStatus:
            {
                BattleGUI.SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Effect: ", Space);
                BattleGUI.EditInt(ref condition.IntValue, "Min Stacks:");
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceIsEnemy:
            {
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceIsFriend:
            {
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntitiesEngagedMin:
            {
                BattleGUI.EditInt(ref condition.IntValue, "Min Entities Engaged:");
                break;
            }
        }

        var hasAndCondition = condition.AndCondition != null;
        BattleGUI.EditBool(ref hasAndCondition, "AND Condition");
        if (hasAndCondition)
        {
            if (condition.AndCondition == null)
            {
                condition.AndCondition = new TriggerData.TriggerCondition();
            }
            EditTriggerCondition(condition.AndCondition, trigger, removable: false);
        }
        else
        {
            condition.AndCondition = null;
        }

        var hasOrCondition = condition.OrCondition != null;
        BattleGUI.EditBool(ref hasOrCondition, "OR Condition");
        if (hasOrCondition)
        {
            if (condition.OrCondition == null)
            {
                condition.OrCondition = new TriggerData.TriggerCondition();
            }
            EditTriggerCondition(condition.OrCondition, trigger, removable: false);
        }
        else
        {
            condition.OrCondition = null;
        }

        BattleGUI.EndIndent();
        return true;
    }
    #endregion

    #region Skill Components
    void EditSkillGroup(ref string skillGroup, string label = "", int labelWidth = 60)
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label, labelWidth);
        }

        var copy = skillGroup;
        var skills = BattleData.SkillGroups.Keys.ToList();
        var index = skills.FindIndex(0, s => s.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        skillGroup = skills[EditorGUILayout.Popup(index, skills.ToArray(),
                GUILayout.Width(250))];
    }

    void EditSkillChargeData(SkillChargeData data, ref Action.eActionType newChargeAction, ref bool showTimeline, string skillID)
    {
        BattleGUI.EditFloat(ref data.RequiredChargeTime, "Required Skill Charge Time:", 150);
        BattleGUI.EditFloat(ref data.FullChargeTime, "Full Skill Charge Time:", 150);

        BattleGUI.EditBool(ref data.MovementCancelsCharge, "Movement Cancels Charge");

        EditActionTimeline(data.PreChargeTimeline, ref newChargeAction, ref showTimeline, "Charge Timeline", skillID: skillID, showIndex: 1000);

        BattleGUI.EditBool(ref data.ShowUI, "Show Skill Charge UI");
    }
    #endregion

    #region Status Components
    void EditStatusGroup(ref string statusGroup, string label = "", int labelWidth = 200, int inputWidth = 150)
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label, labelWidth);
        }

        var copy = statusGroup;
        var options = BattleData.StatusEffectGroups.Keys.ToList();
        var index = options.FindIndex(0, s => s.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        statusGroup = options[EditorGUILayout.Popup(index, options.ToArray(), GUILayout.Width(inputWidth))];
    }

    void EditStatusList(List<(bool, string)> list, ref (bool, string) newElement, string label, string noLabel, string addLabel)
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label);
        }

        BattleGUI.StartIndent();
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var isGroup = list[i].Item1 ? 1 : 0;
                var status = list[i].Item2;

                isGroup = EditorGUILayout.Popup(isGroup, new string[2] { "Status Effect:", "Status Effect Group:" }, GUILayout.Width(180));

                if (isGroup == 0)
                {
                    BattleGUI.SelectStatus(ref status);
                }
                else
                {
                    EditStatusGroup(ref status);
                }

                list[i] = (isGroup == 1, status);

                if (BattleGUI.Remove())
                {
                    list.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            BattleGUI.Label(noLabel);
        }

        GUILayout.BeginHorizontal();
        if (!string.IsNullOrEmpty(addLabel))
        {
            BattleGUI.Label(addLabel, addLabel.Count() * 8);
        }

        var group = newElement.Item1 ? 1 : 0;
        group = EditorGUILayout.Popup(group, new string[2] { "Status Effect:", "Status Effect Group:" }, GUILayout.Width(180));

        var options = group == 1 ? BattleData.StatusEffectGroups.Keys.ToList() : BattleData.StatusEffects.Keys.ToList();
        for (int i = 0; i < options.Count; i++)
        {
            if (list.Any((s) => s.Item2.Equals(options[i])))
            {
                options.RemoveAt(i);
                i--;
            }
        }

        var copy = newElement.Item2;
        var index = options.FindIndex(0, a => a.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        var newStatus = options[EditorGUILayout.Popup(index, options.ToArray(),
                        GUILayout.Width(200))];

        newElement = (group == 1, newStatus);

        if (BattleGUI.Button("+", 20))
        {
            list.Add(newElement);
        }

        GUILayout.EndHorizontal();
        BattleGUI.EndIndent();
    }

    void EditStatusStackList(List<(string, int)> list, ref string newElement,
                             string label = "", string noLabel = "", string addLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label);
        }

        BattleGUI.StartIndent();
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var status = list[i].Item1;
                var stacks = list[i].Item2;

                GUILayout.BeginHorizontal();
                BattleGUI.SelectStatus(ref status, "Status Effect:", 90);
                BattleGUI.EditInt(ref stacks, "Stacks:", 50, makeHorizontal: false);

                stacks = Mathf.Min(stacks, BattleData.StatusEffects[status].MaxStacks);

                list[i] = (status, stacks);

                if (BattleGUI.Remove())
                {
                    list.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            BattleGUI.Label(noLabel);
        }

        var options = BattleData.StatusEffects.Keys.ToList();
        for (int i = 0; i < options.Count; i++)
        {
            if (list.Any((s) => s.Item1.Equals(options[i])))
            {
                options.RemoveAt(i);
                i--;
            }
        }

        if (options.Count > 0)
        {
            GUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(addLabel))
            {
                BattleGUI.Label(addLabel, addLabel.Count() * 8);
            }

            var copy = newElement; // This is needed for the lambda expression to work.
            var index = options.FindIndex(0, a => a.Equals(copy));
            if (index < 0)
            {
                index = 0;
            }
            newElement = options[EditorGUILayout.Popup(index, options.ToArray(),
                         GUILayout.Width(200))];

            if (BattleGUI.Button("+", 20) && newElement != null)
            {
                list.Add((newElement, 1));
            }

            GUILayout.EndHorizontal();
        }
        BattleGUI.EndIndent();
    }
    #endregion

    #region Values
    void EditValue(Value v, ValueComponent.eValueType valueType, string label = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            BattleGUI.Label(label);
        }

        BattleGUI.StartIndent();
        for (int i = 0; i < v.Count; i++)
        {
            var result = EditValueComponent(v[i], valueType, editPotency: true);
            if (result == BattleGUI.eReturnResult.Copy)
            {
                v.Add(BattleGUI.Copy(v[i]));
            }
            else if (result == BattleGUI.eReturnResult.Remove)
            {
                v.RemoveAt(i);
                i--;
            }
        }
        if (v.Count < 1)
        {
            BattleGUI.Label("(No Value Components)");
        }

        if (BattleGUI.Button("Add Component", 120))
        {
            v.Add(new ValueComponent());
        }
        BattleGUI.EndIndent();
    }

    BattleGUI.eReturnResult EditValueComponent(ValueComponent component, ValueComponent.eValueType valueType, bool editPotency)
    {
        GUILayout.BeginHorizontal();
        var options = ValueComponent.AvailableComponentTypes[valueType];
        var strings = BattleGUI.EnumStrings(options);

        if (options.Count < 1)
        {
            BattleGUI.Label($"(No available component types for the {valueType} context)");
            GUILayout.EndHorizontal();
            return BattleGUI.eReturnResult.None;
        }

        var index = Array.IndexOf(strings, component.ComponentType.ToString());
        if (index < 0)
        {
            index = 0;
        }
        var newComponentType = options[EditorGUILayout.Popup(index, strings, GUILayout.Width(160))];

        if (component.ComponentType != newComponentType)
        {
            if (newComponentType == ValueComponent.eValueComponentType.ActionResultValue)
            {
                component.Attribute = "";
            }
            component.ComponentType = newComponentType;
        }

        BattleGUI.Label(" Value:", 43);
        if (editPotency)
        {
            BattleGUI.EditFloat(ref component.Potency, "", 70, makeHorizontal: false);
        }
        else
        {
            BattleGUI.Label(component.Potency.ToString(), 70);
        }

        if (component.ComponentType == ValueComponent.eValueComponentType.CasterAttributeBase ||
            component.ComponentType == ValueComponent.eValueComponentType.CasterAttributeCurrent ||
            component.ComponentType == ValueComponent.eValueComponentType.TargetAttributeBase ||
            component.ComponentType == ValueComponent.eValueComponentType.TargetAttributeCurrent)
        {
            BattleGUI.Label("x", 10);
            BattleGUI.SelectAttribute(ref component.Attribute, "", 70);
        }
        else if (component.ComponentType == ValueComponent.eValueComponentType.CasterResourceCurrent ||
                 component.ComponentType == ValueComponent.eValueComponentType.CasterResourceMax ||
                 component.ComponentType == ValueComponent.eValueComponentType.CasterResourceRatio ||
                 component.ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                 component.ComponentType == ValueComponent.eValueComponentType.TargetResourceMax ||
                 component.ComponentType == ValueComponent.eValueComponentType.TargetResourceRatio)
        {
            BattleGUI.Label("x", 10);
            BattleGUI.SelectResource(ref component.Attribute, makeHorizontal: false);
        }
        else if (component.ComponentType == ValueComponent.eValueComponentType.ActionResultValue)
        {
            BattleGUI.Label("x Action ID:", 50);
            component.Attribute = GUILayout.TextField(component.Attribute, GUILayout.Width(210));
        }
        else if (component.ComponentType == ValueComponent.eValueComponentType.CasterLevel || 
                 component.ComponentType == ValueComponent.eValueComponentType.TargetLevel)
        {
            BattleGUI.Label("x Level", 50);
        }

        var result = BattleGUI.eReturnResult.None;
        if (BattleGUI.Copy())
        {
            result = BattleGUI.eReturnResult.Copy;
        }
        if (BattleGUI.Remove())
        {
            result = BattleGUI.eReturnResult.Remove;
        }
        GUILayout.EndHorizontal();

        return result;
    }
    #endregion
    #endregion
}
