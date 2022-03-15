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
    string Path = "Data/BattleData";
    const int Space = 150;

    #region GameData
    bool ShowAttributes = false;
    List<string> EntityAttributes = new List<string>();
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
    List<string> Categories = new List<string>();
    string NewCategory = "";

    bool ShowPayloadFlags = false;
    List<string> PayloadFlags = new List<string>();
    string NewPayloadFlag = "";

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

        public EditorSkill(SkillData skillData = null)
        {
            SkillData = Copy(skillData);
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
                Skills = Utility.CopyList(list);
            }
        }
    }
    List<EditorSkillGroup> SkillGroups = new List<EditorSkillGroup>();
    string NewSkillGroup;

    class EditorPayload
    {
        public string NewCategory = "";
        public string NewFlag = "";
        public string NewStatusApply = "";
        public string NewStatusRemoveStack = "";
        public (bool, string) NewStatusClear = (false, "");
        public bool ShowTimeline = false;
        public EditorPayload AlternatePayload = null;

        public EditorPayload()
        {

        }
    }

    class EditorPayloadAction : List<EditorPayload>
    {
        public bool ShowPayloads;

        public EditorPayloadAction(int payloads)
        {
            ShowPayloads = false;

            for (int i = 0; i < payloads; i++)
            {
                Add(new EditorPayload());
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
            EditorPayloads[ActionKey(action)] = new EditorPayloadAction(action.PayloadData.Count);
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
    bool ShowValues;
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
                Statuses = Utility.CopyList(list);
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
            Data = Copy(data);

            if (Data.OnInterval == null)
            {
                Data.OnInterval = new List<(PayloadData PayloadData, float Interval)>();
            }
            OnInterval = new List<EditorPayload>();
            while(OnInterval.Count < Data.OnInterval.Count)
            {
                OnInterval.Add(new EditorPayload());
            }

            if (Data.OnCleared == null)
            {
                Data.OnCleared = new List<PayloadData>();
            }
            OnCleared = new List<EditorPayload>();
            while (OnCleared.Count < Data.OnCleared.Count)
            {
                OnCleared.Add(new EditorPayload());
            }

            if (Data.OnExpired == null)
            {
                Data.OnExpired = new List<PayloadData>();
            }
            OnExpired = new List<EditorPayload>();
            while (OnCleared.Count < Data.OnExpired.Count)
            {
                OnExpired.Add(new EditorPayload());
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

    [MenuItem("Tools/Battle System Data")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BattleSystemDataEditor));
    }

    void Awake()
    {
        var pathSaved = PlayerPrefs.HasKey(PathPlayerPrefs) || string.IsNullOrEmpty(PlayerPrefs.GetString(PathPlayerPrefs));
        if (!pathSaved)
        {
            PlayerPrefs.SetString(PathPlayerPrefs, Path);
        }
        Path = PlayerPrefs.GetString(PathPlayerPrefs);
        BattleData.LoadData(Path);
        UpdateValues();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        Label("Resources/", 70);
        Path = GUILayout.TextField(Path, GUILayout.Width(300f));
        PlayerPrefs.SetString(PathPlayerPrefs, Path);
        Label($".json", 30);

        if (GUILayout.Button("Load"))
        {
            GUI.FocusControl(null);

            BattleData.LoadData(Path);
            UpdateValues();
        }

        if (GUILayout.Button("Save"))
        {
            GUI.FocusControl(null);

            if (!string.IsNullOrEmpty(Path))
            {
                BattleData.SaveData(Path);
            }
            UpdateValues();
        }
        GUILayout.EndHorizontal();

        Tab = GUILayout.Toolbar(Tab, new string[] { "Game Data", "Skill Data", "Status Effect Data", "Entity Data" });

        GUILayout.BeginHorizontal();
        Label("", 5);
        ShowHelp = EditorGUILayout.Toggle(ShowHelp, GUILayout.Width(12));
        Label("Show Help");
        GUILayout.EndHorizontal();

        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos);

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

    void UpdateValues()
    {
        EntityAttributes = new List<string>();
        foreach (var attribute in BattleData.EntityAttributes)
        {
            EntityAttributes.Add(attribute);
        }

        EntityResources = new List<EditorResource>();
        foreach (var resource in BattleData.EntityResources)
        {
            EntityResources.Add(new EditorResource(resource.Key, resource.Value.Copy()));
        }

        Categories = new List<string>();
        foreach (var category in BattleData.Categories)
        {
            Categories.Add(category);
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
    }

    #region GameData
    void EditGameData()
    {
        EditAttributes();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        EditResources();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        EditCategories();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        EditPayloadFlags();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
    }

    void EditAttributes()
    {
        ShowAttributes = EditorGUILayout.Foldout(ShowAttributes, "Entity Attributes");
        if (ShowAttributes)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("Entity Attributes can be used to customise Values. Values apply to a variety of areas in the system, such as damage dealt, " +
                                        "buffs granted or the max value of a resource.\n " +
                                        "Attributes can also be applied to the functions in Formulae.cs to further customise how damage is calculated and customise" +
                                        " other entity properties such as movement speed, jump height, skill cast speed or status effect duration.\n" +
                                        "Entity Attributes can be affected by status effects. When using attributes to define values it's possible to use the base value " +
                                        "(without attribute changes) or the current value (with attribute changes).", MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < EntityAttributes.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EntityAttributes[i] = GUILayout.TextField(EntityAttributes[i], GUILayout.Width(203));
                if (Rename())
                {
                    BattleData.EntityAttributes[i] = EntityAttributes[i];
                }

                var remove = Remove();
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
            Label($"New Attribute: ", 90);
            NewAttribute = GUILayout.TextField(NewAttribute, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewAttribute) &&
                !EntityAttributes.Contains(NewAttribute))
            {
                EntityAttributes.Add(NewAttribute);
                BattleData.EntityAttributes.Add(NewAttribute);
                NewAttribute = "";
            }
            GUILayout.EndHorizontal();
            EndIndent();
        }
    }

    void EditResources()
    {
        ShowResources = EditorGUILayout.Foldout(ShowResources, "Entity Resources");
        if (ShowResources)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("Resources are properties of an Entity that can change as a result of using Actions. " +
                                        "Example resources include HP, MP, Shield and Stamina.\n" +
                                        "When defining a resource, its max value has to be specified. The resource will not go above this value.",
                                        MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < EntityResources.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                Label("Resource: ", 74);
                var oldName = EntityResources[i].Name;
                EntityResources[i].Name = GUILayout.TextField(EntityResources[i].Name, GUILayout.Width(200));
                if (Button("Save Changes", 110))
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

                var remove = Remove();
                GUILayout.EndHorizontal();

                if (remove)
                {
                    EntityResources.RemoveAt(i);
                    BattleData.EntityResources.Remove(EntityResources[i].Name);
                    i--;
                    continue;
                }

                EditValue(EntityResources[i].Value, eEditorValueRange.Resource, $"Max { EntityResources[i].Name.ToUpper()} Value:");
            }

            EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

            GUILayout.BeginHorizontal();
            Label("New Resource: ", 88);
            NewResource.Name = GUILayout.TextField(NewResource.Name, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewResource.Name) &&
                !BattleData.EntityResources.ContainsKey(NewResource.Name))
            {
                EntityResources.Add(new EditorResource(NewResource.Name, NewResource.Value.Copy()));
                BattleData.EntityResources.Add(NewResource.Name, NewResource.Value.Copy());
                NewResource.Name = "";
            }
            GUILayout.EndHorizontal();
            EndIndent();
        }
    }

    void EditCategories()
    {
        ShowCategories = EditorGUILayout.Foldout(ShowCategories, "Categories");
        if (ShowCategories)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("Categories are custom properties that can describe Entities and Payloads." +
                                    "These properties can be used to customise triggers and status effects.\n" +
                                    "Example properties include elements such as fire and water, damage types " +
                                    "such as piercing and blunt and skill types such as damage, healing and buff.", MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < Categories.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Categories[i] = GUILayout.TextField(Categories[i], GUILayout.Width(203));
                if (Rename())
                {
                    BattleData.Categories[i] = Categories[i];
                }

                var remove = Remove();
                GUILayout.EndHorizontal();

                if (remove)
                {
                    Categories.RemoveAt(i);
                    BattleData.Categories.RemoveAt(i);
                    i--;
                    continue;
                }
            }
            GUILayout.BeginHorizontal();
            Label("New Category: ", 100);
            NewCategory = GUILayout.TextField(NewCategory, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewCategory) &&
                !BattleData.Categories.Contains(NewCategory))
            {
                Categories.Add(NewCategory);
                BattleData.Categories.Add(NewCategory);
                NewCategory = "";
            }
            GUILayout.EndHorizontal();
            EndIndent();
        }
    }

    void EditPayloadFlags()
    {
        ShowPayloadFlags = EditorGUILayout.Foldout(ShowPayloadFlags, "Payload Flags");
        if (ShowPayloadFlags)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("Payload flags can be used to customise damage or healing applied through Payloads in Formulae.cs.\n" +
                                        "(A Payload defines all changes applied to an Entity that a Payload Action is used on, such as attribute " +
                                        "changes and status effects.)", MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < PayloadFlags.Count; i++)
            {
                GUILayout.BeginHorizontal();
                PayloadFlags[i] = GUILayout.TextField(PayloadFlags[i], GUILayout.Width(203));
                if (Rename())
                {
                    BattleData.PayloadFlags[i] = PayloadFlags[i];
                }

                var remove = Remove();
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
            Label("New Payload Flag: ", 120);
            NewPayloadFlag = GUILayout.TextField(NewPayloadFlag, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewPayloadFlag) &&
                !BattleData.PayloadFlags.Contains(NewPayloadFlag))
            {
                PayloadFlags.Add(NewPayloadFlag);
                BattleData.PayloadFlags.Add(NewPayloadFlag);
                NewPayloadFlag = "";
            }
            GUILayout.EndHorizontal();
            EndIndent();
        }
    }
    #endregion

    #region Skill Data
    void EditSkillData()
    {
        EditSkillGroups();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        EditSkills();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
    }

    void EditSkillGroups()
    {
        ShowSkillGroups = EditorGUILayout.Foldout(ShowSkillGroups, "Skill Groups");
        if (ShowSkillGroups)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("", MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < BattleData.SkillGroups.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                Label("Skill Group: ", 80);

                var oldName = SkillGroups[i].GroupID;
                SkillGroups[i].GroupID = GUILayout.TextField(SkillGroups[i].GroupID, GUILayout.Width(200));
                if (Button("Save Changes", 110))
                {
                    GUI.FocusControl(null);
                    var value = Utility.CopyList(SkillGroups[i].Skills);

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

                if (Remove())
                {
                    SkillGroups.RemoveAt(i);
                    BattleData.SkillGroups.Remove(SkillGroups[i].GroupID);
                }
                GUILayout.EndHorizontal();

                EditListString(ref SkillGroups[i].NewSkill, SkillGroups[i].Skills, BattleData.Skills.Keys.ToList(),
                               "", "No Skills in the Skill Group", "Add Skill:");
            }

            // New skill group
            GUILayout.BeginHorizontal();
            Label("New Skill Group: ", 128);
            NewSkillGroup = GUILayout.TextField(NewSkillGroup, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewSkillGroup) &&
                !BattleData.SkillGroups.ContainsKey(NewSkillGroup))
            {
                SkillGroups.Add(new EditorSkillGroup(NewSkillGroup));
                BattleData.SkillGroups.Add(NewSkillGroup, new List<string>());
                NewSkillGroup = "";
            }
            GUILayout.EndHorizontal();
            EndIndent();
        }
    }

    void EditSkills()
    {
        ShowSkillData = EditorGUILayout.Foldout(ShowSkillData, "Skills");
        if (ShowSkillData)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("", MessageType.None);
            }

            for (int i = 0; i < Skills.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Label("", 10);
                var skill = Skills[i];
                skill.ShowSkill = EditorGUILayout.Foldout(skill.ShowSkill, skill.SkillData.SkillID);
                GUILayout.EndHorizontal();
                if (skill.ShowSkill)
                {
                    StartIndent();
                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                    GUILayout.BeginHorizontal();

                    // ID
                    Label("Skill ID: ", 70);
                    skill.SkillID = GUILayout.TextField(skill.SkillID, GUILayout.Width(200));

                    // Save/Remove
                    if (Button("Save Changes", 110))
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

                        var value = Copy(skill.SkillData);

                        if (skill.SkillData.SkillID != skill.SkillID)
                        {
                            if (!BattleData.Skills.ContainsKey(skill.SkillID))
                            {
                                BattleData.Skills.Remove(skill.SkillData.SkillID);
                            }
                            else
                            {
                                skill.SkillID = skill.SkillData.SkillID;
                            }
                        }
                        skill.SkillData.SkillID = skill.SkillID;
                        BattleData.Skills[skill.SkillData.SkillID] = value;
                    }

                    if (Remove())
                    {
                        Skills.RemoveAt(i);
                        BattleData.Skills.Remove(skill.SkillData.SkillID);
                    }
                    GUILayout.EndHorizontal();

                    // Values
                    StartIndent();
                    EditBool(ref skill.SkillData.Interruptible, "Is Interruptible");

                    EditBool(ref skill.HasChargeTime, "Has Charge Time");
                    if (skill.HasChargeTime)
                    {
                        if (skill.SkillData.SkillChargeData == null)
                        {
                            skill.SkillData.SkillChargeData = new SkillChargeData();
                        }
                        EditSkillChargeData(skill.SkillData.SkillChargeData, ref skill.NewChargeTimelineAction,
                                            ref skill.ShowChargeTimeline, skill.SkillID);
                    }

                    // Actions
                    EditActionTimeline(skill.SkillData.SkillTimeline, ref skill.NewTimelineAction,
                                       ref skill.ShowTimeline, "Skill Timeline:", skillID: skill.SkillID);

                    EditBool(ref skill.SkillData.NeedsTarget, "TargetRequired");
                    EditEnum(ref skill.SkillData.PreferredTarget, $"Preferred Target: ");
                    EditFloat(ref skill.SkillData.Range, "Max Distance From Target:", 250, 150);

                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                    EndIndent();
                    EndIndent();
                }
            }

            // New skill
            GUILayout.BeginHorizontal();
            EditString(ref NewSkill, "New Skill: ", 80, 200, false);
            if (Add() && !BattleData.Skills.ContainsKey(NewSkill))
            {
                var newSkill = new SkillData(NewSkill);
                Skills.Add(new EditorSkill(newSkill));
                BattleData.Skills.Add(NewSkill, newSkill);

                NewSkill = "";
            }
            GUILayout.EndHorizontal();
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
                EditorGUILayout.HelpBox("", MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < BattleData.StatusEffectGroups.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                Label("Status Effect Group: ", 120);

                var oldName = StatusGroups[i].GroupID;
                StatusGroups[i].GroupID = GUILayout.TextField(StatusGroups[i].GroupID, GUILayout.Width(200));
                if (Button("Save Changes", 110))
                {
                    GUI.FocusControl(null);
                    var value = Utility.CopyList(StatusGroups[i].Statuses);

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

                if (Remove())
                {
                    StatusGroups.RemoveAt(i);
                    BattleData.StatusEffectGroups.Remove(StatusGroups[i].GroupID);
                }
                GUILayout.EndHorizontal();

                EditListString(ref StatusGroups[i].NewStatus, StatusGroups[i].Statuses, BattleData.StatusEffects.Keys.ToList(),
                               "", "No Status Effects in the Status Effect Group", "Add Status Effect:");
            }

            // New status effect group
            GUILayout.BeginHorizontal();
            Label("New Status Effect Group: ", 165);
            NewStatusGroup = GUILayout.TextField(NewStatusGroup, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewStatusGroup) &&
                !BattleData.StatusEffectGroups.ContainsKey(NewStatusGroup))
            {
                StatusGroups.Add(new EditorStatusGroup(NewStatusGroup));
                BattleData.StatusEffectGroups.Add(NewStatusGroup, new List<string>());
                NewStatusGroup = "";
            }
            GUILayout.EndHorizontal();
            EndIndent();
        }
    }

    void EditStatusEffects()
    {
        ShowStatusEffects = EditorGUILayout.Foldout(ShowStatusEffects, "Status Effects");
        if (ShowStatusEffects)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("", MessageType.None);
            }

            for (int i = 0; i < StatusEffects.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Label("", 10);
                var status = StatusEffects[i];
                status.Show = EditorGUILayout.Foldout(status.Show, status.Data.StatusID);
                GUILayout.EndHorizontal();
                if (status.Show)
                {
                    StartIndent();
                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                    GUILayout.BeginHorizontal();

                    // ID
                    Label("Status ID: ", 70);
                    var newID = status.Data.StatusID;
                    newID = GUILayout.TextField(newID, GUILayout.Width(200));

                    // Save/Remove
                    if (Button("Save Changes", 110))
                    {
                        GUI.FocusControl(null);

                        var value = Copy(status.Data);

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

                    if (Remove())
                    {
                        StatusEffects.RemoveAt(i);
                        BattleData.StatusEffects.Remove(status.Data.StatusID);
                        i--;
                        continue;
                    }
                    GUILayout.EndHorizontal();

                    StartIndent();
                    EditEffects(status);

                    EditInt(ref status.Data.MaxStacks, "Max Stacks:", Space);
                    EditFloat(ref status.Data.Duration, "Duration:", Space);

                    EditPayloadFloatList(status.Data.OnInterval, ref status.ShowOnInterval, status.OnInterval, "On Interval:", "Interval:");
                    EditPayloadList(status.Data.OnCleared, ref status.ShowOnCleared, status.OnCleared, "On Cleared:");
                    EditPayloadList(status.Data.OnExpired, ref status.ShowOnExpired, status.OnExpired, "On Expired:");

                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                    EndIndent();
                    EndIndent();
                }
            }

            // New status effect
            GUILayout.BeginHorizontal();
            EditString(ref NewStatusEffect, "New Status Effect: ", Space, 200, makeHorizontal: false);
            if (Add() && !BattleData.StatusEffects.ContainsKey(NewStatusEffect))
            {
                var newStatus = new StatusEffectData(NewStatusEffect);
                StatusEffects.Add(new EditorStatusEffect(newStatus));
                BattleData.StatusEffects.Add(NewStatusEffect, newStatus);

                NewStatusEffect = "";
            }
            GUILayout.EndHorizontal();
        }
    }

    Dictionary<Effect.eEffectType, string> EffectHelp = new Dictionary<Effect.eEffectType, string>()
    {
        [Effect.eEffectType.AttributeChange] = "",
        [Effect.eEffectType.Convert] = "",
        [Effect.eEffectType.Immunity] = "",
        [Effect.eEffectType.Lock] = "",
        [Effect.eEffectType.ResourceGuard] = "",
        [Effect.eEffectType.Shield] = "",
        [Effect.eEffectType.Trigger] = "",
    };

    void EditEffects(EditorStatusEffect status)
    {
        if (status.Data.Effects.Count == 0)
        {
            Label("No Effects.");
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
        EditEnum(ref NewEffect, "New Effect:", Space, makeHorizontal: false);
        if (Add())
        {
            var newEffect = Effect.MakeNew(NewEffect);
            status.Data.Effects.Add(newEffect);
        }
        GUILayout.EndHorizontal();
    }

    bool EditEffect(Effect effect)
    {
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        GUILayout.BeginHorizontal();
        Label($"{effect.EffectType} Effect", Space);
        var remove = Remove();
        GUILayout.EndHorizontal();

        if (remove)
        {
            return false;
        }

        if (ShowHelp && EffectHelp.ContainsKey(effect.EffectType))
        {
            EditorGUILayout.HelpBox(EffectHelp[effect.EffectType], MessageType.None);
        }

        StartIndent();

        EditInt(ref effect.StacksRequiredMin, "Min Stacks Required:", Space);
        if (effect.StacksRequiredMin < 1)
        {
            effect.StacksRequiredMin = 1;
        }


        EditInt(ref effect.StacksRequiredMax, "Max Stacks Required:", Space);
        if (effect.StacksRequiredMax < effect.StacksRequiredMin)
        {
            effect.StacksRequiredMax = effect.StacksRequiredMin;
        }

        switch (effect.EffectType)
        {
            case Effect.eEffectType.AttributeChange:
            {
                var e = effect as EffectAttributeChange;
                if (e == null)
                {
                    return false;
                }

                SelectAttribute(ref e.Attribute);
                EditValue(e.Value, eEditorValueRange.NonAction, "Change:");

                GUILayout.BeginHorizontal();
                EditEnum(ref e.PayloadTargetType, "Change Affects:", Space, 90, makeHorizontal: false);

                switch (e.PayloadTargetType)
                {
                    case Effect.ePayloadFilter.Action:
                    {
                        EditString(ref e.PayloadTarget, "Action ID:", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Category:
                    {
                        SelectStringFromList(ref e.PayloadTarget, BattleData.Categories.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Skill:
                    {
                        SelectStringFromList(ref e.PayloadTarget, BattleData.Skills.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.SkillGroup:
                    {
                        SelectStringFromList(ref e.PayloadTarget, BattleData.SkillGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Status:
                    {
                        SelectStringFromList(ref e.PayloadTarget, BattleData.StatusEffects.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.StatusGroup:
                    {
                        SelectStringFromList(ref e.PayloadTarget, BattleData.StatusEffectGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                }
                GUILayout.EndHorizontal();
                break;
            }
            case Effect.eEffectType.Convert:
            {
                var e = effect as EffectConvert;
                if (e == null)
                {
                    return false;
                }

                break;
            }
            case Effect.eEffectType.Immunity:
            {
                var e = effect as EffectImmunity;
                if (e == null)
                {
                    return false;
                }   

                GUILayout.BeginHorizontal();
                EditEnum(ref e.PayloadFilter, "Immunity To:", Space, 90, makeHorizontal: false);

                switch (e.PayloadFilter)
                {
                    case Effect.ePayloadFilter.Action:
                    {
                        EditString(ref e.PayloadName, "Action ID:", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Category:
                    {
                        SelectStringFromList(ref e.PayloadName, BattleData.Categories.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Skill:
                    {
                        SelectStringFromList(ref e.PayloadName, BattleData.Skills.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.SkillGroup:
                    {
                        SelectStringFromList(ref e.PayloadName, BattleData.SkillGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.Status:
                    {
                        SelectStringFromList(ref e.PayloadName, BattleData.StatusEffects.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                    case Effect.ePayloadFilter.StatusGroup:
                    {
                        SelectStringFromList(ref e.PayloadName, BattleData.StatusEffectGroups.Keys.ToList(),
                                             "", makeHorizontal: false);
                        break;
                    }
                }
                GUILayout.EndHorizontal();

                EditInt(ref e.Limit, "Max Hits Resisted:", Space);
                EditBool(ref e.EndStatusOnEffectEnd, "End Status when Limit Reached");
                break;
            }
            case Effect.eEffectType.Lock:
            {
                var e = effect as EffectLock;
                if (e == null)
                {
                    return false;
                }

                GUILayout.BeginHorizontal();
                EditEnum(ref e.LockType, "Lock Type:", Space, 90, makeHorizontal: false);
                if (e.LockType == EffectLock.eLockType.Skill)
                {
                    SelectStringFromList(ref e.Skill, BattleData.Skills.Keys.ToList(),
                                         "", makeHorizontal: false);
                }
                else if (e.LockType == EffectLock.eLockType.SkillsGroup)
                {
                    SelectStringFromList(ref e.Skill, BattleData.SkillGroups.Keys.ToList(),
                     "", makeHorizontal: false);
                }

                GUILayout.EndHorizontal();
                break;
            }
            case Effect.eEffectType.ResourceGuard:
            {
                var e = effect as EffectResourceGuard;
                if (e == null)
                {
                    return false;
                }

                SelectResource(ref e.Resource, "Guarded Resource:", Space);
                var hasMin = e.MinValue != null;
                EditBool(ref hasMin, "Min Value");
                if (hasMin)
                {
                    if (e.MinValue == null)
                    {
                        e.MinValue = new Value();
                        e.MinValue.Add(new ValueComponent(ValueComponent.eValueComponentType.FlatValue, 1.0f));
                    }
                    EditValue(e.MinValue, eEditorValueRange.NonAction);
                }

                SelectResource(ref e.Resource, "Guarded Resource:", Space);
                var hasMax = e.MaxValue != null;
                EditBool(ref hasMax, "Max Value");
                if (hasMax)
                {
                    if (e.MaxValue == null)
                    {
                        e.MaxValue = new Value();
                        e.MaxValue.Add(new ValueComponent(ValueComponent.eValueComponentType.TargetResourceMax, 1.0f));
                    }
                    EditValue(e.MaxValue, eEditorValueRange.NonAction);
                }

                EditInt(ref e.Limit, "Max Hits Guarded:", Space);
                EditBool(ref e.EndStatusOnEffectEnd, "End Status when Limit Reached");

                break;
            }
            case Effect.eEffectType.Shield:
            {
                var e = effect as EffectShield;
                if (e == null)
                {
                    return false;
                }

                SelectResource(ref e.ShieldResource, "Shielding Resource:", Space);
                SelectResource(ref e.ShieldedResource, "Shielded Resource:", Space);

                EditValue(e.ShieldResourceToGrant, eEditorValueRange.NonAction, "Shield Resource Granted:");
                EditValue(e.MaxDamageAbsorbed, eEditorValueRange.NonAction, "Max Damage Absorbed:");
                EditBool(ref e.SetMaxShieldResource, "Set Granted Resource as Max (shield-exclusive resources)");

                EditFloat(ref e.DamageMultiplier, "Damage Absorption Multiplier", Space + 50);
                EditFloatDict(e.CategoryMultipliers, "Category-Specific Damage Absorption Multiplier:", BattleData.Categories,
                              ref NewCategoryMultiplier, "", Space, "New Category Multiplier:", Space);

                EditInt(ref e.Priority, "Shield Priority:", Space);

                EditInt(ref e.Limit, "Max Hits Absorbed:", Space);
                EditBool(ref e.EndStatusOnEffectEnd, "End Status when Limit Reached");
                break;
            }
            case Effect.eEffectType.Trigger:
            {
                var e = effect as EffectTrigger;
                if (e == null)
                {
                    return false;
                }

                EditTrigger(e.TriggerData, "Trigger:");
                break;
            }
        }
        EndIndent();
        return true;
    }
    #endregion

    #region Entity Data
    void EditEntityData()
    {
        EditFactions();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        EditEntities();
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
    }

    void EditEntities()
    {

    }

    #region Factions
    void EditFactions()
    {
        ShowFactions = EditorGUILayout.Foldout(ShowFactions, "Entity Factions");
        if (ShowFactions)
        {
            if (ShowHelp)
            {
                EditorGUILayout.HelpBox("Each Entity is part of a faction. The lists of friendly and enemy Entities help " +
                                        "determine which Entities can be affected by Actions used by an Entity, for example " +
                                        "a healing spell can be set to only affect friendly entities.", MessageType.None);
            }

            StartIndent();
            for (int i = 0; i < Factions.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                Label("Faction: ", 60);

                var oldName = Factions[i].Data.FactionID;
                Factions[i].Data.FactionID = GUILayout.TextField(Factions[i].Data.FactionID, GUILayout.Width(200));
                if (Button("Save Changes", 110))
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
                if (Remove())
                {
                    Factions.RemoveAt(i);
                    BattleData.EntityResources.Remove(Factions[i].Data.FactionID);
                }
                GUILayout.EndHorizontal();

                EditFactionList(Factions[i].Data, Factions[i].Data.FriendlyFactions, ref Factions[i].NewFriendlyFaction,
                                "Friendly Factions:", "Add Friendly Faction:", "(No Friendly Factions)");
                EditFactionList(Factions[i].Data, Factions[i].Data.EnemyFactions, ref Factions[i].NewEnemyFaction,
                                "Enemy Factions:", "Add Enemy Faction:", "(No Enemy Factions)");

                GUILayout.BeginHorizontal();
                Label("New Faction: ", 120);
                NewFaction = GUILayout.TextField(NewFaction, GUILayout.Width(200));
                if (Add() && !string.IsNullOrEmpty(NewFaction) &&
                    !BattleData.Factions.ContainsKey(NewFaction))
                {
                    var newFactionData = new FactionData(NewFaction);
                    Factions.Add(new EditorFaction(newFactionData));
                    BattleData.Factions.Add(NewFaction, newFactionData);
                    NewFaction = "";
                }
                GUILayout.EndHorizontal();
            }
            EndIndent();
        }
    }

    void EditFactionList(FactionData faction, List<string> factions, ref string newFaction, string label = "", string addLabel = "", string noLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        if (factions.Count > 0)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Label($"• {factions[i]}", 100);
                if (Remove())
                {
                    factions.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            Label(noLabel);
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
                    Label($"{addLabel}", 140);
                }

                var copy = newFaction; // This is needed for the lambda expression to work.
                var index = options.FindIndex(0, a => a.Equals(copy));
                if (index < 0)
                {
                    index = 0;
                }
                newFaction = options[EditorGUILayout.Popup(index, options.ToArray(),
                            GUILayout.Width(70))];

                if (Button("+", 20) && newFaction != null)
                {
                    factions.Add(newFaction);
                }

                GUILayout.EndHorizontal();
            }
        }
        EndIndent();
    }
    #endregion
    #endregion

    #region Components
    #region Action Components
    Dictionary<Action.eActionType, string> ActionHelp = new Dictionary<Action.eActionType, string>()
    {
        [Action.eActionType.ApplyCooldown] = "",
        [Action.eActionType.CollectCost] = "",
        [Action.eActionType.DestroySelf] = "",
        [Action.eActionType.LoopBack] = "",
        [Action.eActionType.Message] = "",
        [Action.eActionType.PayloadArea] = "",
        [Action.eActionType.PayloadDirect] = "",
        [Action.eActionType.SpawnEntity] = "",
        [Action.eActionType.SpawnProjectile] = "",
        [Action.eActionType.SetAnimation] = "",
    };
    bool EditAction(Action action)
    {
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        GUILayout.BeginHorizontal();
        Label($"{action.ActionType} Action", Space);
        var remove = Remove();
        GUILayout.EndHorizontal();

        if (remove)
        {
            return false;
        }

        if (ShowHelp && ActionHelp.ContainsKey(action.ActionType))
        {
            EditorGUILayout.HelpBox(ActionHelp[action.ActionType], MessageType.None);
        }

        StartIndent();

        EditString(ref action.ActionID, "Action ID:", Space);
        EditFloat(ref action.Timestamp, "Timestamp:", Space);

        switch (action.ActionType)
        {
            case Action.eActionType.ApplyCooldown:
            {
                var a = action as ActionCooldown;

                if (a == null)
                {
                    a = new ActionCooldown();
                }

                GUILayout.BeginHorizontal();
                Label("Cooldown:", Space);
                a.Cooldown = EditorGUILayout.FloatField(action.Timestamp, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                EditEnum(ref a.ChangeMode, "Change Mode:", Space, 120);
                EditEnum(ref a.CooldownTarget, "Cooldown Target:", Space, 120);

                GUILayout.BeginHorizontal();
                if (a.CooldownTarget == ActionCooldown.eCooldownTarget.Skill)
                {
                    SelectSkill(ref a.CooldownTargetName, "Cooldown Skill ID:", Space);
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
                var a = action as ActionCostCollection;

                if (a == null)
                {
                    return false;
                }

                SelectResource(ref a.ResourceName, "Resource Collected: ", Space);
                EditEnum(ref a.ValueType, "Value Type: ", Space);
                if (a.ValueType == ActionCostCollection.eCostValueType.FlatValue)
                {
                    EditFloat(ref a.Value, "Amount Collected: ", Space);
                }
                else if (a.ValueType == ActionCostCollection.eCostValueType.CurrentMult || a.ValueType == ActionCostCollection.eCostValueType.MaxMult)
                {
                    GUILayout.BeginHorizontal();
                    EditFloat(ref a.Value, "Amount Collected: ", Space, 70, false);
                    Label($"x {(a.ValueType == ActionCostCollection.eCostValueType.CurrentMult ? "Current" : "Max")} {a.ResourceName.ToUpper()}");
                    GUILayout.EndHorizontal();
                }

                EditBool(ref a.Optional, "Is Optional");

                break;
            }
            case Action.eActionType.DestroySelf:
            {
                var a = action as ActionDestroySelf;

                if (a == null)
                {
                    return false;
                }

                break;
            }
            case Action.eActionType.LoopBack:
            {
                var a = action as ActionLoopBack;

                if (a == null)
                {
                    return false;
                }

                EditFloatSlider(ref a.GoToTimestamp, "Go To Timestamp:", 0.0f, a.Timestamp, Space);
                EditInt(ref a.Loops, "Loops:", Space);

                break;
            }
            case Action.eActionType.Message:
            {
                var a = action as ActionMessage;

                if (a == null)
                {
                    return false; ;
                }

                EditString(ref a.MessageString, "Message Text:", Space, 300);
                EditColor(ref a.MessageColor, "Message Colour:", Space);

                break;
            }
            case Action.eActionType.PayloadArea:
            {
                var a = action as ActionPayloadArea;

                if (a == null)
                {
                    return false;
                }

                EditPayloadAction(a, EditorPayloadAction.GetEditorPayloadAction(a));
                EditAreas(a.AreasAffected, "Areas Affected By Payload:");

                break;
            }
            case Action.eActionType.PayloadDirect:
            {
                var a = action as ActionPayloadDirect;

                if (a == null)
                {
                    return false;
                }

                EditPayloadAction(a, EditorPayloadAction.GetEditorPayloadAction(a));
                EditEnum(ref a.ActionTargets, "Payload Targets:", Space);
                if (a.ActionTargets == ActionPayloadDirect.eDirectActionTargets.TaggedEntity)
                {
                    EditString(ref a.EntityTag, "Target Tag:", Space);
                }

                break;
            }
            case Action.eActionType.SpawnProjectile:
            {
                var a = action as ActionProjectile;

                if (a == null)
                {
                    return false;
                }

                EditActionProjectile(a);

                break;
            }
            case Action.eActionType.SpawnEntity:
            {
                var a = action as ActionSummon;

                if (a == null)
                {
                    return false;
                }

                EditActionSummon(a);

                break;
            }
            case Action.eActionType.SetAnimation:
            {
                var a = action as ActionAnimationSet;

                if (a == null)
                {
                    return false;
                }

                break;
            }
        }

        EditActionConditions(action);
        EndIndent();
        return true;
    }

    #region Summon Actions
    void EditActionSummon(ActionSummon a)
    {
        SelectStringFromList(ref a.EntityID, BattleData.Entities.Keys.ToList(), "Summonned Entity:", Space);

        if (a.SummonAtPosition == null)
        {
            a.SummonAtPosition = new TransformData();
        }
        EditTransform(a.SummonAtPosition, "Summon At Position:");

        EditFloat(ref a.SummonDuration, "Summon Duration:", Space);
        EditInt(ref a.SummonLimit, $"Max Summonned {a.EntityID}s:", Space);

        // Shared attributes
        var options = BattleData.Entities[a.EntityID].BaseAttributes.Keys.Where(k => !a.SharedAttributes.Keys.Contains(k)).ToList();
        EditFloatDict(a.SharedAttributes, "Inherited Attributes:", options, ref NewString, ": ", Space, $"Add Attribute:", Space, true);

        EditBool(ref a.LifeLink, "Kill Entity When Summoner Dies");
        EditBool(ref a.InheritFaction, "Inherit Summoner's Faction");
    }

    void EditActionProjectile(ActionProjectile a)
    {
        EditActionSummon(a);

        var newMode = a.ProjectileMovementMode;
        EditEnum(ref newMode, "Projectile Movement Mode:", 200);
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
        StartIndent();
        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Homing ||
            a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditEnum(ref a.Target, "Moving Toward:", 200);
            if (a.Target == ActionProjectile.eTarget.StaticPosition)
            {
                if (a.TargetPosition == null)
                {
                    a.TargetPosition = new TransformData();
                }
                StartIndent();
                EditTransform(a.TargetPosition, "Position Transform:");
                EndIndent();
            }
        }

        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditFloatSlider(ref a.ArchAngle, "Arch Angle:", 1.0f, 85.0f, 200);
            EditFloat(ref a.Gravity, "Gravity (Affects Speed):", 200);
        }

        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Orbit)
        {
            EditEnum(ref a.Anchor, "Orbit Anchor:", 200);
            if (a.Anchor == ActionProjectile.eAnchor.CustomPosition)
            {
                if (a.AnchorPosition == null)
                {
                    a.AnchorPosition = new TransformData();
                }
                StartIndent();
                EditTransform(a.AnchorPosition, "Position Transform:");
                EndIndent();
            }
        }

        // Projectile Timeline
        if (a.ProjectileMovementMode != ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditProjectileTimeline(a);
        }
        EndIndent();

        // Projectile Triggers
        EditCollisionReactions(a.OnEnemyHit, "On Enemy Hit:", a.SkillID);
        EditCollisionReactions(a.OnFriendHit, "On Friend Hit:", a.SkillID);
        EditCollisionReactions(a.OnTerrainHit, "On Terrain Hit:", a.SkillID);
    }

    void EditCollisionReactions(List<ActionProjectile.OnCollisionReaction> reactions, string label, string skillID = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        if (reactions.Count == 0)
        {
            Label("No Reactions");
        }

        for (int i = 0; i < reactions.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditEnum(ref reactions[i].Reaction, "", makeHorizontal: false);
            var remove = Remove();
            EditorGUILayout.EndHorizontal();

            if (remove)
            {
                reactions.RemoveAt(i);
                i--;
                continue;
            }

            if (reactions[i].Reaction == ActionProjectile.OnCollisionReaction.eReactionType.ExecuteActions)
            {
                EditActionTimeline(reactions[i].Actions, ref NewAction, ref ShowValues, "Projectile Action Timeline:", skillID);
            }
        }

        GUILayout.BeginHorizontal();
        EditEnum(ref NewReaction, "New Collision Reaction: ", 150, 200, false);
        if (Add())
        {
            reactions.Add(new ActionProjectile.OnCollisionReaction(NewReaction));
        }
        EditorGUILayout.EndHorizontal();
        EndIndent();
    }

    void EditProjectileTimeline(ActionProjectile a)
    {
        GUILayout.BeginHorizontal();
        Label("Projectile Timeline", 200);
        if (a.ProjectileTimeline.Count > 1 && Button("Sort"))
        {
            GUI.FocusControl(null);
            a.ProjectileTimeline.Sort((s1, s2) => s1.Timestamp.CompareTo(s2.Timestamp));
        }
        GUILayout.EndHorizontal();

        StartIndent();
        for (int i = 0; i < a.ProjectileTimeline.Count; i++)
        {
            GUILayout.BeginHorizontal();
            Label($"State {i}", 100);
            if (Button("Copy"))
            {
                a.ProjectileTimeline.Add(Copy(a.ProjectileTimeline[i]));
            }

            var remove = Remove();
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
        if (Button("Add Projectile State", 160))
        {
            a.ProjectileTimeline.Add(new ActionProjectile.ProjectileState(a.ProjectileTimeline.Count > 0 ?
                                     a.ProjectileTimeline[a.ProjectileTimeline.Count - 1].Timestamp : 0.0f));
        }
        GUILayout.EndHorizontal();
        EndIndent();
    }

    void EditProjectileTimelineState(ActionProjectile.ProjectileState state, ActionProjectile.eProjectileMovementMode mode)
    {
        StartIndent();
        EditFloat(ref state.Timestamp, "Timestamp:", 80);
        EditVector2(ref state.SpeedMultiplier, "Speed (Random between X and Y):");
        EditVector2(ref state.RotationPerSecond, "Rotation Speed (Random between X and Y):");
        if (mode == ActionProjectile.eProjectileMovementMode.Free)
        {
            EditVector2(ref state.RotationY, "Rotate By Degrees (Random between X and Y):");
        }
        EndIndent();
    }   
    #endregion

    void EditActionTimeline(ActionTimeline timeline, ref Action.eActionType newAction, ref bool showTimeline, string title = "", string skillID = "")
    {
        showTimeline = EditorGUILayout.Foldout(showTimeline, title);
        if (showTimeline)
        {
            if (timeline == null)
            {
                timeline = new ActionTimeline();
            }

            if (timeline.Count > 1 && Button("Sort", 90))
            {
                GUI.FocusControl(null);
                timeline.Sort((a1, a2) => a1.Timestamp.CompareTo(a2.Timestamp));
            }

            for (int i = 0; i < timeline.Count; i++)
            {
                // Show and edit an action and check if remove button was pressed.
                var remove = !EditAction(timeline[i]);
                if (remove)
                {
                    timeline.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            EditorDrawLine();

            // New action
            GUILayout.BeginHorizontal();
            EditEnum(ref newAction, "New Action: ", 188, 200, false);
            if (Add())
            {
                var a = Action.MakeAction(newAction);
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

            if (Button("Hide Timeline", 120))
            {
                showTimeline = false;
            }
            EditorDrawLine();
        }
    }

    void EditActionConditions(Action action)
    {
        if (action.ActionConditions == null)
        {
            action.ActionConditions = new List<ActionCondition>();
        }

        Label("Action Conditions:");
        StartIndent();

        if (action.ActionConditions.Count == 0)
        {
            Label("No Action Conditions");
        }

        for (int i = 0; i < action.ActionConditions.Count; i++)
        {
            var condition = action.ActionConditions[i];

            GUILayout.BeginHorizontal();
            EditEnum(ref condition.Condition, "Condition:", Space, makeHorizontal: false);
            var remove = Remove();
            GUILayout.EndHorizontal();

            if (remove)
            {
                action.ActionConditions.RemoveAt(i);
                i--;
                continue;
            }

            StartIndent();
            if (condition.Condition == ActionCondition.eActionCondition.ValueBelow ||
                condition.Condition == ActionCondition.eActionCondition.ValueAbove)
            {
                EditEnum(ref condition.ConditionValueType, "Value Type:", Space);
                GUILayout.BeginHorizontal();
                var text = (condition.Condition == ActionCondition.eActionCondition.ValueBelow ? " <" : " >");

                switch(condition.ConditionValueType)
                {
                    case ActionCondition.eConditionValueType.ResourceRatio:
                    {
                        SelectResource(ref condition.ConditionTarget, "", makeHorizontal: false);
                        EditFloatSlider(ref condition.ConditionValueBoundary, text, 0.0f, 1.0f, 40, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.ResourceCurrent:
                    {
                        SelectResource(ref condition.ConditionTarget, "", makeHorizontal: false);
                        EditFloat(ref condition.ConditionValueBoundary, text, 40, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.ChargeRatio:
                    {
                        EditFloatSlider(ref condition.ConditionValueBoundary, "Skill Charge Ratio" + text, 0.0f, 1.0f, 120, makeHorizontal: false);
                        break;
                    }
                    case ActionCondition.eConditionValueType.RandomValue:
                    {
                        EditFloatSlider(ref condition.ConditionValueBoundary, "Random Value" + text, 0.0f, 1.0f, 120, makeHorizontal: false);
                        break;
                    }
                }

                GUILayout.EndHorizontal();
            }
            else if (condition.Condition == ActionCondition.eActionCondition.ActionSuccess || 
                     condition.Condition == ActionCondition.eActionCondition.ActionSuccess)
            {
                EditString(ref condition.ConditionTarget, "Action:", Space);
            }
            else if (condition.Condition == ActionCondition.eActionCondition.HasStatus)
            {
                SelectStatus(ref condition.ConditionTarget, "Status Effect", Space);
                EditInt(ref condition.MinStatusStacks, "Min Stacks:", Space);
            }
            EndIndent();
        }

        GUILayout.BeginHorizontal();
        EditEnum(ref NewActionCondition, "New Action Condition:", Space, makeHorizontal: false);
        if (Add())
        {
            action.ActionConditions.Add(new ActionCondition(NewActionCondition));
        }
        GUILayout.EndHorizontal();
        EndIndent();
    }

    void EditArea(ActionPayloadArea.Area area)
    {
        StartIndent();
        var newShape = area.Shape;
        EditEnum(ref newShape, "Shape:", 150);
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
            EditFloat(ref area.Dimensions.x, "Radius:", 100);
            EditFloat(ref area.InnerDimensions.x, "Inner Radius:", 100);

            EditFloatSlider(ref area.Dimensions.y, "Cone Angle:", 0.0f, 360.0f, 100);
            EditFloatSlider(ref area.InnerDimensions.y, "Inner Cone Angle:", 0.0f, 360.0f, 100);
        }
        if (area.Shape == ActionPayloadArea.Area.eShape.Cube)
        {
            EditVector3(area.Dimensions, "Dimensions:");
            var inner = new Vector3(area.InnerDimensions.x, 0.0f, area.InnerDimensions.y);
            EditVector3(inner, "Inner Dimensions:");
            area.InnerDimensions.x = inner.x;
            area.InnerDimensions.y = inner.z;
        }
        if (area.Shape == ActionPayloadArea.Area.eShape.Cube || area.Shape == ActionPayloadArea.Area.eShape.Cylinder)
        {
            EditFloat(ref area.Dimensions.z, "Height:", 100);
        }

        if (area.AreaTransform == null)
        {
            area.AreaTransform = new TransformData();
        }
        EditTransform(area.AreaTransform, "Area Transform:");
        EndIndent();
    }

    void EditAreas(List<ActionPayloadArea.Area> areas, string label)
    {
        Label(label);

        StartIndent();
        for (int i = 0; i < areas.Count; i++)
        {
            GUILayout.BeginHorizontal();
            Label($"Area [{i}]", 50);

            if (Remove())
            {
                areas.RemoveAt(i);
                i--;
                continue;
            }
            GUILayout.EndHorizontal();

            EditArea(areas[i]);
        }

        GUILayout.BeginHorizontal();
        if (Button("Add Area", 90))
        {
            areas.Add(new ActionPayloadArea.Area());
        }
        GUILayout.EndHorizontal();
        StartIndent();
    }

    void EditTransform(TransformData transform, string label)
    {
        Label(label);

        StartIndent();
        EditEnum(ref transform.PositionOrigin, "Transform Position Origin:", 200);
        EditEnum(ref transform.ForwardSource, "Transform Forward Source:", 200);
        if (transform.PositionOrigin == TransformData.ePositionOrigin.TaggedEntityPosition ||
            transform.ForwardSource == TransformData.eForwardSource.TaggedEntityForward)
        {
            EditString(ref transform.EntityTag, "Entity Tag:", 200 * 12);
            EditEnum(ref transform.TaggedTargetPriority, "Tagged Entity Priority:", 200);
        }
        EditVector3(transform.PositionOffset, "Position Offset: ");
        EditVector3(transform.RandomPositionOffset, "Random Position Offset: ");

        EditFloat(ref transform.ForwardRotationOffset, "Rotation Offset: ", 200);
        EditFloat(ref transform.RandomForwardOffset, "Random Rotation Offset: ", 200);
        EndIndent();
    }

    #endregion

    #region List Components

    void SelectStringFromList(ref string value, List<string> list, string label, int labelWidth = 150, int inputWidth = 200, bool makeHorizontal = true)
    {
        if (list.Count == 0)
        {
            Label("List is empty!");
            return;
        }

        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var copy = value; // Copy the string to use it in a lambda expression
        var index = list.FindIndex(0, v => v.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        value = list[EditorGUILayout.Popup(index, list.ToArray(),
                   GUILayout.Width(inputWidth))];
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void SelectAttribute(ref string attribute, bool showLabel = true)
    {
        if (BattleData.EntityAttributes.Count == 0)
        {
            Label("No Attributes!");
            return;
        }

        if (showLabel)
        {
            Label("Attribute:", 60);
        }

        var attributeCopy = attribute; // Copy the string to use it in a lambda expression

        var index = BattleData.EntityAttributes.FindIndex(0, a => a.Equals(attributeCopy));
        if (index < 0)
        {
            index = 0;
        }
        attribute = BattleData.EntityAttributes[EditorGUILayout.Popup(index, BattleData.EntityAttributes.ToArray(),
                    GUILayout.Width(60))];
    }

    void SelectCategory(ref string category, bool showLabel = true)
    {
        var categories = BattleData.Categories;
        if (categories.Count == 0)
        {
            Label("No categories!");
            return;
        }

        if (showLabel)
        {
            Label("Category:", 52);
        }
        var categoryCopy = category; // Copy the string to use it in a lambda expression
        var index = categories.FindIndex(0, a => a.Equals(categoryCopy));
        if (index < 0)
        {
            index = 0;
        }
        category = categories[EditorGUILayout.Popup(index, categories.ToArray(), GUILayout.Width(60))];
    }

    void SelectFaction(ref string faction, bool showLabel = true)
    {
        if (BattleData.Factions.Count == 0)
        {
            Label("No Factions!");
            return;
        }

        if (showLabel)
        {
            Label("Faction:", 52);
        }
        var factionCopy = faction; // Copy the string to use it in a lambda expression
        var options = BattleData.Factions.Keys.ToList();

        if (options.Count > 0)
        {
            var index = options.FindIndex(0, a => a.Equals(factionCopy));
            if (index < 0)
            {
                index = 0;
            }
            faction = options[EditorGUILayout.Popup(index, options.ToArray(),
                      GUILayout.Width(60))];
        }
    }

    void SelectResource(ref string resource, string label = "", int labelWidth = 60, bool makeHorizontal = true)
    {
        var resources = BattleData.EntityResources.Keys.ToList();

        if (resources.Count == 0)
        {
            Label("No Resources!");
            return;
        }

        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var resourceCopy = resource; // Copy the string to use it in a lambda expression

        var index = resources.FindIndex(0, r => r.Equals(resourceCopy));
        if (index < 0)
        {
            index = 0;
        }

        resource = resources[EditorGUILayout.Popup(index, resources.ToArray(),
                   GUILayout.Width(90))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }
    #endregion

    #region Payload Components
    void EditPayload(PayloadData payload, EditorPayload editorPayload, bool isSkill)
    {
        StartIndent();
        EditListString(ref editorPayload.NewCategory, payload.Categories, Utility.CopyList(BattleData.Categories),
                       "Payload Categories: ", "No Payload Categories", "Add Payload Category:");

        EditValue(payload.PayloadValue, isSkill ? eEditorValueRange.SkillAction : eEditorValueRange.NonAction, "Payload Damage:");
        SelectResource(ref payload.ResourceAffected, "Resource Affected: ", 150);

        EditListString(ref editorPayload.NewFlag, payload.Flags, Utility.CopyList(BattleData.PayloadFlags),
                       "Payload Flags: ", "No Payload Flags", "Add Payload Flag:");

        // Tag
        var tag = payload.Tag != null;
        EditBool(ref tag, "Tag");
        if (tag)
        {
            if (payload.Tag == null)
            {
                payload.Tag = new TagData();
            }
            StartIndent();
            EditString(ref payload.Tag.TagID, "Tag ID: ", 150, 150);
            EditInt(ref payload.Tag.TagLimit, "Max tagged: ", 150);
            EditFloat(ref payload.Tag.TagDuration, "Tag Duration: ", 150);
            EndIndent();
        }
        else
        {
            payload.Tag = null;
        }

        // Statuses
        if (payload.ApplyStatus == null)
        {
            payload.ApplyStatus = new List<(string StatusID, int Stacks)>();
        }
        EditStatusStackList(payload.ApplyStatus, ref editorPayload.NewStatusApply,
                            "Applied Status Effects:", "No Status Effects", "Add Status Effect:");
        if (payload.RemoveStatusStacks == null)
        {
            payload.RemoveStatusStacks = new List<(string StatusID, int Stacks)>();
        }
        EditStatusStackList(payload.RemoveStatusStacks, ref editorPayload.NewStatusRemoveStack,
                    "Removed Status Effects:", "No Status Effects", "Add Status Effect:");
        if (payload.ClearStatus == null)
        {
            payload.ClearStatus = new List<(bool StatusGroup, string StatusID)>();
        }
        EditStatusList(payload.ClearStatus, ref editorPayload.NewStatusClear,
                "Cleared Status Effects:", "No Status Effects", "Add:");

        // Kill/revive
        EditBool(ref payload.Instakill, "Instakill");
        if (payload.Instakill)
        {
            payload.Revive = false;
        }

        EditBool(ref payload.Revive, "Revive");
        if (payload.Revive)
        {
            payload.Instakill = false;
        }

        EditFloatSlider(ref payload.SuccessChance, "Success Chance: ", 0.0f, 1.0f, 120, 150);

        // Payload conditions
        var hasPayloadCondition = payload.PayloadCondition != null;
        EditBool(ref hasPayloadCondition, "Payload Condition");
        if (hasPayloadCondition)
        {
            if (payload.PayloadCondition == null)
            {
                payload.PayloadCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            EditPayloadCondition(payload.PayloadCondition);

            // Alternate payload
            var hasAlternatePayload = editorPayload.AlternatePayload != null;
            EditBool(ref hasAlternatePayload, "Alternate Payload");
            if (hasAlternatePayload)
            {
                if (editorPayload.AlternatePayload == null)
                {
                    var newPayload = new PayloadData();
                    payload.AlternatePayload = newPayload;
                    editorPayload.AlternatePayload = new EditorPayload();
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
        EndIndent();
    }

    void EditPayloadAction(ActionPayload action, EditorPayloadAction editorAction)
    {
        EditEnum(ref action.Target, "Targets Affected: ", Space);
        EditEnum(ref action.TargetState, "Required Target State: ", Space);

        if (editorAction == null)
        {
            EditorPayloadAction.AddPayloadAction(action);
            return;
        }

        EditPayloadList(action.PayloadData, ref editorAction.ShowPayloads, editorAction, "Payloads:", action);

        EditInt(ref action.TargetLimit, "Max Targets Affected:", Space);
        if (action.TargetLimit > 0)
        {
            EditEnum(ref action.TargetPriority, "Target Priority: ", Space);
            if (action.TargetPriority == ActionPayload.eTargetPriority.ResourceCurrentHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceCurrentLowest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceMaxHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceMaxLowest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceRatioHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceRatioLowest)
            {
                StartIndent();
                SelectResource(ref action.Resource, "Resource: ", Space);
                EndIndent();
            }
        }
    }

    void EditPayloadList(List<PayloadData> payloadData, ref bool show, List<EditorPayload> editorPayloads, string label, Action action = null)
    {
        show = EditorGUILayout.Foldout(show, label);
        if (show)
        {
            for (int i = 0; i < payloadData.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

                StartIndent();
                GUILayout.BeginHorizontal();
                Label($"Payload {i}", 100);
                if (Button("Remove Payload", Space))
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
                }

                while (editorPayloads.Count < payloadData.Count)
                {
                    editorPayloads.Add(new EditorPayload());
                }

                var isSkill = action != null && !string.IsNullOrEmpty(action.SkillID);
                EditPayload(payloadData[i], editorPayloads[i], isSkill);
                EndIndent();
            }

            // New payload
            GUILayout.BeginHorizontal();
            if (Button("Add New Payload", Space))
            {
                var newPayload = new PayloadData();
                payloadData.Add(newPayload);
                if (action != null)
                {
                    EditorPayloads[EditorPayloadAction.ActionKey(action)].Add(new EditorPayload());
                }
            }
            GUILayout.EndHorizontal();

            if (payloadData.Count > 0 && Button("Hide Payloads", 120))
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
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

                StartIndent();
                GUILayout.BeginHorizontal();
                Label($"Payload {i}", 100);
                if (Button("Remove Payload", Space))
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

                while (editorPayloads.Count < list.Count)
                {
                    editorPayloads.Add(new EditorPayload());
                }

                var payload = list[i].Item1;
                var floatValue = list[i].Item2;

                StartIndent();
                EditFloat(ref floatValue, floatFieldLabel, Space);
                EndIndent();
                EditPayload(payload, editorPayloads[i], false);

                list[i] = (payload, floatValue);

                EndIndent();
            }

            // New payload
            GUILayout.BeginHorizontal();
            if (Button("Add New Payload", Space))
            {
                var newPayload = new PayloadData();
                list.Add((newPayload, 1.0f));
            }
            GUILayout.EndHorizontal();

            if (list.Count > 0 && Button("Hide Payloads", 120))
            {
                show = false;
            }
        }
    }

    void EditPayloadCondition(PayloadCondition condition)
    {
        var newConditionType = condition.ConditionType;
        EditEnum(ref newConditionType, "Condition Type:");
        EditBool(ref condition.ExpectedResult, "Required Result: " + (condition.ExpectedResult ? "Success" : "Failure"));

        if (newConditionType != condition.ConditionType)
        {
            condition.SetCondition(newConditionType);
        }

        switch (condition.ConditionType)
        {
            case PayloadCondition.ePayloadConditionType.AngleBetweenDirections:
            {
                EditEnum(ref condition.Direction1, "Direction 1:", 100);
                EditEnum(ref condition.Direction2, "Direction 2:", 100);

                EditFloatSlider(ref condition.Range.x, "Angle Min:", 0.0f, 100);
                EditFloatSlider(ref condition.Range.y, "Angle Max:", 0.0f, 100);
                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetHasStatus:
            {
                SelectStatus(ref condition.StatusID, "Status Effect:", makeHorizontal: true);
                if (BattleData.StatusEffects.ContainsKey(condition.StatusID))
                {
                    EditIntSlider(ref condition.MinStatusStacks, "Min. Status Stacks:", 1, BattleData.StatusEffects[condition.StatusID].MaxStacks);
                }
                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetWithinDistance:
            {
                EditFloat(ref condition.Range.x, "Distance Min:");
                EditFloat(ref condition.Range.y, "Distance Max:");

                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetResourceRatioWithinRange:
            {
                EditFloatSlider(ref condition.Range.x, "Distance Min:", 0.0f, 1.0f);
                EditFloatSlider(ref condition.Range.y, "Distance Max:", 0.0f, 1.0f);

                break;
            }
        }

        var hasAndCondition = condition.AndCondition != null;
        EditBool(ref hasAndCondition, "AND Condition");
        if (hasAndCondition)
        {
            if (condition.AndCondition == null)
            {
                condition.AndCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            StartIndent();
            EditPayloadCondition(condition.AndCondition);
            EndIndent();
        }
        else
        {
            condition.AndCondition = null;
        }

        var hasOrCondition = condition.OrCondition != null;
        EditBool(ref hasOrCondition, "OR Condition");
        if (hasOrCondition)
        {
            if (condition.OrCondition == null)
            {
                condition.OrCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            StartIndent();
            EditPayloadCondition(condition.OrCondition);
            EndIndent();
        }
        else
        {
            condition.OrCondition = null;
        }
    }
    #endregion

    #region Triggers
    bool EditTrigger(TriggerData trigger, string label, bool removable = false)
    {
        if (trigger == null)
        {
            trigger = new TriggerData();
        }

        GUILayout.BeginHorizontal();
        Label(label, Space);
        var remove = (removable && Remove());
        GUILayout.EndHorizontal();

        if (remove)
        {
            return false;
        }

        StartIndent();
        EditEnum(ref trigger.Trigger, "Trigger:", Space);

        EditTriggerConditions(trigger.Trigger, trigger.Conditions, "Trigger Conditions:", ref ShowTriggerConditions);

        EditFloat(ref trigger.Cooldown, "Trigger Cooldown", Space);
        EditInt(ref trigger.Limit, "Trigger Limit", Space);

        EditFloatSlider(ref trigger.TriggerChance, "Trigger Chance:", 0.0f, 0.1f, Space);

        EditActionTimeline(trigger.Actions, ref NewAction, ref ShowValues, "On Trigger:");

        EndIndent();
        return true;
    }

    void EditTriggerConditions(TriggerData.eTrigger trigger, List<TriggerData.TriggerCondition> conditions, string label, ref bool show)
    {
        if (conditions == null)
        {
            conditions = new List<TriggerData.TriggerCondition>();
        }

        show = EditorGUILayout.Foldout(show, label);

        StartIndent();
        for (int i = 0; i < conditions.Count; i++)
        {
            var remove = !EditTriggerCondition(conditions[i], trigger, $"Condition {i}");
            if (remove)
            {
                conditions.RemoveAt(i);
                i--;
            }
        }

        if (Add())
        {
            conditions.Add(new TriggerData.TriggerCondition());
        }
        EndIndent();
    }

    bool EditTriggerCondition(TriggerData.TriggerCondition condition, TriggerData.eTrigger trigger, string label = "", bool removable = true)
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        if (removable && Remove())
        {
            return false;
        }

        StartIndent();
        EditEnum(ref condition.ConditionType, condition.AvailableConditions(trigger), "Condition:");
        EditBool(ref condition.DesiredOutcome, $"Desired Outcome: {(condition.DesiredOutcome ? "Success" : "Fail")}");

        switch (condition.ConditionType)
        {
            case TriggerData.TriggerCondition.eConditionType.CausedBySkill:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.Skills.Keys.ToList(), "Skill: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedBySkillGroup:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.SkillGroups.Keys.ToList(), "Skill Group: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedByAction:
            {
                EditString(ref condition.StringValue, "Action ID:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.PayloadCategory:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.Categories, "Category: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.PayloadFlag:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.PayloadFlags, "Payload Flag: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.ResultFlag:
            {
                EditString(ref condition.StringValue, "Result Flag:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedByStatus:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Effect: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.CausedByStatusGroup:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Group: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.ResourceAffected:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntityResourceMin:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                EditFloat(ref condition.FloatValue, "Min Value:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceResourceMin:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                EditFloat(ref condition.FloatValue, "Min Value:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntityResourceRatioMin:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                EditFloatSlider(ref condition.FloatValue, "Min Ratio:", 0.0f, 0.1f, Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceResourceRatioMin:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.EntityResources.Keys.ToList(), "Resource: ", Space);
                EditFloatSlider(ref condition.FloatValue, "Min Ratio:", 0.0f, 0.1f, Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.PayloadResultMin:
            {
                EditFloat(ref condition.FloatValue, "Min Resource Change:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.ActionResultMin:
            {
                EditFloat(ref condition.FloatValue, "Min Action Result:", Space);
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.NumTargetsAffectedMin:
            {
                EditInt(ref condition.IntValue, "Min Targets Affected:");
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.EntityHasStatus:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Effect: ", Space);
                EditInt(ref condition.IntValue, "Min Stacks:");
                break;
            }
            case TriggerData.TriggerCondition.eConditionType.TriggerSourceHasStatus:
            {
                SelectStringFromList(ref condition.StringValue, BattleData.StatusEffects.Keys.ToList(), "Status Effect: ", Space);
                EditInt(ref condition.IntValue, "Min Stacks:");
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
        }

        var hasAndCondition = condition.AndCondition != null;
        EditBool(ref hasAndCondition, "AND Condition");
        if (hasAndCondition)
        {
            if (condition.AndCondition == null)
            {
                condition.AndCondition = new TriggerData.TriggerCondition();
            }
            EditTriggerCondition(condition.AndCondition, trigger);
        }
        else
        {
            condition.AndCondition = null;
        }

        var hasOrCondition = condition.OrCondition != null;
        EditBool(ref hasOrCondition, "OR Condition");
        if (hasOrCondition)
        {
            if (condition.OrCondition == null)
            {
                condition.OrCondition = new TriggerData.TriggerCondition();
            }
            EditTriggerCondition(condition.OrCondition, trigger);
        }
        else
        {
            condition.OrCondition = null;
        }

        EndIndent();
        return true;
    }
    #endregion

    #region Skill Components
    void SelectSkill(ref string skill, string label = "", int labelWidth = 60)
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var copy = skill;
        var skills = BattleData.Skills.Keys.ToList();
        var index = skills.FindIndex(0, s => s.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        skill = skills[EditorGUILayout.Popup(index, skills.ToArray(),
                GUILayout.Width(250))];
    }

    void EditSkillGroup(ref string skillGroup, string label = "", int labelWidth = 60)
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
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
        EditFloat(ref data.RequiredChargeTime, "Required Skill Charge Time:", 150);
        EditFloat(ref data.FullChargeTime, "Full Skill Charge Time:", 150);

        EditBool(ref data.MovementCancelsCharge, "Movement Cancels Charge");

        EditActionTimeline(data.PreChargeTimeline, ref newChargeAction, ref showTimeline, "Charge Timeline:", skillID: skillID);

        EditBool(ref data.ShowUI, "Show Skill Charge UI");
    }
    #endregion

    #region Status Components
    void SelectStatus(ref string status, string label = "", int labelWidth = 200, int inputWidth = 200, bool makeHorizontal = false)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var copy = status;
        var options = BattleData.StatusEffects.Keys.ToList();
        var index = options.FindIndex(0, s => s.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        status = options[EditorGUILayout.Popup(index, options.ToArray(),
                 GUILayout.Width(inputWidth))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditStatusGroup(ref string statusGroup, string label = "", int labelWidth = 200, int inputWidth = 150)
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
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
            Label(label);
        }

        StartIndent();
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
                    SelectStatus(ref status);
                }
                else
                {
                    EditStatusGroup(ref status);
                }

                list[i] = (isGroup == 1, status);

                if (Remove())
                {
                    list.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            Label(noLabel);
        }

        GUILayout.BeginHorizontal();
        if (!string.IsNullOrEmpty(addLabel))
        {
            Label(addLabel, addLabel.Count() * 8);
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

        if (Button("+", 20))
        {
            list.Add(newElement);
        }

        GUILayout.EndHorizontal();
        EndIndent();
    }

    void EditStatusStackList(List<(string, int)> list, ref string newElement,
                             string label = "", string noLabel = "", string addLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var status = list[i].Item1;
                var stacks = list[i].Item2;

                GUILayout.BeginHorizontal();
                SelectStatus(ref status, "Status Effect:", 90);
                EditInt(ref stacks, "Stacks:", 50, makeHorizontal: false);

                stacks = Mathf.Min(stacks, BattleData.StatusEffects[status].MaxStacks);

                list[i] = (status, stacks);

                if (Remove())
                {
                    list.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            Label(noLabel);
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
                Label(addLabel, addLabel.Count() * 8);
            }

            var copy = newElement; // This is needed for the lambda expression to work.
            var index = options.FindIndex(0, a => a.Equals(copy));
            if (index < 0)
            {
                index = 0;
            }
            newElement = options[EditorGUILayout.Popup(index, options.ToArray(),
                         GUILayout.Width(200))];

            if (Button("+", 20) && newElement != null)
            {
                list.Add((newElement, 1));
            }

            GUILayout.EndHorizontal();
        }
        EndIndent();
    }
    #endregion

    enum eEditorValueRange
    {
        SkillAction = -1,
        Resource = 3,
        NonAction = 7
    }

    void EditValue(Value v, eEditorValueRange valueRange, string label = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        for (int i = 0; i < v.Count; i++)
        {
            GUILayout.BeginHorizontal();
            Label("Component Type:", 120);

            string[] options = Utility.EnumStrings<ValueComponent.eValueComponentType>((int)valueRange);

            var newComponentType = (ValueComponent.eValueComponentType)EditorGUILayout.Popup((int)v[i].ComponentType,
                                  options, GUILayout.Width(160));

            if (v[i].ComponentType != newComponentType)
            {
                if (newComponentType == ValueComponent.eValueComponentType.ActionResultValue)
                {
                    v[i].Attribute = "";
                }
                v[i].ComponentType = newComponentType;
            }

            Label(" Value:", 43);
            v[i].Potency = EditorGUILayout.FloatField(v[i].Potency, GUILayout.Width(70));
            if (BattleData.EntityAttributes.Count > 0 && (v[i].ComponentType == ValueComponent.eValueComponentType.CasterAttributeBase ||
                v[i].ComponentType == ValueComponent.eValueComponentType.CasterAttributeCurrent))
            {
                Label("x", 10);
                SelectAttribute(ref v[i].Attribute, false);
            }
            else if (BattleData.EntityResources.Count > 0 && v[i].ComponentType == ValueComponent.eValueComponentType.CasterResourceCurrent ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.CasterResourceMax ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.TargetResourceMax)
            {
                Label("x", 10);
                SelectResource(ref v[i].Attribute, makeHorizontal: false);
            }
            else if (v[i].ComponentType == ValueComponent.eValueComponentType.ActionResultValue)
            {
                Label("x Action ID:", 68);
                v[i].Attribute = GUILayout.TextField(v[i].Attribute, GUILayout.Width(190));
            }

            if (Button("Copy", 70))
            {
                v.Add(v[i].Copy);
            }
            if (Remove())
            {
                v.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }

        if (Button("Add Component", 120))
        {
            v.Add(new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, EntityAttributes[0]));
        }
        EndIndent();
    }
    #endregion

    #region Utility
    bool Add()
    {
        return GUILayout.Button("Add", GUILayout.Width(90));
    }

    bool Remove()
    {
        return GUILayout.Button("Remove", GUILayout.Width(90));
    }

    bool Rename()
    {
        return GUILayout.Button("Rename", GUILayout.Width(90));
    }

    bool Button(string text, int width = 90)
    {
        return GUILayout.Button(text, GUILayout.Width(width));
    }

    void EditBool(ref bool value, string label)
    {
        GUILayout.BeginHorizontal();
        value = EditorGUILayout.Toggle(value, GUILayout.Width(12));
        Label(label);
        GUILayout.EndHorizontal();
    }

    void EditColor(ref Color color, string label, int labelWidth = 200, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        Label(label, labelWidth);
        color = EditorGUILayout.ColorField(color, GUILayout.Width(40));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditEnum<T>(ref T value, string label = "", int labelWidth = 150, int enumWidth = 250, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var enumValues = Utility.EnumValues<T>();
        var enumStrings = Utility.EnumStrings<T>();

        var copy = value; // Copy the value to use it in the lambda.
        var index = enumValues.FindIndex(0, v => v.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }

        value = enumValues[EditorGUILayout.Popup(index, enumStrings,
                           GUILayout.Width(enumWidth))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditEnum<T>(ref T value, List<T> options, string label = "", int labelWidth = 150, int enumWidth = 250, 
                     bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var copy = value; // Copy the value to use it in the lambda.
        var index = options.FindIndex(0, v => v.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }

        value = options[EditorGUILayout.Popup(index, Utility.EnumStrings(options),
                   GUILayout.Width(enumWidth))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditFloat(ref float value, string label, int labelWidth = 200, int inputWidth = 70, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        Label(label, labelWidth);
        value = EditorGUILayout.FloatField(value, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditFloatSlider(ref float value, string label, float min = 0.0f, float max = 1.0f, int labelWidth = 200, int inputWidth = 250, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        Label(label, labelWidth);
        value = EditorGUILayout.Slider(value, min, max, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditFloatDict(Dictionary<string, float> dictionary, string label, List<string> options, ref string newElement,
                             string elementLabel, int elementLabelWidth, string addLabel, int addWidth,
                             bool slider = false, float min = 0.0f, float max = 1.0f)
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        var keys = dictionary.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            GUILayout.BeginHorizontal();
            var value = dictionary[keys[i]];
            if (slider)
            {
                EditFloatSlider(ref value, $"{keys[i]}{elementLabel}", min, max, elementLabelWidth, makeHorizontal: false);
            }
            else
            {
                EditFloat(ref value, $"{keys[i]}{elementLabel}", elementLabelWidth, makeHorizontal: false);
            }
            dictionary[keys[i]] = value;

            if (Remove())
            {
                dictionary.Remove(keys[i]);
                keys.RemoveAt(i);
                i--;
            }
            GUILayout.EndHorizontal();
        }

        if (options.Count > 0)
        {
            GUILayout.BeginHorizontal();
            SelectStringFromList(ref newElement, options, addLabel, addWidth, 150, false);
            if (Add())
            {
                dictionary.Add(newElement, max);
            }
            GUILayout.EndHorizontal();
        }
        EndIndent();
    }

    void EditInt(ref int value, string label, int labelWidth = 200, int inputWidth = 70, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        Label(label, labelWidth);
        value = EditorGUILayout.IntField(value, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditIntSlider(ref int value, string label, int min = 0, int max = 180, int labelWidth = 200, int inputWidth = 150)
    {
        GUILayout.BeginHorizontal();
        Label(label, labelWidth);
        value = EditorGUILayout.IntSlider(value, min, max, GUILayout.Width(inputWidth));
        GUILayout.EndHorizontal();
    }

    void Label(string label)
    {
        GUILayout.Label(label);
    }

    void Label(string label, int width = 0)
    {
        GUILayout.Label(label, GUILayout.Width(width));
    }

    void EditListString(ref string newElement, List<string> list, List<string> options,
                     string label = "", string noLabel = "", string addLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Label($"• {list[i]}", 200);
                if (Remove())
                {
                    list.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            Label(noLabel);
        }

        for (int i = 0; i < options.Count; i++)
        {
            if (list.Contains(options[i]))
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
                Label(addLabel, addLabel.Count() * 8);
            }

            var copy = newElement; // This is needed for the lambda expression to work.
            var index = options.FindIndex(0, a => a.Equals(copy));
            if (index < 0)
            {
                index = 0;
            }
            newElement = options[EditorGUILayout.Popup(index, options.ToArray(),
                         GUILayout.Width(250))];

            if (Button("+", 20) && newElement != null)
            {
                list.Add(newElement);
            }

            GUILayout.EndHorizontal();
        }
        EndIndent();
    }

    void EditString(ref string value, string label, int labelWidth = 200, int inputWidth = 150, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        Label(label, labelWidth);
        value = GUILayout.TextField(value, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditVector2(ref Vector2 value, string label, int width = 300)
    {
        value = EditorGUILayout.Vector2Field(label, value, GUILayout.Width(width));
    }

    void EditVector3(Vector3 value, string label, int width = 300)
    {
        EditorGUILayout.Vector3Field(label, value, GUILayout.Width(width));
    }

    public static void EditorDrawLine()
    {
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
    }

    public static void EditorDrawLine(Color color, int thickness = 1, int padding = 10)
    {
        Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
        r.height = thickness;
        r.y += padding / 2;
        r.x -= 2;
        r.width += 6;
        EditorGUI.DrawRect(r, color);
    }

    void StartIndent()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        GUILayout.BeginVertical();
    }

    void EndIndent()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    static T Copy<T>(T classObject) where T : new()
    {
        // Use the serializer to perform a deep copy of a class object.
        BattleData.Serializer.TrySerialize(classObject, out var data);

        var newObject = new T();
        BattleData.Serializer.TryDeserialize(data, ref newObject);

        return newObject;
    }
    #endregion
}
