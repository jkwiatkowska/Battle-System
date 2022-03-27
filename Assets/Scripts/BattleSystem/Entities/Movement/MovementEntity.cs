using System;
using System.Collections;
using UnityEngine;

public class MovementEntity : MonoBehaviour
{
    [SerializeField] float GroundCheckSphereRadius;
    protected Entity Entity;

    [NonSerialized] public Vector3 Velocity;
    [NonSerialized] public float GravitationalForce;
    public bool IsGrounded                              { get; protected set; }
    Vector3 GroundCheckSphereOffset;

    public bool IsRunning                               { get; protected set; }

    public float LastMoved                              { get; protected set; }
    public float LastJumped                             { get; protected set; }

    public virtual void Setup(Entity entity)
    {
        Entity = entity;
        GravitationalForce = Constants.Gravity;
        Velocity = new Vector3();
        GroundCheckSphereOffset = new Vector3(0.0f, GroundCheckSphereRadius - Constants.Epsilon, 0.0f);
    }

    protected virtual void FixedUpdate()
    {
        UpdateVelocity();
    }

    public virtual void SetRunning(bool running)
    {
        if (running)
        {
            var movementData = Entity.EntityData.Movement;
            if (movementData.ConsumeResourceWhenRunning)
            {
                var drain = movementData.RunResourcePerSecond.IncomingValue(Entity, Entity.EntityAttributes()) * Time.fixedDeltaTime;
                if (Entity.ResourcesCurrent.ContainsKey(movementData.RunResource) &&
                    Entity.ResourcesCurrent[movementData.RunResource] >= drain)
                {
                    Entity.ApplyChangeToResource(movementData.RunResource, -drain);
                    IsRunning = true;
                }
                else
                {
                    IsRunning = false;
                }
            }
        }
        else
        {
            IsRunning = false;
        }
    }

    public virtual Vector3 Move(Vector3 direction, bool updateRotation, float speedMultiplier = 1.0f, bool updateLastMoved = true)
    {
        if (Entity.IsMovementLocked)
        {
            return Vector3.zero;
        }

        direction.Normalize();

        var movement = direction * GetEntityMovement(Entity, Time.fixedDeltaTime, speedMultiplier);
        transform.position += movement;
        if (updateRotation)
        {
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        if (updateLastMoved)
        {
            LastMoved = BattleSystem.Time;
        }

        return movement;
    }

    public static float GetEntityMovement(Entity entity, float timeElapsed, float speedMultiplier = 1.0f)
    {
        return Formulae.EntityMovementSpeed(entity, entity.Movement.IsRunning) * speedMultiplier * timeElapsed;
    }

    public void Jump()
    {
        if (Entity.IsJumpingLocked)
        {
            return;
        }

        if (IsGrounded)
        {
            Velocity.y += Mathf.Sqrt(Formulae.EntityJumpHeight(Entity) * -2.0f * GravitationalForce);
        }

        LastJumped = BattleSystem.Time;
    }

    public void ApplyForce(Vector3 force)
    {
        Velocity += force;
    }

    public void Launch(Vector3 targetPosition, float angle)
    {
        float xzDistance = Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(targetPosition.x, 0.0f, targetPosition.z));
        float yDistance = Mathf.Abs(targetPosition.y - transform.position.y);
        float tanAlpha = Mathf.Tan(angle * Mathf.Deg2Rad);

        float zSpeed = Mathf.Sqrt(GravitationalForce * xzDistance * xzDistance / (2.0f * (yDistance - xzDistance * tanAlpha)));
        float ySpeed = tanAlpha * zSpeed;

        Vector3 localVelocity = new Vector3(0f, ySpeed, zSpeed);
        Velocity = transform.TransformDirection(localVelocity);
    }

    public void RotateY(float rotationPerSecond, ref float targetRotation)
    {
        var rotation = GetRotation(rotationPerSecond, Time.fixedDeltaTime, ref targetRotation);

        transform.rotation *= Quaternion.Euler(0.0f, rotation, 0.0f);

        targetRotation -= rotation;
    }

    public static float GetRotation(float rotationPerSecond, float timeElapsed, ref float targetRotation)
    {
        var rotation = rotationPerSecond * timeElapsed;
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

        return rotation;
    }

    public void RotateTowardPosition(Vector3 targetPosition)
    {
        var rotationSpeed = Formulae.EntityRotateSpeed(Entity);
        RotateTowardPosition(targetPosition, rotationSpeed);
    }

    public void RotateTowardPosition(Vector3 targetPosition, float rotationSpeed)
    {
        var targetDirection = targetPosition - transform.position;
        targetDirection.y = 0.0f;
        var initialForward = transform.forward;

        var direction = Vector3.RotateTowards(initialForward, targetDirection, Time.fixedDeltaTime * rotationSpeed, 0.0f);

        transform.rotation = Quaternion.LookRotation(direction);
    }

    public IEnumerator RotateTowardCoroutine(Vector3 targetPosition, float rotationMultiplier = 1.0f)
    {
        var targetDirection = targetPosition - transform.position;
        targetDirection.y = 0.0f;
        var initialForward = transform.forward;

        var speed = Formulae.EntityRotateSpeed(Entity) * rotationMultiplier;
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
        IsGrounded = Velocity.y <= Constants.Epsilon && Physics.CheckSphere(transform.position + GroundCheckSphereOffset, GroundCheckSphereRadius, BattleSystem.Instance.TerrainLayers);

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

        if (Velocity != Vector3.zero)
        {
            gameObject.transform.position += Velocity * Time.deltaTime;
        }
    }
}
