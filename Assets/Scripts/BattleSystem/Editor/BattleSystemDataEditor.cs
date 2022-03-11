using FullSerializer;
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
            BattleData.LoadData(Path);

            UpdateValues();
        }

        if (GUILayout.Button("Save"))
        {
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
                if (GUILayout.Button("Rename", GUILayout.Width(90)))
                {
                    BattleData.EntityAttributes[i] = EntityAttributes[i];
                }
                if (GUILayout.Button("Remove", GUILayout.Width(90)))
                {
                    EntityAttributes.RemoveAt(i);
                    BattleData.EntityAttributes.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(1)}New Attribute: ", GUILayout.Width(100));
            NewAttribute = GUILayout.TextField(NewAttribute, GUILayout.Width(200));
            if (GUILayout.Button("Add", GUILayout.Width(90)) && !string.IsNullOrEmpty(NewAttribute) &&
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
                if (GUILayout.Button("Save Changes", GUILayout.Width(110)))
                {
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
                if (GUILayout.Button("Remove", GUILayout.Width(90)))
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
            if (GUILayout.Button("Add", GUILayout.Width(90)) && !string.IsNullOrEmpty(NewResource.Name) &&
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
                if (GUILayout.Button("Rename", GUILayout.Width(90)))
                {
                    BattleData.Categories[i] = Categories[i];
                }
                if (GUILayout.Button("Remove", GUILayout.Width(90)))
                {
                    Categories.RemoveAt(i);
                    BattleData.Categories.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(1)}New Category: ", GUILayout.Width(100));
            NewCategory = GUILayout.TextField(NewCategory, GUILayout.Width(200));
            if (GUILayout.Button("Add", GUILayout.Width(90)) && !string.IsNullOrEmpty(NewCategory) &&
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
                if (GUILayout.Button("Rename", GUILayout.Width(90)))
                {
                    BattleData.PayloadFlags[i] = PayloadFlags[i];
                }
                if (GUILayout.Button("Remove", GUILayout.Width(90)))
                {
                    PayloadFlags.RemoveAt(i);
                    BattleData.PayloadFlags.RemoveAt(i);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(1)}New Payload Flag: ", GUILayout.Width(120));
            NewPayloadFlag = GUILayout.TextField(NewPayloadFlag, GUILayout.Width(200));
            if (GUILayout.Button("Add", GUILayout.Width(90)) && !string.IsNullOrEmpty(NewPayloadFlag) &&
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
                if (GUILayout.Button("Save Changes", GUILayout.Width(110)))
                {
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
                if (GUILayout.Button("Remove", GUILayout.Width(90)))
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
                if (GUILayout.Button("Add", GUILayout.Width(90)) && !string.IsNullOrEmpty(NewFaction) &&
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
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
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

                if (GUILayout.Button("+", GUILayout.Width(20)) && newFaction != null)
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
                if (GUILayout.Button("Save Changes", GUILayout.Width(110)))
                {
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

                if (GUILayout.Button("Remove", GUILayout.Width(90)))
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
            if (GUILayout.Button("Add", GUILayout.Width(90)) && !string.IsNullOrEmpty(NewSkillGroup) &&
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
                    if (GUILayout.Button("Save Changes", GUILayout.Width(110)))
                    {
                        if (!skill.HasChargeTime)
                        {
                            skill.SkillData.SkillChargeData = null;
                        }

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

                    if (GUILayout.Button("Remove", GUILayout.Width(90)))
                    {
                        Skills.RemoveAt(i);
                        BattleData.Skills.Remove(skill.SkillData.SkillID);
                    }
                    GUILayout.EndHorizontal();

                    // Values
                    EditBool(ref skill.SkillData.Interruptible, "Is Interruptible", 33);

                    EditBool(ref skill.HasChargeTime, "Has Charge Time", 33);
                    if (skill.HasChargeTime)
                    {
                        if (skill.SkillData.SkillChargeData == null)
                        {
                            skill.SkillData.SkillChargeData = new SkillChargeData();
                        }
                        EditSkillChargeData(skill.SkillData.SkillChargeData, ref skill.NewChargeTimelineAction, ref skill.ShowChargeTimeline);
                    }

                    EditActionTimeline(skill.SkillData.SkillTimeline, ref skill.NewTimelineAction, ref skill.ShowTimeline, "Skill Timeline:");

                    EditBool(ref skill.SkillData.NeedsTarget, "TargetRequired", 33);
                    EditEnum(ref skill.SkillData.PreferredTarget, $"{Indent(3)}Preferred Target: ");
                    EditFloat(ref skill.SkillData.Range, $"{Indent(3)}Max Distance From Target:", 250, 150);

                    EditorDrawLine(new Color(0.35f, 0.35f, 0.35f));
                }
            }

            // New skill
        }

        // Save + sort action

    }
    #endregion

    #region Status Effect Data
        void EditStatusEffectData()
    {
        // Status Effect Groups
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Status Effects
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();
    }
    #endregion

    #region Entity Data
    void EditEntityData()
    {

    }
    #endregion

    #region Components
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
        GUILayout.Label($"{Indent(indent)}Action:", GUILayout.Width(50 + indent * 10));

        string[] options = Utility.EnumStrings<Action.eActionType>();

        var newActionType = (Action.eActionType)EditorGUILayout.Popup((int)action.ActionType,
                             options, GUILayout.Width(160));

        if (action.ActionType != newActionType)
        {
            action.ActionType = newActionType;
        }

        GUILayout.Label("Action ID:", GUILayout.Width(60));
        action.ActionID = GUILayout.TextField(action.ActionID, GUILayout.Width(190));

        GUILayout.Label("Timestamp:", GUILayout.Width(70));
        action.Timestamp = EditorGUILayout.FloatField(action.Timestamp, GUILayout.Width(70));

        GUILayout.EndHorizontal();

        if (ShowHelp && ActionHelp.ContainsKey(action.ActionType))
        {
            EditorGUILayout.HelpBox(ActionHelp[action.ActionType], MessageType.None);
        }

        indent++;

        switch (action.ActionType)
        {
            case Action.eActionType.ApplyCooldown:
            {
                var a = action as ActionCooldown;

                if (a == null)
                {
                    a = Action.Convert<ActionCooldown>(action);
                    a.SetTypeDefaults();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label($"{Indent(indent)}Cooldown:", GUILayout.Width(250));
                a.Cooldown = EditorGUILayout.FloatField(action.Timestamp, GUILayout.Width(60));
                GUILayout.EndHorizontal();

                EditEnum(ref a.ChangeMode, $"{Indent(indent)}Change Mode:", 250, 120);
                EditEnum(ref a.CooldownTarget, $"{Indent(indent)}Cooldown Target:", 250, 120);

                GUILayout.BeginHorizontal();
                if (a.CooldownTarget == ActionCooldown.eCooldownTarget.Skill)
                {
                    EditSkill(ref a.CooldownTargetName, $"{Indent(indent)}Cooldown Skill ID:", 250);
                }
                else if (a.CooldownTarget == ActionCooldown.eCooldownTarget.SkillGroup)
                {
                    EditSkillGroup(ref a.CooldownTargetName, $"{Indent(indent)}Cooldown Skill Group Name:", 250);
                }

                GUILayout.EndHorizontal();

                break;
            }
            case Action.eActionType.CollectCost:
            {
                var a = action as ActionCostCollection;

                if (a == null)
                {
                    a = Action.Convert<ActionCostCollection>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.DestroySelf:
            {
                var a = action as ActionDestroySelf;

                if (a == null)
                {
                    a = Action.Convert<ActionDestroySelf>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.LoopBack:
            {
                var a = action as ActionLoopBack;

                if (a == null)
                {
                    a = Action.Convert<ActionLoopBack>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.Message:
            {
                var a = action as ActionMessage;

                if (a == null)
                {
                    a = Action.Convert<ActionMessage>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.PayloadArea:
            {
                var a = action as ActionPayloadArea;

                if (a == null)
                {
                    a = Action.Convert<ActionPayloadArea>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.PayloadDirect:
            {
                var a = action as ActionPayloadDirect;

                if (a == null)
                {
                    a = Action.Convert<ActionPayloadDirect>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.SpawnProjectile:
            {
                var a = action as ActionProjectile;

                if (a == null)
                {
                    a = Action.Convert<ActionProjectile>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.SpawnEntity:
            {
                var a = action as ActionSummon;

                if (a == null)
                {
                    a = Action.Convert<ActionSummon>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
            case Action.eActionType.SetAnimation:
            {
                var a = action as ActionAnimationSet;

                if (a == null)
                {
                    a = Action.Convert<ActionAnimationSet>(action);
                    a.SetTypeDefaults();
                }

                break;
            }
        }

        EditActionConditions(action);

        return true;
    }

    void EditActionTimeline(ActionTimeline timeline, ref Action.eActionType newAction, ref bool showTimeline, string title = "", int indent = 3)
    {
        showTimeline = EditorGUILayout.Foldout(showTimeline, Indent(indent) + title);
        if (showTimeline)
        {
            for (int i = 0; i < timeline.Count; i++)
            {
                // Show and edit an action and check if remove button was pressed.
                if (!EditAction(timeline[i], indent + 1))
                {
                    timeline.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            // New action
            GUILayout.BeginHorizontal();
            EditEnum(ref newAction, $"{Indent(indent + 1)}New Action: ", 200, 200, false);
            if (GUILayout.Button("Add", GUILayout.Width(90)))
            {
                var a = Action.MakeAction(newAction);
                a.ActionID = newAction.ToString() + "Action";
                if (timeline.Count > 0)
                {
                    a.Timestamp = timeline[timeline.Count - 1].Timestamp;
                    a.SkillID = timeline[timeline.Count - 1].SkillID;
                }
                timeline.Add(a);
            }
            GUILayout.EndHorizontal();
        }
    }

    void EditActionConditions(Action action)
    {

    }

    void EditAttribute(ref string attribute, bool showLabel = true)
    {
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

    void EditBool(ref bool value, string label, int indent = 33)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(indent));
        value = EditorGUILayout.Toggle(value, GUILayout.Width(12));
        GUILayout.Label(label);
        GUILayout.EndHorizontal();
    }

    void EditCategory(ref string category, bool showLabel = true)
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

    void EditEnum<T>(ref T value, string label, int labelWidth = 250, int enumWidth = 60, bool makeHorizontal = true)
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

    void EditFaction(ref string faction, bool showLabel = true)
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

    void EditFloat(ref float value, string label, int labelWidth = 200, int inputWidth = 70)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        value = EditorGUILayout.FloatField(value, GUILayout.Width(inputWidth));
        GUILayout.EndHorizontal();
    }

    void EditInt(ref int value, string label, int labelWidth = 200, int inputWidth = 70)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(labelWidth));
        value = EditorGUILayout.IntField(value, GUILayout.Width(inputWidth));
        GUILayout.EndHorizontal();
    }

    void EditListString(ref string newElement, List<string> list, List<string> options, 
                     string label = "", string noLabel = "", string addLabel = "")
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(1)}{label}");
        }

        if (list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                GUILayout.BeginHorizontal();
                Indent(2);
                GUILayout.Label($"{Indent(2)}• {list[i]}", GUILayout.Width(250));
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    list.RemoveAt(i);
                    i--;
                }
                GUILayout.EndHorizontal();
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            GUILayout.Label($"{Indent(2)}{noLabel}");
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
                GUILayout.Label($"{Indent(2)}{addLabel}", GUILayout.Width((addLabel.Count() + 8) * 6));
            }

            var copy = newElement; // This is needed for the lambda expression to work.
            var index = options.FindIndex(0, a => a.Equals(copy));
            if (index < 0)
            {
                index = 0;
            }
            newElement = options[EditorGUILayout.Popup(index, options.ToArray(),
                         GUILayout.Width(250))];

            if (GUILayout.Button("+", GUILayout.Width(20)) && newElement != null)
            {
                list.Add(newElement);
            }

            GUILayout.EndHorizontal();
        }
    }

    void EditResource(ref string resource, bool showLabel = true)
    {
        if (showLabel)
        {
            GUILayout.Label("Resource:", GUILayout.Width(60));
        }
        var resourceCopy = resource; // Copy the string to use it in a lambda expression
        var resources = BattleData.EntityResources.Keys.ToList();
        var index = resources.FindIndex(0, r => r.Equals(resourceCopy));
        if (index < 0)
        {
            index = 0;
        }
        resource = resources[EditorGUILayout.Popup(index, resources.ToArray(),
                   GUILayout.Width(60))];
    }

    void EditSkill(ref string skill, string label = "", int labelWidth = 60)
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

    void EditSkillChargeData(SkillChargeData data, ref Action.eActionType newChargeAction, ref bool showTimeline)
    {
        EditFloat(ref data.RequiredChargeTime, $"{Indent(4)}Required Skill Charge Time:", 250);
        EditFloat(ref data.FullChargeTime, $"{Indent(4)}Full Skill Charge Time:", 250);

        EditBool(ref data.MovementCancelsCharge, "Movement Cancels Charge", 46);

        EditActionTimeline(data.PreChargeTimeline, ref newChargeAction, ref showTimeline, "Charge Timeline:", 5);

        EditBool(ref data.ShowUI, "Show Skill Charge UI", 46);
    }

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
            GUILayout.Label($"{Indent(1)}{label}");
        }

        for (int i = 0; i < v.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(2)}Component Type:", GUILayout.Width(130));

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
                EditAttribute(ref v[i].Attribute, false);
            }
            else if (BattleData.EntityResources.Count > 0 && v[i].ComponentType == ValueComponent.eValueComponentType.CasterResourceCurrent ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.CasterResourceMax ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.TargetResourceCurrent ||
                     v[i].ComponentType == ValueComponent.eValueComponentType.TargetResourceMax)
            {
                GUILayout.Label("x", GUILayout.Width(10));
                EditResource(ref v[i].Attribute, false);
            }
            else if (v[i].ComponentType == ValueComponent.eValueComponentType.ActionResultValue)
            {
                GUILayout.Label("x Action ID:", GUILayout.Width(68));
                v[i].Attribute = GUILayout.TextField(v[i].Attribute, GUILayout.Width(190));
            }

            if (GUILayout.Button("Copy", GUILayout.Width(70)))
            {
                v.Add(v[i].Copy);
            }
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                v.RemoveAt(i);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal();
        GUILayout.Label("", GUILayout.Width(20));
        if (GUILayout.Button("Add Component", GUILayout.Width(120)))
        {
            v.Add(new ValueComponent(ValueComponent.eValueComponentType.CasterAttributeCurrent, 1.0f, EntityAttributes[0]));
        }
        GUILayout.EndHorizontal();
    }
    #endregion

    #region Utility
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
