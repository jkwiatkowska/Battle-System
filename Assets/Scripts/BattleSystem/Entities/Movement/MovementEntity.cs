using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementEntity : MonoBehaviour
{
    [SerializeField] float GroundCheckSphereRadius;
    Entity Parent;

    Vector3 Velocity;
    Vector3 GroundCheckSphereOffset;

    public float GravitationalForce;
    public bool IsGrounded { get; protected set; }

    float MovementLock;

    public float LastMoved      { get; private set; }
    public float LastJumped     { get; private set; }

    public virtual void Setup(Entity parent)
    {
        Parent = parent;
        GravitationalForce = Constants.Gravity;
        Velocity = new Vector3();
        GroundCheckSphereOffset = new Vector3(0.0f, GroundCheckSphereRadius, 0.0f);
    }

    protected virtual void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void LockMovement(float time)
    {
        MovementLock = BattleSystem.Time + time;
    }

    public bool IsMovementLocked
    {
        get
        {
            return MovementLock > BattleSystem.Time;
        }
    }

    public virtual void Move(Vector3 direction, bool updateRotation, float speedMultiplier = 1.0f)
    {
        if (IsMovementLocked)
        {
            return;
        }

        direction.Normalize();

        transform.position += direction * Formulae.EntityMovementSpeed(Parent) * speedMultiplier * Time.fixedDeltaTime;
        if (updateRotation)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        LastMoved = BattleSystem.Time;
    }

    public void Jump()
    {
        if (IsMovementLocked)
        {
            return;
        }

        if (IsGrounded)
        {
            Velocity.y += Mathf.Sqrt(Formulae.EntityJumpHeight(Parent) * -2.0f * GravitationalForce);
        }

        LastJumped = BattleSystem.Time;
    }

    public void RotateY(float rotationPerSecond, ref float targetRotation)
    {
        var rotation = rotationPerSecond * Time.fixedDeltaTime;
        if (targetRotation < 0.0f)
        {
            rotation *= -1.0f;
            if (rotation < targetRotation)
            {
                rotation = targetRotation;
            }
        }
        else if (rotation > targetRotation)
        {
            rotation = targetRotation;
        }

        transform.rotation *= Quaternion.Euler(0.0f, rotation, 0.0f);

        targetRotation -= rotation;
    }

    public void RotateTowardPosition(Vector3 targetPosition, float rotationSpeed)
    {
        var targetDirection = targetPosition - transform.position;
        var initialForward = transform.forward;
;
        var direction = Vector3.RotateTowards(initialForward, targetDirection, Time.fixedDeltaTime * rotationSpeed, 0.0f);

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public IEnumerator RotateTowardCoroutine(Vector3 targetPosition, float rotationMultiplier = 1.0f)
    {
        var targetDirection = targetPosition - transform.position;
        targetDirection.y = 0.0f;
        var initialForward = transform.forward;

        var speed = Formulae.EntityRotateSpeed(Parent) * rotationMultiplier;
        var startTime = BattleSystem.Time;

        do
        {
            var timeElapsed = BattleSystem.Time - startTime;
            var direction = Vector3.RotateTowards(initialForward, targetDirection, timeElapsed * speed, 0.0f);

            transform.rotation = Quaternion.LookRotation(direction);

            yield return null;
        }
        while (Vector3.Angle(transform.forward, targetDirection) > 0.5f);
    }

    void UpdateVelocity()
    {
        IsGrounded = Velocity.y <= 0.0f && Physics.CheckSphere(transform.position + GroundCheckSphereOffset, GroundCheckSphereRadius, BattleSystem.Instance.TerrainLayers);

        if (IsGrounded)
        {
            if (Velocity.y < 0.0f)
            {
                Velocity.y = 0.0f;
            }
        }
        else
        {
            Velocity.y += GravitationalForce * Time.deltaTime;
        }

        gameObject.transform.position += (Velocity * Time.deltaTime);
    }
}
