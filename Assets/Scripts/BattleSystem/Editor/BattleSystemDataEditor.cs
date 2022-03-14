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
        public bool Tag = false;
        public string NewStatusApply = "";
        public string NewStatusRemoveStack = "";
        public (bool, string) NewStatusClear = (false, "");
        public bool ShowTimeline = false;
        public EditorPayload AlternatePayload = null;

        public EditorPayload(PayloadData payload)
        {
            Tag = payload.Tag != null;
        }
    }

    class EditorPayloadAction : List<EditorPayload>
    {
        public bool ShowPayloads;

        public EditorPayloadAction(ActionPayload action)
        {
            ShowPayloads = false;

            foreach (var payload in action.PayloadData)
            {
                Add(new EditorPayload(payload));
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
    bool ShowValues;
    ActionProjectile.OnCollisionReaction.eReactionType NewReaction;

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

        public EditorStatusEffect(StatusEffectData data)
        {
            Data = Copy(data);
        }
    }

    bool ShowStatusGroups = false;
    bool ShowStatusEffects = false;

    string NewStatusGroup = "";
    string NewStatusEffect = "";

    List <EditorStatusGroup> StatusGroups = new List<EditorStatusGroup>();
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
        GUILayout.Label("Resources/", GUILayout.Width(70));
        Path = GUILayout.TextField(Path, GUILayout.Width(300f));
        PlayerPrefs.SetString(PathPlayerPrefs, Path);
        GUILayout.Label($".json", GUILayout.Width(30));

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
        GUILayout.Label("", GUILayout.Width(5));
        ShowHelp = EditorGUILayout.Toggle(ShowHelp, GUILayout.Width(12));
        GUILayout.Label("Show Help");
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

        EditFactions();
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

            for (int i = 0; i < EntityAttributes.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(10));
                EntityAttributes[i] = GUILayout.TextField(EntityAttributes[i], GUILayout.Width(203));
                if (Rename())
                {
                    BattleData.EntityAttributes[i] = EntityAttributes[i];
                }
                if (Remove())
                {
                    EntityAttributes.RemoveAt(i);
                    BattleData.EntityAttributes.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(1)}New Attribute: ", GUILayout.Width(100));
            NewAttribute = GUILayout.TextField(NewAttribute, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewAttribute) &&
                !EntityAttributes.Contains(NewAttribute))
            {
                EntityAttributes.Add(NewAttribute);
                BattleData.EntityAttributes.Add(NewAttribute);
                NewAttribute = "";
            }
            GUILayout.EndHorizontal();
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

            for (int i = 0; i < EntityResources.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{Indent(1)}Resource: ", GUILayout.Width(74));
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
                if (Remove())
                {
                    EntityResources.RemoveAt(i);
                    BattleData.EntityResources.Remove(EntityResources[i].Name);
                }
                GUILayout.EndHorizontal();

                EditValue(EntityResources[i].Value, eEditorValueRange.Resource, $"Max { EntityResources[i].Name.ToUpper()} Value:");
            }

            EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

            GUILayout.BeginHorizontal();
            GUILayout.Label("New Resource: ", GUILayout.Width(88));
            NewResource.Name = GUILayout.TextField(NewResource.Name, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewResource.Name) &&
                !BattleData.EntityResources.ContainsKey(NewResource.Name))
            {
                EntityResources.Add(new EditorResource(NewResource.Name, NewResource.Value.Copy()));
                BattleData.EntityResources.Add(NewResource.Name, NewResource.Value.Copy());
                NewResource.Name = "";
            }
            GUILayout.EndHorizontal();
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

            for (int i = 0; i < Categories.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(10));
                Categories[i] = GUILayout.TextField(Categories[i], GUILayout.Width(203));
                if (Rename())
                {
                    BattleData.Categories[i] = Categories[i];
                }
                if (Remove())
                {
                    Categories.RemoveAt(i);
                    BattleData.Categories.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(1)}New Category: ", GUILayout.Width(100));
            NewCategory = GUILayout.TextField(NewCategory, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewCategory) &&
                !BattleData.Categories.Contains(NewCategory))
            {
                Categories.Add(NewCategory);
                BattleData.Categories.Add(NewCategory);
                NewCategory = "";
            }
            GUILayout.EndHorizontal();
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

            for (int i = 0; i < PayloadFlags.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(10));
                PayloadFlags[i] = GUILayout.TextField(PayloadFlags[i], GUILayout.Width(203));
                if (Rename())
                {
                    BattleData.PayloadFlags[i] = PayloadFlags[i];
                }
                if (Remove())
                {
                    PayloadFlags.RemoveAt(i);
                    BattleData.PayloadFlags.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(1)}New Payload Flag: ", GUILayout.Width(120));
            NewPayloadFlag = GUILayout.TextField(NewPayloadFlag, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewPayloadFlag) &&
                !BattleData.PayloadFlags.Contains(NewPayloadFlag))
            {
                PayloadFlags.Add(NewPayloadFlag);
                BattleData.PayloadFlags.Add(NewPayloadFlag);
                NewPayloadFlag = "";
            }
            GUILayout.EndHorizontal();
        }
    }

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

            for (int i = 0; i < Factions.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                GUILayout.Label("Faction: ", GUILayout.Width(60));

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
                GUILayout.Label($"{Indent(1)}New Faction: ", GUILayout.Width(120));
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
        }
    }

    void EditFactionList(FactionData faction, List<string> factions, ref string newFaction, string label = "", string addLabel = "", string noLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(1)}{label}");
        }

        if (factions.Count > 0)
        {
            for (int i = 0; i < factions.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Indent(2);
                GUILayout.Label($"{Indent(2)}• {factions[i]}", GUILayout.Width(100));
                if (Add())
                {
                    factions.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            GUILayout.Label($"{Indent(2)}{noLabel}");
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
                    GUILayout.Label($"{Indent(2)}{addLabel}", GUILayout.Width(140));
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

            for (int i = 0; i < BattleData.SkillGroups.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{Indent(1)}Skill Group: ", GUILayout.Width(80));

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
            GUILayout.Label($"{Indent(1)}New Skill Group: ", GUILayout.Width(128));
            NewSkillGroup = GUILayout.TextField(NewSkillGroup, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewSkillGroup) &&
                !BattleData.SkillGroups.ContainsKey(NewSkillGroup))
            {
                SkillGroups.Add(new EditorSkillGroup(NewSkillGroup));
                BattleData.SkillGroups.Add(NewSkillGroup, new List<string>());
                NewSkillGroup = "";
            }
            GUILayout.EndHorizontal();
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
                GUILayout.Label("", GUILayout.Width(10));
                var skill = Skills[i];
                skill.ShowSkill = EditorGUILayout.Foldout(skill.ShowSkill, skill.SkillData.SkillID);
                GUILayout.EndHorizontal();
                if (skill.ShowSkill)
                {
                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                    GUILayout.BeginHorizontal();

                    // ID
                    GUILayout.Label($"{Indent(2)}Skill ID: ", GUILayout.Width(70));
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
                    EditBool(ref skill.SkillData.Interruptible, "Is Interruptible", 3);

                    EditBool(ref skill.HasChargeTime, "Has Charge Time", 3);
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

                    EditBool(ref skill.SkillData.NeedsTarget, "TargetRequired", 3);
                    EditEnum(ref skill.SkillData.PreferredTarget, $"{Indent(3)}Preferred Target: ");
                    EditFloat(ref skill.SkillData.Range, $"{Indent(3)}Max Distance From Target:", 250, 150);

                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                }
            }

            // New skill
            GUILayout.BeginHorizontal();
            EditString(ref NewSkill, $"{Indent(1)}New Skill: ", 80, 200, false);
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

            for (int i = 0; i < BattleData.StatusEffectGroups.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{Indent(1)}Status Effect Group: ", GUILayout.Width(120));

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
            GUILayout.Label($"{Indent(1)}New Status Effect Group: ", GUILayout.Width(165));
            NewStatusGroup = GUILayout.TextField(NewStatusGroup, GUILayout.Width(200));
            if (Add() && !string.IsNullOrEmpty(NewStatusGroup) &&
                !BattleData.StatusEffectGroups.ContainsKey(NewStatusGroup))
            {
                StatusGroups.Add(new EditorStatusGroup(NewStatusGroup));
                BattleData.StatusEffectGroups.Add(NewStatusGroup, new List<string>());
                NewStatusGroup = "";
            }
            GUILayout.EndHorizontal();
        }
    }

    void EditStatusEffects()
    {

    }
    #endregion

    #region Entity Data
    void EditEntityData()
    {

    }
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
    bool EditAction(Action action, int indent = 4)
    {
        EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

        GUILayout.BeginHorizontal();
        GUILayout.Label($"{Indent(indent)}{action.ActionType} Action", GUILayout.Width(200 + indent * 12));
        if (Remove())
        {
            return false;
        }
        GUILayout.EndHorizontal();

        indent++;

        EditString(ref action.ActionID, $"{Indent(indent)}Action ID:", 200 + indent * 12);
        EditFloat(ref action.Timestamp, $"{Indent(indent)}Timestamp:", 200 + indent * 12);

        if (ShowHelp && ActionHelp.ContainsKey(action.ActionType))
        {
            EditorGUILayout.HelpBox(ActionHelp[action.ActionType], MessageType.None);
        }

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
                GUILayout.Label($"{Indent(indent)}Cooldown:", GUILayout.Width(200 + indent * 12));
                a.Cooldown = EditorGUILayout.FloatField(action.Timestamp, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                EditEnum(ref a.ChangeMode, $"{Indent(indent)}Change Mode:", 200 + indent * 12, 120);
                EditEnum(ref a.CooldownTarget, $"{Indent(indent)}Cooldown Target:", 200 + indent * 12, 120);

                GUILayout.BeginHorizontal();
                if (a.CooldownTarget == ActionCooldown.eCooldownTarget.Skill)
                {
                    SelectSkill(ref a.CooldownTargetName, $"{Indent(indent)}Cooldown Skill ID:", 200 + indent * 12);
                }
                else if (a.CooldownTarget == ActionCooldown.eCooldownTarget.SkillGroup)
                {
                    EditSkillGroup(ref a.CooldownTargetName, $"{Indent(indent)}Cooldown Skill Group Name:", 200 + indent * 12);
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

                SelectResource(ref a.ResourceName, $"{Indent(indent)}Resource Collected: ", 200 + indent * 12);
                EditEnum(ref a.ValueType, $"{Indent(indent)}Value Type: ", 200 + indent * 12);
                if (a.ValueType == ActionCostCollection.eCostValueType.FlatValue)
                {
                    EditFloat(ref a.Value, $"{Indent(indent)}Amount Collected: ", 200 + indent * 12);
                }
                else if (a.ValueType == ActionCostCollection.eCostValueType.CurrentMult || a.ValueType == ActionCostCollection.eCostValueType.MaxMult)
                {
                    GUILayout.BeginHorizontal();
                    EditFloat(ref a.Value, $"{Indent(indent)}Amount Collected: ", 200 + indent * 12, 70, false);
                    GUILayout.Label($"x {(a.ValueType == ActionCostCollection.eCostValueType.CurrentMult ? "Current" : "Max")} {a.ResourceName.ToUpper()}");
                    GUILayout.EndHorizontal();
                }

                EditBool(ref a.Optional, "Is Optional", indent);

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

                EditFloatSlider(ref a.GoToTimestamp, $"{Indent(indent)}Go To Timestamp:", 0.0f, a.Timestamp, 200 + indent * 12);
                EditInt(ref a.Loops, $"{Indent(indent)}Loops:", 200 + indent * 12);

                break;
            }
            case Action.eActionType.Message:
            {
                var a = action as ActionMessage;

                if (a == null)
                {
                    return false; ;
                }

                EditString(ref a.MessageString, $"{Indent(indent)}Message Text:", 200 + indent * 12, 300);
                EditColor(ref a.MessageColor, $"{Indent(indent)}Message Colour:", 200 + indent * 12, 20);

                break;
            }
            case Action.eActionType.PayloadArea:
            {
                var a = action as ActionPayloadArea;

                if (a == null)
                {
                    return false;
                }

                EditPayloadAction(a, EditorPayloadAction.GetEditorPayloadAction(a), indent);
                EditAreas(a.AreasAffected, indent, "Areas Affected By Payload:");

                break;
            }
            case Action.eActionType.PayloadDirect:
            {
                var a = action as ActionPayloadDirect;

                if (a == null)
                {
                    return false;
                }

                EditPayloadAction(a, EditorPayloadAction.GetEditorPayloadAction(a), indent);
                EditEnum(ref a.ActionTargets, $"{Indent(indent)}Payload Targets:", 200 + indent * 12);
                if (a.ActionTargets == ActionPayloadDirect.eDirectActionTargets.TaggedEntity)
                {
                    EditString(ref a.EntityTag, $"{Indent(indent)}Target Tag:", 200 + indent * 12);
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

                EditActionProjectile(a, indent);

                break;
            }
            case Action.eActionType.SpawnEntity:
            {
                var a = action as ActionSummon;

                if (a == null)
                {
                    return false;
                }

                EditActionSummon(a, indent);

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
        return true;
    }

    #region Summon Actions
    void EditActionSummon(ActionSummon a, int indent)
    {
        SelectStringFromList(ref a.EntityID, BattleData.Entities.Keys.ToList(), $"{Indent(indent)}Summonned Entity:", 200 + indent * 12);

        if (a.SummonAtPosition == null)
        {
            a.SummonAtPosition = new TransformData();
        }
        EditTransform(a.SummonAtPosition, indent, "Summon At Position:");

        EditFloat(ref a.SummonDuration, $"{Indent(indent)}Summon Duration:", 200 + indent * 12);
        EditInt(ref a.SummonLimit, $"{Indent(indent)}Max Summonned {a.EntityID}s:", 200 + indent * 12);

        // Shared attributes
        var options = BattleData.Entities[a.EntityID].BaseAttributes.Keys.Where(k => !a.SharedAttributes.Keys.Contains(k)).ToList();
        EditFloatSliderDict(a.SharedAttributes, "Inherited Attributes:", options, ref NewString, ": ", 150, indent, $"Add Attribute:", 150);

        EditBool(ref a.LifeLink, "Kill Entity When Summoner Dies", indent);
        EditBool(ref a.InheritFaction, "Inherit Summoner's Faction", indent);

    }

    void EditActionProjectile(ActionProjectile a, int indent)
    {
        EditActionSummon(a, indent);

        var newMode = a.ProjectileMovementMode;
        EditEnum(ref newMode, $"{Indent(indent)}Projectile Movement Mode:");
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
        indent++;
        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Homing ||
            a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditEnum(ref a.Target, $"{Indent(indent)}Moving Toward:", 200 + indent * 12);
            if (a.Target == ActionProjectile.eTarget.StaticPosition)
            {
                if (a.TargetPosition == null)
                {
                    a.TargetPosition = new TransformData();
                }
                EditTransform(a.TargetPosition, indent + 1, "Position Transform:");
            }
        }

        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditFloatSlider(ref a.ArchAngle, $"{Indent(indent)}Arch Angle:", 1.0f, 85.0f, 200 + indent * 12);
            EditFloat(ref a.Gravity, $"{Indent(indent)}Gravity (Affects Speed):", 200 + indent * 12);
        }

        if (a.ProjectileMovementMode == ActionProjectile.eProjectileMovementMode.Orbit)
        {
            EditEnum(ref a.Anchor, $"{Indent(indent)}Orbit Anchor:", 200 + indent * 12);
            if (a.Anchor == ActionProjectile.eAnchor.CustomPosition)
            {
                if (a.AnchorPosition == null)
                {
                    a.AnchorPosition = new TransformData();
                }
                EditTransform(a.AnchorPosition, indent + 1, "Position Transform:");
            }
        }

        // Projectile Timeline
        if (a.ProjectileMovementMode != ActionProjectile.eProjectileMovementMode.Arched)
        {
            EditProjectileTimeline(a, indent);
        }
        indent--;

        // Projectile Triggers
        EditCollisionReactions(a.OnEnemyHit, "On Enemy Hit:", indent, a.SkillID);
        EditCollisionReactions(a.OnFriendHit, "On Friend Hit:", indent, a.SkillID);
        EditCollisionReactions(a.OnTerrainHit, "On Terrain Hit:", indent, a.SkillID);
    }

    void EditCollisionReactions(List<ActionProjectile.OnCollisionReaction> reactions, string label, int indent, string skillID = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(indent)}{label}");
        }
        indent++;

        if (reactions.Count == 0)
        {
            GUILayout.Label($"{Indent(indent)}No Reactions");
        }

        for (int i = 0; i < reactions.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditEnum(ref reactions[i].Reaction, $" ", indent * 12, makeHorizontal: false);
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
                EditActionTimeline(reactions[i].Actions, ref NewAction, ref ShowValues, "Projectile Action Timeline:", indent, skillID);
            }
        }

        GUILayout.BeginHorizontal();
        EditEnum(ref NewReaction, $"{Indent(indent)}New Collision Reaction: ", 200 + indent * 12, 200, false);
        if (Add())
        {
            reactions.Add(new ActionProjectile.OnCollisionReaction(NewReaction));
        }
        EditorGUILayout.EndHorizontal();
    }

    void EditProjectileTimeline(ActionProjectile a, int indent)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{Indent(indent)}Projectile Timeline", GUILayout.Width(200 + indent * 12));
        if (a.ProjectileTimeline.Count > 1 && Button("Sort"))
        {
            GUI.FocusControl(null);
            a.ProjectileTimeline.Sort((s1, s2) => s1.Timestamp.CompareTo(s2.Timestamp));
        }
        GUILayout.EndHorizontal();

        indent++;
        for (int i = 0; i < a.ProjectileTimeline.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(indent)}State {i}", GUILayout.Width(100 + indent * 12));
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

            EditProjectileTimelineState(a.ProjectileTimeline[i], a.ProjectileMovementMode, indent + 1);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label($"", GUILayout.Width(indent * 12));
        if (Button("Add Projectile State", 160))
        {
            a.ProjectileTimeline.Add(new ActionProjectile.ProjectileState(a.ProjectileTimeline.Count > 0 ? 
                                     a.ProjectileTimeline[a.ProjectileTimeline.Count - 1].Timestamp : 0.0f));
        }
        GUILayout.EndHorizontal();
    }

    void EditProjectileTimelineState(ActionProjectile.ProjectileState state, ActionProjectile.eProjectileMovementMode mode, int indent)
    {
        EditFloat(ref state.Timestamp, $"{Indent(indent)}Timestamp:", 80 + indent * 12);
        EditVector2(state.SpeedMultiplier, "Speed (Random between X and Y):", indent);
        EditVector2(state.RotationPerSecond, "Rotation Speed (Random between X and Y):", indent);
        if (mode == ActionProjectile.eProjectileMovementMode.Free)
        {
            EditVector2(state.RotationY, "Rotate By Degrees (Random between X and Y):", indent);
        }
    }
    #endregion

    void EditActionTimeline(ActionTimeline timeline, ref Action.eActionType newAction, ref bool showTimeline, string title = "", int indent = 3, string skillID = "")
    {
        showTimeline = EditorGUILayout.Foldout(showTimeline, Indent(indent) + title);
        if (showTimeline)
        {
            if (timeline == null)
            {
                timeline = new ActionTimeline();
            }

            if (timeline.Count > 1 && Button("Sort", 90, indent))
            {
                GUI.FocusControl(null);
                timeline.Sort((a1, a2) => a1.Timestamp.CompareTo(a2.Timestamp));
            }

            for (int i = 0; i < timeline.Count; i++)
            {
                // Show and edit an action and check if remove button was pressed.
                var remove = !EditAction(timeline[i], indent + 1);
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
            EditEnum(ref newAction, $"{Indent(indent + 1)}New Action: ", 188 + indent * 12, 200, false);
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

            if (Button("Hide Timeline", 120, indent))
            {
                showTimeline = false;
            }
            EditorDrawLine();
        }
    }

    void EditActionConditions(Action action)
    {

    }

    void EditArea(ActionPayloadArea.Area area, int indent)
    {
        var newShape = area.Shape;
        EditEnum(ref newShape, $"{Indent(indent)}Shape:", 250 + indent * 12);
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
            EditFloat(ref area.Dimensions.x, $"{Indent(indent)}Radius:", 100 + indent * 12);
            EditFloat(ref area.InnerDimensions.x, $"{Indent(indent)}Inner Radius:", 100 + indent * 12);

            EditFloatSlider(ref area.Dimensions.y, $"{Indent(indent)}Cone Angle:", 0.0f, 360.0f, 100 + indent * 12);
            EditFloatSlider(ref area.InnerDimensions.y, $"{Indent(indent)}Inner Cone Angle:", 0.0f, 360.0f, 100 + indent * 12);
        }
        if (area.Shape == ActionPayloadArea.Area.eShape.Cube)
        {
            EditVector3(area.Dimensions, "Dimensions:", indent);
            var inner = new Vector3(area.InnerDimensions.x, 0.0f, area.InnerDimensions.y);
            EditVector3(inner, "Inner Dimensions:", indent);
            area.InnerDimensions.x = inner.x;
            area.InnerDimensions.y = inner.z;
        }
        if (area.Shape == ActionPayloadArea.Area.eShape.Cube || area.Shape == ActionPayloadArea.Area.eShape.Cylinder)
        {
            EditFloat(ref area.Dimensions.z, $"{Indent(indent)}Height:", 100 + indent * 12);
        }

        if (area.AreaTransform == null)
        {
            area.AreaTransform = new TransformData();
        }
        EditTransform(area.AreaTransform, indent, "Area Transform:");
    }

    void EditAreas(List<ActionPayloadArea.Area> areas, int indent, string label)
    {
        GUILayout.Label(Indent(indent) + label);

        indent++;
        for (int i = 0; i < areas.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Indent(indent) + $"Area [{i}]", GUILayout.Width(50 + indent * 12));

            if (Remove())
            {
                areas.RemoveAt(i);
                i--;
                continue;
            }
            GUILayout.EndHorizontal();

            EditArea(areas[i], indent + 1);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(indent * 12));
        if (Button("Add Area", 90))
        {
            areas.Add(new ActionPayloadArea.Area());
        }
        GUILayout.EndHorizontal();
    }

    void EditTransform(TransformData transform, int indent, string label)
    {
        GUILayout.Label(Indent(indent) + label);
        indent++;

        EditEnum(ref transform.PositionOrigin, $"{Indent(indent)}Transform Position Origin:", 200 + indent * 12);
        EditEnum(ref transform.ForwardSource, $"{Indent(indent)}Transform Forward Source:", 200 + indent * 12);
        if (transform.PositionOrigin == TransformData.ePositionOrigin.TaggedEntityPosition ||
            transform.ForwardSource == TransformData.eForwardSource.TaggedEntityForward)
        {
            EditString(ref transform.EntityTag, $"{Indent(indent + 1)}Entity Tag:", 200 + (indent + 1) * 12);
            EditEnum(ref transform.TaggedTargetPriority, $"{Indent(indent + 1)}Tagged Entity Priority:", 200 + (indent + 1) * 12);
        }
        EditVector3(transform.PositionOffset, "Position Offset: ", indent);
        EditVector3(transform.RandomPositionOffset, "Random Position Offset: ", indent);

        EditFloat(ref transform.ForwardRotationOffset, $"{Indent(indent)}Rotation Offset: ", 200 + indent * 12);
        EditFloat(ref transform.RandomForwardOffset, $"{Indent(indent)}Random Rotation Offset: ", 200 + indent * 12);
    }

    void EditFloatSliderDict(Dictionary<string, float> dictionary, string label, List<string> options, ref string newElement,
                             string elementLabel, int elementLabelWidth, int indent, string addLabel, int addWidth, 
                             float min = 0.0f, float max = 1.0f)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(indent)}{label}");
        }

        indent++;

        var keys = dictionary.Keys.ToList();
        for (int i = 0; i < keys.Count; i++)
        {
            GUILayout.BeginHorizontal();
            var value = dictionary[keys[i]];
            EditFloatSlider(ref value, $"{Indent(indent)}{keys[i]}{elementLabel}", min, max, elementLabelWidth, makeHorizontal: false);
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
            SelectStringFromList(ref newElement, options, $"{Indent(indent)}{addLabel}", addWidth, 150, false);
            if (Add())
            {
                dictionary.Add(newElement, max);
            }
            GUILayout.EndHorizontal();
        }
    }

    #endregion

    #region List Components

    void SelectStringFromList(ref string value, List<string> list, string label, int labelWidth = 250, int inputWidth = 200, bool makeHorizontal = true)
    {
        if (list.Count == 0)
        {
            GUILayout.Label("List is empty!");
            return;
        }

        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
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
            GUILayout.Label("No Attributes!");
            return;
        }

        if (showLabel)
        {
            GUILayout.Label("Attribute:", GUILayout.Width(60));
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
        if (showLabel)
        {
            GUILayout.Label("Category:", GUILayout.Width(52));
        }
        var categoryCopy = category; // Copy the string to use it in a lambda expression
        var index = BattleData.Categories.FindIndex(0, a => a.Equals(categoryCopy));
        if (index < 0)
        {
            index = 0;
        }
        category = BattleData.Categories[EditorGUILayout.Popup(index, BattleData.Categories.ToArray(),
                   GUILayout.Width(60))];
    }

    void SelectFaction(ref string faction, bool showLabel = true)
    {
        if (showLabel)
        {
            GUILayout.Label("Faction:", GUILayout.Width(52));
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
            GUILayout.Label("No Resources!");
            return;
        }

        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
        }

        var resourceCopy = resource; // Copy the string to use it in a lambda expression

        var index = resources.FindIndex(0, r => r.Equals(resourceCopy));
        if (index < 0)
        {
            index = 0;
        }

        resource = resources[EditorGUILayout.Popup(index, resources.ToArray(),
                   GUILayout.Width(60))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }
    #endregion

    #region Payload Components
    void EditPayload(PayloadData payload, EditorPayload editorPayload, bool isSkill, int indent)
    {
        EditListString(ref editorPayload.NewCategory, payload.Categories, Utility.CopyList(BattleData.Categories),
                       "Payload Categories: ", "No Payload Categories", "Add Payload Category:", indent);

        EditValue(payload.PayloadValue, isSkill ? eEditorValueRange.SkillAction : eEditorValueRange.NonAction, "Payload Damage:", indent: indent);
        SelectResource(ref payload.ResourceAffected, $"{Indent(indent)}Resource Affected: ", 150 + indent * 12);

        EditListString(ref editorPayload.NewFlag, payload.Flags, Utility.CopyList(BattleData.PayloadFlags),
                       "Payload Flags: ", "No Payload Flags", "Add Payload Flag:", indent);

        // Tag
        EditBool(ref editorPayload.Tag, "Tag", indent);
        if (editorPayload.Tag)
        {
            if (payload.Tag == null)
            {
                payload.Tag = new TagData();
            }
            EditString(ref payload.Tag.TagID, $"{Indent(indent + 1)}Tag ID: ", 150 + indent * 12, 150);
            EditInt(ref payload.Tag.TagLimit, $"{Indent(indent + 1)}Max tagged: ", 150 + indent * 12);
            EditFloat(ref payload.Tag.TagDuration, $"{Indent(indent + 1)}Tag Duration: ", 150 + indent * 12);
        }

        // Statuses
        if (payload.ApplyStatus == null)
        {
            payload.ApplyStatus = new List<(string StatusID, int Stacks)>();
        }
        EditStatusStackList(payload.ApplyStatus, ref editorPayload.NewStatusApply, indent,
                            "Applied Status Effects:", "No Status Effects", "Add Status Effect:");
        if (payload.RemoveStatusStacks == null)
        {
            payload.RemoveStatusStacks = new List<(string StatusID, int Stacks)>();
        }
        EditStatusStackList(payload.RemoveStatusStacks, ref editorPayload.NewStatusRemoveStack, indent,
                    "Removed Status Effects:", "No Status Effects", "Add Status Effect:");
        if (payload.ClearStatus == null)
        {
            payload.ClearStatus = new List<(bool StatusGroup, string StatusID)>();
        }
        EditStatusList(payload.ClearStatus, ref editorPayload.NewStatusClear, indent,
                "Cleared Status Effects:", "No Status Effects", "Add:");
        
        // Kill/revive
        EditBool(ref payload.Instakill, "Instakill", indent);
        if (payload.Instakill)
        {
            payload.Revive = false;
        }

        EditBool(ref payload.Revive, "Revive", indent);
        if (payload.Revive)
        {
            payload.Instakill = false;
        }

        EditFloatSlider(ref payload.SuccessChance, $"{Indent(indent)}Success Chance: ", 0.0f, 1.0f, 120 + indent * 12, 250);

        // Payload conditions
        var hasPayloadCondition = payload.PayloadCondition != null;
        EditBool(ref hasPayloadCondition, "Payload Condition", indent);
        if (hasPayloadCondition)
        {
            if (payload.PayloadCondition == null)
            {
                payload.PayloadCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            EditPayloadCondition(payload.PayloadCondition, indent);

            // Alternate payload
            var hasAlternatePayload = editorPayload.AlternatePayload != null;
            EditBool(ref hasAlternatePayload, "Alternate Payload", indent);
            if (hasAlternatePayload)
            {
                if (editorPayload.AlternatePayload == null)
                {
                    var newPayload = new PayloadData();
                    payload.AlternatePayload = newPayload;
                    editorPayload.AlternatePayload = new EditorPayload(newPayload);
                }
                EditPayload(payload.AlternatePayload, editorPayload.AlternatePayload, isSkill, indent + 1);
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
    }

    void EditPayloadAction(ActionPayload action, EditorPayloadAction editorAction, int indent)
    {
        EditEnum(ref action.Target, $"{Indent(indent)}Targets Affected: ", 200 + indent * 12);
        EditEnum(ref action.TargetState, $"{Indent(indent)}Required Target State: ", 200 + indent * 12);

        if (editorAction == null)
        {
            EditorPayloadAction.AddPayloadAction(action);
            return;
        }

        editorAction.ShowPayloads = EditorGUILayout.Foldout(editorAction.ShowPayloads, $"{Indent(indent - 1)} Payloads:");
        if (editorAction.ShowPayloads)
        {
            for (int i = 0; i < action.PayloadData.Count; i++)
            {
                EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{Indent(indent + 1)}Payload {i}", GUILayout.Width(100 + indent * 12));
                if (Button("Remove Payload", 200))
                {
                    EditorPayloads[EditorPayloadAction.ActionKey(action)].RemoveAt(i);
                    action.PayloadData.RemoveAt(i);

                    i--;
                    continue;
                }
                GUILayout.EndHorizontal();

                EditPayload(action.PayloadData[i], editorAction[i], !string.IsNullOrEmpty(action.SkillID), indent + 2);
            }

            // New payload
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width((indent + 1) * 12));
            if (Button("Add New Payload", 200))
            {
                var newPayload = new PayloadData();
                action.PayloadData.Add(newPayload);
                EditorPayloads[EditorPayloadAction.ActionKey(action)].Add(new EditorPayload(newPayload));
            }
            GUILayout.EndHorizontal();

            if (Button("Hide Payloads", 120, indent))
            {
                editorAction.ShowPayloads = false;
            }
        }

        EditInt(ref action.TargetLimit, $"{Indent(indent)}Max Targets Affected:", 200 + indent * 12);
        if (action.TargetLimit > 0)
        {
            EditEnum(ref action.TargetPriority, $"{Indent(indent)}TargetPriority: ", 200 + indent * 12);
            if (action.TargetPriority == ActionPayload.eTargetPriority.ResourceCurrentHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceCurrentLowest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceMaxHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceMaxLowest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceRatioHighest ||
                action.TargetPriority == ActionPayload.eTargetPriority.ResourceRatioLowest)
            {
                SelectResource(ref action.Resource, $"{Indent(indent + 1)}Resource: ", 200 + (indent + 1) * 12);
            }
        }
    }

    void EditPayloadCondition(PayloadCondition condition, int indent)
    {
        var newConditionType = condition.ConditionType;
        EditEnum(ref newConditionType, $"{Indent(indent)}Condition Type:");
        EditBool(ref condition.ExpectedResult, "Required Result: " + (condition.ExpectedResult ? "Success" : "Failure"), indent);

        if (newConditionType != condition.ConditionType)
        {
            condition.SetCondition(newConditionType);
        }

        switch (condition.ConditionType)
        {
            case PayloadCondition.ePayloadConditionType.AngleBetweenDirections:
            {
                EditEnum(ref condition.Direction1, $"{Indent(indent)}Direction 1:", 100 + indent * 12);
                EditEnum(ref condition.Direction2, $"{Indent(indent)}Direction 2:", 100 + indent * 12);

                EditFloatSlider(ref condition.Range.x, $"{Indent(indent)}Angle Min:", 0.0f, 100 + indent * 12);
                EditFloatSlider(ref condition.Range.y, $"{Indent(indent)}Angle Max:", 0.0f, 100 + indent * 12);
                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetHasStatus:
            {
                SelectStatus(ref condition.StatusID, $"{Indent(indent)}Status Effect:", makeHorizontal: true);
                if (BattleData.StatusEffects.ContainsKey(condition.StatusID))
                {
                    EditIntSlider(ref condition.MinStatusStacks, $"{Indent(indent)}Min. Status Stacks:", 1, BattleData.StatusEffects[condition.StatusID].MaxStacks);
                }
                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetWithinDistance:
            {
                EditFloat(ref condition.Range.x, $"{Indent(indent)}Distance Min:");
                EditFloat(ref condition.Range.y, $"{Indent(indent)}Distance Max:");

                break;
            }
            case PayloadCondition.ePayloadConditionType.TargetResourceRatioWithinRange:
            {
                EditFloatSlider(ref condition.Range.x, $"{Indent(indent)}Distance Min:", 0.0f, 1.0f);
                EditFloatSlider(ref condition.Range.y, $"{Indent(indent)}Distance Max:", 0.0f, 1.0f);

                break;
            }
        }

        var hasAndCondition = condition.AndCondition != null;
        EditBool(ref hasAndCondition, "AND Condition", indent);
        if (hasAndCondition)
        {
            if (condition.AndCondition == null)
            {
                condition.AndCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            EditPayloadCondition(condition.AndCondition, indent + 1);
        }
        else
        {
            condition.AndCondition = null;
        }

        var hasOrCondition = condition.OrCondition != null;
        EditBool(ref hasOrCondition, "OR Condition", indent);
        if (hasOrCondition)
        {
            if (condition.OrCondition == null)
            {
                condition.OrCondition = new PayloadCondition(PayloadCondition.ePayloadConditionType.AngleBetweenDirections);
            }
            EditPayloadCondition(condition.OrCondition, indent + 1);
        }
        else
        {
            condition.OrCondition = null;
        }
    }
    #endregion

    #region Skill Components
    void SelectSkill(ref string skill, string label = "", int labelWidth = 60)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
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
            GUILayout.Label(label, GUILayout.Width(labelWidth));
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
        EditFloat(ref data.RequiredChargeTime, $"{Indent(4)}Required Skill Charge Time:", 250);
        EditFloat(ref data.FullChargeTime, $"{Indent(4)}Full Skill Charge Time:", 250);

        EditBool(ref data.MovementCancelsCharge, "Movement Cancels Charge", 4);

        EditActionTimeline(data.PreChargeTimeline, ref newChargeAction, ref showTimeline, "Charge Timeline:", 5, skillID: skillID);

        EditBool(ref data.ShowUI, "Show Skill Charge UI", 4);
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
            GUILayout.Label(label, GUILayout.Width(labelWidth));
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

    void EditStatusGroup(ref string statusGroup, string label = "", int labelWidth = 200, int inputWidth = 200)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
        }

        var copy = statusGroup;
        var options = BattleData.StatusEffectGroups.Keys.ToList();
        var index = options.FindIndex(0, s => s.Equals(copy));
        if (index < 0)
        {
            index = 0;
        }
        statusGroup = options[EditorGUILayout.Popup(index, options.ToArray(),
                 GUILayout.Width(250))];
    }

    void EditStatusList(List<(bool, string)> list, ref (bool, string) newElement, int indent,
                             string label, string noLabel, string addLabel)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(indent)}{label}");
        }

        indent++;
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                var isGroup = list[i].Item1 ? 1 : 0;
                var status = list[i].Item2;

                GUILayout.Label("", GUILayout.Width(indent * 12));
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
            GUILayout.Label($"{Indent(indent)}{noLabel}");
        }

        GUILayout.BeginHorizontal();
        if (!string.IsNullOrEmpty(addLabel))
        {
            GUILayout.Label($"{Indent(indent)}{addLabel}", GUILayout.Width((addLabel.Count() + indent * 4) * 4));
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
    }

    void EditStatusStackList(List<(string, int)> list, ref string newElement, int indent,
                             string label = "", string noLabel = "", string addLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(indent)}{label}");
        }

        indent++;
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var status = list[i].Item1;
                var stacks = list[i].Item2;

                GUILayout.BeginHorizontal();
                SelectStatus(ref status, $"{Indent(indent)}Status Effect:", 90 + indent * 11);
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
            GUILayout.Label($"{Indent(indent)}{noLabel}");
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
                GUILayout.Label($"{Indent(indent)}{addLabel}", GUILayout.Width((addLabel.Count() + indent * 4) * 4));
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
    }
    #endregion

    enum eEditorValueRange
    {
        SkillAction = -1,
        Resource = 3,
        NonAction = 7
    }

    void EditValue(Value v, eEditorValueRange valueRange, string label = "", int indent = 1)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(indent)}{label}");
        }

        indent++;
        for (int i = 0; i < v.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(indent)}Component Type:", GUILayout.Width(120 + indent * 12));

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

            GUILayout.Label(" Value:", GUILayout.Width(43));
            v[i].Potency = EditorGUILayout.FloatField(v[i].Potency, GUILayout.Width(70));
            if (BattleData.EntityAttributes.Count > 0 && (v[i].ComponentType == ValueComponent.eValueComponentType.CasterAttributeBase || 
                v[i].ComponentType == ValueComponent.eValueComponentType.CasterAttributeCurrent))
            {
                GUILayout.Label("x", GUILayout.Width(10));
                SelectAttribute(ref v[i].Attribute, false);
            }
            else if (BattleData.EntityResources.Count > 0 && v[i].ComponentType == ValueComponent.eValueComponentType.CasterResourceCurrent ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.CasterResourceMax ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.TargetResourceMax)
            {
                GUILayout.Label("x", GUILayout.Width(10));
                SelectResource(ref v[i].Attribute, makeHorizontal: false);
            }
            else if (v[i].ComponentType == ValueComponent.eValueComponentType.ActionResultValue)
            {
                GUILayout.Label("x Action ID:", GUILayout.Width(68));
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
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(12 * indent));
        if (Button("Add Component", 120))
        {
            v.Add(new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, EntityAttributes[0]));
        }
        GUILayout.EndHorizontal();
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

    bool Button(string text, int width = 90, int indent = 0)
    {
        if (indent > 0)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("", GUILayout.Width(indent * 12));
        }

        var result = GUILayout.Button(text, GUILayout.Width(width));

        if (indent > 0)
        {
            GUILayout.EndHorizontal();
        }

        return result;
    }

    void EditBool(ref bool value, string label, int indent = 3)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(indent * 12));
        value = EditorGUILayout.Toggle(value, GUILayout.Width(12));
        GUILayout.Label(label);
        GUILayout.EndHorizontal();
    }

    void EditColor(ref Color color, string label, int labelWidth = 200, int inputWidth = 10, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        color = EditorGUILayout.ColorField(color, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditEnum<T>(ref T value, string label, int labelWidth = 250, int enumWidth = 250, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label, GUILayout.Width(labelWidth));
        }

        var enumValues = Utility.EnumValues<T>();
        var enumStrings = Utility.EnumStrings<T>().ToArray();

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
    void EditFloat(ref float value, string label, int labelWidth = 200, int inputWidth = 70, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        GUILayout.Label(label, GUILayout.Width(labelWidth));
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
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        value = EditorGUILayout.Slider(value, min, max, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditInt(ref int value, string label, int labelWidth = 200, int inputWidth = 70, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        value = EditorGUILayout.IntField(value, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditIntSlider(ref int value, string label, int min = 0, int max = 180, int labelWidth = 200, int inputWidth = 150)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        value = EditorGUILayout.IntSlider(value, min, max, GUILayout.Width(inputWidth));
        GUILayout.EndHorizontal();
    }

    void EditListString(ref string newElement, List<string> list, List<string> options,
                     string label = "", string noLabel = "", string addLabel = "", int indent = 1)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(indent)}{label}");
        }

        indent++;
        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{Indent(indent)}• {list[i]}", GUILayout.Width(250));
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
            GUILayout.Label($"{Indent(indent)}{noLabel}");
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
                GUILayout.Label($"{Indent(indent)}{addLabel}", GUILayout.Width((addLabel.Count() + 8) * 8));
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
    }

    void EditString(ref string value, string label, int labelWidth = 200, int inputWidth = 150, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        value = GUILayout.TextField(value, GUILayout.Width(inputWidth));
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    void EditVector2(Vector2 value, string label, int indent, int width = 300)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(indent * 12));
        EditorGUILayout.Vector2Field(label, value, GUILayout.Width(width));
        GUILayout.EndHorizontal();
    }

    void EditVector3(Vector3 value, string label, int indent, int width = 300)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(indent * 12));
        EditorGUILayout.Vector3Field(label, value, GUILayout.Width(width));
        GUILayout.EndHorizontal();
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

    string Indent(int width = 1)
    {
        var indent = "";
        for (int i = 0; i < width; i++)
        {
            indent += "    ";
        }
        return indent;
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
