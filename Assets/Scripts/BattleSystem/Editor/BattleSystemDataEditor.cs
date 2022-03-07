using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BattleSystemDataEditor : EditorWindow
{
    int Tab = 0;

    bool ShowAttributes = false;
    List<string> EntityAttributes;
    string NewAttribute = "";

    [MenuItem("Tools/Battle System Data")]
    public static void ShowWindow()
    {
        GetWindow(typeof(BattleSystemDataEditor));
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

            EntityAttributes = new List<string>();
            foreach (var attribute in BattleData.EntityAttributes)
            {
                EntityAttributes.Add(attribute);
            }


        }

        if (GUILayout.Button("Save"))
        {
            BattleData.SaveData(path);
        }
        GUILayout.EndHorizontal();

        Tab = GUILayout.Toolbar(Tab, new string[] { "Game Data", "Skill Data", "Status Effect Data", "Entity Data" });
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
    }
    
    void EditGameData()
    {
        if (BattleData.EntityAttributes == null)
        {
            BattleData.EntityAttributes = new List<string>();
        }

        // Attributes
        if (EntityAttributes != null)
        {
            ShowAttributes = EditorGUILayout.Foldout(ShowAttributes, "Entity Attributes");
            if (ShowAttributes)
            {
                for (int i = EntityAttributes.Count - 1; i >= 0; i--)
                {
                    GUILayout.BeginHorizontal();
                    EntityAttributes[i] = GUILayout.TextField(EntityAttributes[i], GUILayout.Width(200));
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

        // Resources
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Categories
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Payload Flags
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Categories
        GUILayout.BeginHorizontal();

        GUILayout.EndHorizontal();

        // Factions
        GUILayout.BeginHorizontal();
        // Faction data and player faction
        GUILayout.EndHorizontal();
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
}
