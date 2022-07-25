using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public const float Gravity = -9.8f;                 // Default gravitational force
    public const float Epsilon = 0.000001f;             // Used to determine if a float value is close to 0. 
    public const float ObstacleDetectRange = 0.05f;     // Distance at which an entity will detect an obstacle (raycast length)
    public const float ObstacleDetectHeight = 0.02f;    // Raycast height
    public const float EntityRefreshRate = 0.2f;        // Rate at which an aggressive entity attempts to detect targets. 
}
