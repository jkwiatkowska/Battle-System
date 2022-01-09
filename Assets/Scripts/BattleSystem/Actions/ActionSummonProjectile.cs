using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSummonProjectile : ActionSummon
{
    public enum eProjectileBehaviour
    {
        PassThrough,
        Disappear,
        Bounce,
        UseSkill
    }

    public string ProjectileID;
    public float StartSpeed;
    public Vector2 SpeedBoundaries;
    public float SpeedChangePerSecond;

    // Some behaviours cancel one another out, but others may be used together
    public List<eProjectileBehaviour> OnEnemyHit;
    public List<eProjectileBehaviour> OnWallHit;
}
