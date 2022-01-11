using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Names
{
    // This class can be used to customise how entity, skill and attribute names.
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

    public static string SkillName(SkillData skillData)
    {
        var name = skillData.SkillID;

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
}
