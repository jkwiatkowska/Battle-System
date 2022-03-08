using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionData
{
    public string FactionID;
    public List<string> FriendlyFactions;
    public List<string> EnemyFactions;

    public FactionData(string id)
    {
        FactionID = id;
        FriendlyFactions = new List<string>();
        EnemyFactions = new List<string>();
    }

    public FactionData Copy()
    {
        var factionData = new FactionData(FactionID);

        foreach (var faction in FriendlyFactions)
        {
            factionData.FriendlyFactions.Add(faction);
        }

        foreach (var faction in EnemyFactions)
        {
            factionData.EnemyFactions.Add(faction);
        }

        return factionData;
    }
}
