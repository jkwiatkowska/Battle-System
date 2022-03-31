using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BattleGUI
{
    public enum eReturnResult
    {
        None,
        Remove,
        Copy
    }

    #region Buttons
    public static bool Add()
    {
        return GUILayout.Button("Add", GUILayout.Width(70));
    }

    public static bool Copy()
    {
        return GUILayout.Button("Copy", GUILayout.Width(70));
    }

    public static bool Remove()
    {
        return GUILayout.Button("Remove", GUILayout.Width(70));
    }

    public static bool Rename()
    {
        return GUILayout.Button("Rename", GUILayout.Width(70));
    }

    public static bool SaveChanges()
    {
        return GUILayout.Button("Save Changes", GUILayout.Width(110));
    }

    public static bool Button(string text, int width = 90)
    {
        return GUILayout.Button(text, GUILayout.Width(width));
    }
    #endregion

    #region Basic Components
    public static void EditBool(ref bool value, string label, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }
        value = EditorGUILayout.Toggle(value, GUILayout.Width(12));
        Label(label);
        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    public static void EditColor(ref Color color, string label, int labelWidth = 200, bool makeHorizontal = true)
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

    public static void EditEnum<T>(ref T value, string label = "", int labelWidth = 150, int enumWidth = 250, bool makeHorizontal = true)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var enumValues = EnumValues<T>();
        var enumStrings = EnumStrings<T>();

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

    public static void EditEnum<T>(ref T value, List<T> options, string label = "", int labelWidth = 150, int enumWidth = 250,
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

        value = options[EditorGUILayout.Popup(index, EnumStrings(options),
                   GUILayout.Width(enumWidth))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    public static void EditFloat(ref float value, string label, int labelWidth = 200, int inputWidth = 70, bool makeHorizontal = true)
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

    public static void EditFloatSlider(ref float value, string label, float min = 0.0f, float max = 1.0f, int labelWidth = 200, int inputWidth = 250, bool makeHorizontal = true)
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

    public static void EditFloatDict(Dictionary<string, float> dictionary, string label, List<string> options, ref string newElement,
                                     string elementLabel, int elementLabelWidth, string addLabel, int addWidth,
                                     bool slider = false, float min = 0.0f, float max = 1.0f)
    {
        if (!string.IsNullOrEmpty(label))
        {
            Label(label);
        }

        StartIndent();
        if (options == null || options.Count < 1)
        {
            Label("(No options)");
            EndIndent();
            return;
        }

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

    public static bool EditFoldout(ref bool show, string label)
    {
        show = EditorGUILayout.Foldout(show, label);
        return show;
    }
    public static void EditInt(ref int value, string label, int labelWidth = 200, int inputWidth = 70, bool makeHorizontal = true)
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

    public static void EditIntSlider(ref int value, string label, int min = 0, int max = 180, int labelWidth = 200, int inputWidth = 150)
    {
        GUILayout.BeginHorizontal();
        Label(label, labelWidth);
        value = EditorGUILayout.IntSlider(value, min, max, GUILayout.Width(inputWidth));
        GUILayout.EndHorizontal();
    }

    public static void Label(string label)
    {
        GUILayout.Label(label);
    }

    public static void Label(string label, int width = 0)
    {
        if (!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label, GUILayout.Width(width));
        }
    }

    public static void EditList<T>(ref string newElement, List<T> list, List<string> options, Func<T, eReturnResult> editElementFunction,
                                   Func<string, T> newElementFunction, string label = "", string noLabel = "", string addLabel = "") where T : new()
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
                var result = editElementFunction(list[i]);

                if (result == eReturnResult.Remove)
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                else if (result == eReturnResult.Copy)
                {
                    list.Add(Copy(list[i]));
                }
            }
        }
        else if (!string.IsNullOrEmpty(noLabel))
        {
            Label(noLabel);
        }

        if (options != null && options.Count > 0)
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
                list.Add(newElementFunction(newElement));
            }

            GUILayout.EndHorizontal();
        }
        EndIndent();
    }

    public static void EditListString(ref string newElement, List<string> list, List<string> options,
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
                Label($"� {list[i]}", 200);
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

    public static void EditString(ref string value, string label, int labelWidth = 200, int inputWidth = 150, bool makeHorizontal = true)
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

    public static void EditVector2(ref Vector2 value, string label, int width = 300)
    {
        value = EditorGUILayout.Vector2Field(label, value, GUILayout.Width(width));
    }

    public static void EditVector3(Vector3 value, string label, int width = 300)
    {
        EditorGUILayout.Vector3Field(label, value, GUILayout.Width(width));
    }

    public static void Help(string message)
    {
        EditorGUILayout.HelpBox(message, MessageType.None);
    }

    public static void SelectStringFromList(ref string value, List<string> list, string label, int labelWidth = 150, int inputWidth = 200, bool makeHorizontal = true)
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
    #endregion

    #region Visual
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

    public static void StartIndent()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        GUILayout.BeginVertical();
    }

    public static void EndIndent()
    {
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
    #endregion

    #region Utility
    public static T Copy<T>(T classObject) where T : new()
    {
        // Use the serializer to perform a deep copy of a class object.
        BattleData.Serializer.TrySerialize(classObject, out var data);

        var newObject = new T();
        BattleData.Serializer.TryDeserialize(data, ref newObject);

        return newObject;
    }

    public static List<T> CopyList<T>(List<T> list)
    {
        var copy = new List<T>();
        foreach (var item in list)
        {
            copy.Add(item);
        }
        return copy;
    }

    public static List<T> CopyLists<T>(List<List<T>> lists)
    {
        var copy = new List<T>();
        foreach (var list in lists)
        {
            foreach (var item in list)
            {
                copy.Add(item);
            }
        }
        return copy;
    }

    public static List<T> EnumValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().ToList();
    }

    public static string[] EnumStrings<T>()
    {
        var enumTypes = EnumValues<T>();
        var count = enumTypes.Count();
        var types = new string[count];

        for (int i = 0; i < count; i++)
        {
            types[i] = enumTypes[i].ToString();
        }
        return types;
    }

    public static string[] EnumStrings<T>(List<T> list)
    {
        var types = new string[list.Count];

        for (int i = 0; i < list.Count; i++)
        {
            types[i] = list[i].ToString();
        }
        return types;
    }
    #endregion

    #region Battle System
    public static void SelectAttribute(ref string attribute, string label = "Attribute:", int labelWidth = 60, bool makeHorizontal = false)
    {
        if (makeHorizontal)
        {
            GUILayout.BeginHorizontal();
        }

        var attributes = BattleData.EntityAttributes;

        if (attributes.Count == 0)
        {
            Label("(No Attributes!)");
            return;
        }

        if (!string.IsNullOrEmpty(label))
        {
            Label(label, labelWidth);
        }

        var attributeCopy = attribute; // Copy the string to use it in a lambda expression

        var index = attributes.FindIndex(0, a => a.Equals(attributeCopy));
        if (index < 0)
        {
            index = 0;
        }
        attribute = attributes[EditorGUILayout.Popup(index, BattleData.EntityAttributes.ToArray(),
                    GUILayout.Width(90))];

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    public static void SelectCategory(ref string category, bool showLabel = true)
    {
        var categories = BattleData.PayloadCategories;
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

    public static void SelectEntity(ref string id, EntityData.eEntityType entityType, string label, int labelWidth)
    {
        SelectStringFromList(ref id, BattleData.Entities.Keys.Where((e) =>
                             BattleData.Entities[e].EntityType == entityType).ToList(), label, labelWidth);
    }

    public static void SelectFaction(ref string faction, string label = "", int labelWidth = 150, bool makeHorizontal = true)
    {
        if (BattleData.Factions.Count == 0)
        {
            Label("No Factions!");
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
                      GUILayout.Width(250))];
        }

        if (makeHorizontal)
        {
            GUILayout.EndHorizontal();
        }
    }

    public static void SelectResource(ref string resource, string label = "", int labelWidth = 60, bool makeHorizontal = true)
    {
        var resources = BattleData.EntityResources.Keys.ToList();

        if (resources.Count == 0)
        {
            Label("(No Resources!)");
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

    public static void SelectSkill(ref string skill, string label = "", int labelWidth = 60, bool makeHorizontal = false)
    {
        if (makeHorizontal)
        {
            EditorGUILayout.BeginHorizontal();
        }

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

        if (makeHorizontal)
        {
            EditorGUILayout.EndHorizontal();
        }
    }

    public static void SelectStatus(ref string status, string label = "", int labelWidth = 200, int inputWidth = 200, bool makeHorizontal = false)
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

    public static Action CopyAction(Action action)
    {
        switch (action.ActionType)
        {
            case Action.eActionType.ApplyCooldown:
            {
                var a = action as ActionCooldown;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.CollectCost:
            {
                var a = action as ActionCostCollection;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.DestroySelf:
            {
                var a = action as ActionDestroySelf;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.LoopBack:
            {
                var a = action as ActionLoopBack;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.Message:
            {
                var a = action as ActionMessage;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.PayloadArea:
            {
                var a = action as ActionPayloadArea;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.PayloadDirect:
            {
                var a = action as ActionPayloadDirect;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.SpawnProjectile:
            {
                var a = action as ActionProjectile;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.SpawnEntity:
            {
                var a = action as ActionSummon;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
            case Action.eActionType.SetAnimation:
            {
                var a = action as ActionAnimationSet;
                if (a != null)
                {
                    return Copy(a);
                }
                break;
            }
        }

        return null;
    }
    #endregion
}
