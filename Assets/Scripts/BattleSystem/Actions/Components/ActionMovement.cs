public class ActionMovement
{
    public enum eMovementType
    {
        MoveToPosition,                     // Entity moves in the direction of a given position.
        TeleportToPosition,                 // Entity teleports to a given position.
        LaunchToPosition,                   // Entity is launched toward a given position.
        MoveInDirection,                    // Entity moves in a given direction.
        FreezePosition,                     // Entity is unable to move for the duration of the movement.
    }

    public enum eFaceDirection
    {
        FaceMovementDirection,
        FaceOppositeOfMovementDirection,
        KeepOriginalDirection,
    }

    public eMovementType MovementType;
    public TransformData TargetPosition;    // Position to move to.

    public float Speed;                     // If 0, use entity movement speed.
    public float SpeedChangeOverTime;       // Decrease or increase in speed per second.
    public float MinSpeed;                  // If the speed changes over time, a limit can be set
    public float MaxSpeed;

    public eFaceDirection FaceDirection;    // If true, the entity will face the direction it's moving in.
    public bool HorizontalMovementOnly;     // If true, the entity will only move along the X and Z axis.

    public float LaunchAngle;               // For launch movement type. 
        
    public float MaxDuration;               // Movement will stop after this much time.

    public float InterruptionLevel;         // If bigger than zero and smaller than the interrupt resistance level of an entity, the effect won't be applied.
    public int Priority;                    // If the entity is already affected by movement of this type, the movement with the lower priority will be cancelled.
    public bool LockEntityMovement;         // If true, the entity won't be able to move while the movement is active.

    public ActionMovement()
    {
        TargetPosition = new TransformData();
        MaxDuration = 1.0f;
        HorizontalMovementOnly = true;
        LockEntityMovement = true;
    }
}

public class ActionRotation
{
    public enum eRotationType
    {
        SetRotation,                        // Immediately set rotation.
        RotateToDirection,                  // Rotate toward a specific direction over time.
        Rotate,                             // Rotates clockwise by default, counter-clockwise if speed is negative.
    }

    public eRotationType RotationType;
    public DirectionData Direction;         // Rotation to shift to.

    public float Speed;                     // If 0, use entity rotation speed.
    public float SpeedChangeOverTime;       // Decrease or increase in speed per second.
    public float MinSpeed;                  // If the speed changes over time, a limit can be set
    public float MaxSpeed;

    public float MaxDuration;               // Rotation will stop after this much time.

    public float InterruptionLevel;         // If bigger than zero and smaller than the interrupt resistance level of an entity, the effect won't be applied.
    public int Priority;                    // If the entity is already affected by a rotation, the rotation with the lower priority will be cancelled. 

    public ActionRotation()
    {
        Direction = new DirectionData();
        MaxDuration = 1.0f;
    }
}
