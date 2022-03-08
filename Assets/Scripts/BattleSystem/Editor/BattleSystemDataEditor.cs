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

    [MenuItem("Tools/Battle System Data")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BattleSystemDataEditor));
    }

    void Awake()
    {
        UpdateValues();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Path:", GUILayout.Width(35));
        var path = GUILayout.TextField("", GUILayout.Width(400f));

        if (GUILayout.Button("Load"))
        {
            BattleData.LoadMockData();
            //BattleSystemData.LoadData(path);

            UpdateValues();
        }

        if (GUILayout.Button("Save"))
        {
            BattleData.SaveData(path);
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
    }

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

        GUILayout.BeginHorizontal();
        GUILayout.EndHorizontal();
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

                EditValue(EntityResources[i].Value, $"Max { EntityResources[i].Name.ToUpper()} Value:");
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

    void EditValue(Value v, string label = "", bool showAll = false)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label($"{Indent(1)}{label}");
        }

        for (int i = 0; i < v.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{Indent(2)}Component Type:", GUILayout.Width(130));

            string[] options = Utility.EnumStrings<ValueComponent.eValueComponentType>(showAll ? -1 : 3);

            v[i].ComponentType = (ValueComponent.eValueComponentType)EditorGUILayout.Popup((int)v[i].ComponentType,
                                  options, GUILayout.Width(180));
            GUILayout.Label(" Value:", GUILayout.Width(43));
            v[i].Potency = EditorGUILayout.FloatField(v[i].Potency, GUILayout.Width(80));
            if (v[i].ComponentType != ValueComponent.eValueComponentType.FlatValue && EntityAttributes.Count > 0)
            {
                GUILayout.Label("x", GUILayout.Width(10));
                EditAttribute(ref v[i].Attribute, false);
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
                !Categories.Contains(NewCategory))
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
                !PayloadFlags.Contains(NewPayloadFlag))
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
                            BattleData.Factions.Add(Factions[i].Data.FactionID, value);
                        }
                        else
                        {
                            Factions[i].Data.FactionID = oldName;
                            BattleData.Factions[Factions[i].Data.FactionID] = value;
                        }
                    }
                    else
                    {
                        BattleData.Factions[Factions[i].Data.FactionID] = value;
                    }

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
            }
        }
    }

    string Indent(int width = 1)
    {
        var indent = "";
        for (int i = 0; i < width; i ++)
        {
            indent += "    ";
        }
        return indent;
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

    void EditSkillData()
    {
        // Skill Groups
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Skills
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

    }

    void EditStatusEffectData()
    {
        // Status Effect Groups
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Status Effects
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();
    }

    void EditEntityData()
    {

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
}
