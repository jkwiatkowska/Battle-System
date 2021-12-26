using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillActionData
{
    public enum eSkillActionType
    {
        Area,
        Direct,
        Projectile,
        Summon
    }

    public string ActionID;
    public eSkillActionType ActionType;
    public float Timestamp;
}