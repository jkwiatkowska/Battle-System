public class ActionMovement
{
    public enum eMovementType
    {
        MoveToPosition,
        TeleportToPosition,
        LaunchToPosition,
        MoveForward,
        MoveBackward,
    }

    public eMovementType MovementType;
    public TransformData TargetPosition;    // Position to move to.
    public float Speed;                     // If 0, use entity movement speed.

    public bool FaceMovementDirection;      // If true, the entity will face the direction it's moving in.
    public bool HorizontalMovementOnly;     // If true, the entity will only move along the X and Z axis.

    public float LaunchAngle;               // For launch movement type. 
        
    public float MaxDuration;               // Movement will stop after this much time.

    public float InterruptionLevel;         // If bigger than zero and smaller than the interrupt resistance level of an entity, the effect won't be applied.
    public int Priority;                    // If the entity is already affected by movement of this type, the movement with the lower priority will be cancelled.
    public bool LockEntityMovement;         // If true, the entity won't be able to move while the movement is active.

    public ActionMovement()
    {
        TargetPosition = new TransformData();
        FaceMovementDirection = true;
        MaxDuration = 1.0f;
        HorizontalMovementOnly = true;
        LockEntityMovement = true;
    }
}

public class ActionRotation
{
    public enum eRotationType
    {
        RotateToFaceTarget,
        RotateClockwise,
        RotateCounterClockwise,
    }

    public eRotationType RotationType;
    public TransformData Transform;         // Rotation to shift to.
    public float Speed;                     // If 0, use entity rotation speed.

    public float MaxDuration;               // Rotation will stop after this much time.

    public float InterruptionLevel;         // If bigger than zero and smaller than the interrupt resistance level of an entity, the effect won't be applied.
    public int Priority;                    // If the entity is already affected by a rotation, the rotation with the lower priority will be cancelled. 

    public ActionRotation()
    {
        Transform = new TransformData();
        MaxDuration = 1.0f;
    }
}
