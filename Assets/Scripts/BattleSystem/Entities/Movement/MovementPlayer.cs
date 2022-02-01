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

    public virtual void MovePlayer(Vector2 direction, float speedMultiplier = 1.0f)
    {
        // To do: Make a player movement class and move this there
        if (IsMovementLocked)
        {
            return;
        }

        var movementVector = direction.x * Camera.GetPlayerXVector() + -direction.y * Camera.GetPlayerZVector();

        Move(movementVector, speedMultiplier);
    }
}
