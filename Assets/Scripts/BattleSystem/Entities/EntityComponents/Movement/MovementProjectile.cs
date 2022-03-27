using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementProjectile : MovementEntity
{
    Projectile Projectile;
    public override void Setup(Entity parent)
    {
        base.Setup(parent);

        Projectile = parent as Projectile;
        if (Projectile == null)
        {
            Debug.LogError($"Entity {parent.EntityUID} is not a projectile.");
        }    
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
