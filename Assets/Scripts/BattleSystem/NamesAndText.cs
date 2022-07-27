using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NamesAndText
{
    // This class can be used to customise how entity, skill and attribute names and other text appears in game.
    // By default it simply returns their IDs, but it can be customised to get the names from somewhere else, for example a dictionary or a localisation file. 

    public static string EntityName(Entity entity)
    {
        var name = entity.UID;

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

    public static string ResourceChangeText(string resource, float change, PayloadComponentResult payloadResult, out Color color)
    {
        var text = "";
        color = Color.white;

        var resourceChange = payloadResult.PayloadComponent as PayloadResourceChange;
        if (resourceChange == null)
        {
            Debug.LogError("Payload Result does not come from a resource change.");
            return text;
        }

        if (payloadResult?.Payload != null)
        {
            var skill = payloadResult.Payload.Action?.SkillID;
            if (!string.IsNullOrEmpty(skill))
            {
                text += $"{skill} ";
            }
            else
            {
                var statusID = payloadResult.Payload.SourceStatusEffect;
                if (statusID != null)
                {
                    text += $"{statusID} ";
                }
            }

            if (payloadResult.Payload.PayloadData?.Categories != null)
            {
                if (payloadResult.Payload.PayloadData.Categories.Contains("fire"))
                {
                    color = Color.red;
                }
                else if (payloadResult.Payload.PayloadData.Categories.Contains("water"))
                {
                    color = Color.blue;
                }
                else if (payloadResult.Payload.PayloadData.Categories.Contains("geo"))
                {
                    color = new Color(1, 0.6f, 0.2f);
                }
            }
        }

        var recovery = change > 0;
        if (recovery)
        {
            text = "+";
            color = Color.green;
        }

        if (resource == "mp")
        {
            color = recovery ? Color.cyan : new Color(1, 0, 1);
        }

        text += $"{Mathf.Round(change)} {ResourceName(resource)}";

        var flags = payloadResult?.ResultFlags;
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

    public static string NotInLineOfSightMessage(out Color color)
    {
        color = Color.white;
        var message = "Target is not in line of sight.";

        return message;
    }

    public static string MessageFromString(string messageString)
    {
        var message = messageString;

        return message;
    }
}
