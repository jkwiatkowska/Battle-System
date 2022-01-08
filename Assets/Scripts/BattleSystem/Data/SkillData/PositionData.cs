using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionData
{
    public enum ePositionOrigin
    {
        WorldPosition,          // A position in the world
        CasterPosition,         // The entity casting the skill
        SelectedTargetPosition, // Selected targetable
        TaggedEntityPosition,   // Entity referenced with a string tag
        //PositionFromInput       // To do
    }

    public ePositionOrigin PositionOrigin;  // Where a skill is positioned
    public string EntityTag;                // If using tagged entity position

    public Vector2 PositionOffset;          // Position offset from position origin
    public Vector2 RandomPositionOffset;    // Range of a random offset from the summon position, for each x and y axis
}
