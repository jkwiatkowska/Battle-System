using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NamesAndText
{
    // This class can be used to customise how entity, skill and attribute names and other text appears in game.
    // By default it simply returns their IDs, but it can be customised to get the names from somewhere else, for example a dictionary or a localisation file. 

    public static string EntityName(Entity entity)
    {
        var name = entity.ID;

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

    public static string DepletableName(string depletable)
    {
        var name = depletable.ToUpper();

        return name;
    }

    public static string AttributeName(string attribute)
    {
        var name = attribute.ToUpper();

        return name;
    }

    public static string DamageText(ActionPayload action, float value)
    {
        var text = "";

        if (value > 0)
        {
            text = "+";
        }

        text += $"{Mathf.Round(value)} {DepletableName(action.Payload.DepletableAffected)}";

        return text;
    }

    public static string SkillChargeProgressText(float elapsedTime, float fullTime)
    {
        var text = (fullTime - elapsedTime).ToString("0.00") + 's';

        return text;
    }
}