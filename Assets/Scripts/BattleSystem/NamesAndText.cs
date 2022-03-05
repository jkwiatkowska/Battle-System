using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NamesAndText
{
    // This class can be used to customise how entity, skill and attribute names and other text appears in game.
    // By default it simply returns their IDs, but it can be customised to get the names from somewhere else, for example a dictionary or a localisation file. 

    public static string EntityName(Entity entity)
    {
        var name = entity.EntityUID;

        return name;
    }

    public static string EntityLevel(Entity entity)
    {
        var name = $"Lv {entity.Level}";

        return name;
    }

    public static string SkillName(string skillID)
    {
        var name = skillID;

        return name;
    }

    public static string ResourceName(string resource)
    {
        var name = resource.ToUpper();

        return name;
    }

    public static string AttributeName(string attribute)
    {
        var name = attribute.ToUpper();

        return name;
    }

    public static string DamageText(PayloadData payloadData, string resourceAffected, float value, List<string> flags, out Color color)
    {
        var text = "";
        color = Color.red;

        if (value > 0)
        {
            text = "+";
            color = Color.green;
        }

        text += $"{Mathf.Round(value)} {ResourceName(resourceAffected)}";

        if (flags.Contains("critical"))
        {
            text += '!';
        }

        return text;
    }

    public static string MissText(out Color color)
    {
        var text = "Miss!";

        color = Color.grey;

        return text;
    }

    public static string ImmuneText(out Color color)
    {
        var text = "Immune";

        color = Color.grey;

        return text;
    }

    public static string SkillChargeProgressText(float elapsedTime, float fullTime)
    {
        var text = (fullTime - elapsedTime).ToString("0.00") + 's';

        return text;
    }

    public static string OutOfRangeMessage(out Color color)
    {
        color = Color.white;
        var message = "Target is out of range.";

        return message;
    }

    public static string MessageFromString(string messageString)
    {
        var message = messageString;

        return message;
    }
}
