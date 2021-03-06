using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementPlayer : MovementEntity
{
    PlayerCamera Camera;

    public override void Setup(Entity parent)
    {
        base.Setup(parent);
        Camera = FindObjectOfType<PlayerCamera>();
    }    

    public virtual void MovePlayer(Vector2 input, float speedMultiplier = 1.0f)
    {
        if (Entity.IsMovementLocked)
        {
            return;
        }

        var movementVector = input.x * Camera.GetPlayerXVector() + -input.y * Camera.GetPlayerZVector();
        if (movementVector.sqrMagnitude < float.Epsilon)
        {
            return;
        }

        Move(movementVector, true, speedMultiplier);
    }
}
