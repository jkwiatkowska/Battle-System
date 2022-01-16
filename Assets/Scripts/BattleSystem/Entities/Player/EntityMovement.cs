using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    [SerializeField] LayerMask TerrainLayers;
    [SerializeField] float GroundCheckSphereRadius;
    Entity Parent;
    CapsuleCollider Collider;
    PlayerCamera Camera;

    Vector3 Velocity;
    Vector3 GroundCheckSphereOffset;

    float GravitationalForce;
    bool IsGrounded;

    float MovementLock;

    public float LastMoved { get; private set; }

    public void Setup(Entity parent)
    {
        Parent = parent;
        Camera = FindObjectOfType<PlayerCamera>();
        Collider = GetComponentInChildren<CapsuleCollider>();
        GravitationalForce = Constants.Gravity;
        Velocity = new Vector3();
        GroundCheckSphereOffset = new Vector3(0.0f, GroundCheckSphereRadius, 0.0f);
    }

    void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void LockMovement(float time)
    {
        MovementLock = BattleSystem.TimeSinceStart + time;
    }

    public bool IsMovementLocked
    {
        get
        {
            return MovementLock > BattleSystem.TimeSinceStart;
        }
    }

    public void Move(Vector2 input)
    {
        if (IsMovementLocked)
        {
            return;
        }

        var movementVector = input.x * Camera.GetPlayerXVector() + -input.y * Camera.GetPlayerZVector();

        movementVector.Normalize();
        transform.position += movementVector * Formulae.EntityMovementSpeed(Parent) * Time.fixedDeltaTime;
        transform.rotation = Quaternion.LookRotation(movementVector, Vector3.up);

        LastMoved = BattleSystem.TimeSinceStart;
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
    }

    public void RotateToward(Vector3 targetPosition)
    {
        StartCoroutine(RotateTowardCoroutine(targetPosition));
    }

    IEnumerator RotateTowardCoroutine(Vector3 targetPosition)
    {
        var targetDirection = targetPosition - transform.position;
        var initialForward = transform.forward;

        var speed = Formulae.EntityRotateSpeed(Parent);
        var startTime = BattleSystem.TimeSinceStart;

        do
        {
            var timeElapsed = BattleSystem.TimeSinceStart - startTime;
            var direction = Vector3.RotateTowards(initialForward, targetDirection, timeElapsed * speed, 0.0f);

            transform.rotation = Quaternion.LookRotation(direction);

            yield return null;
        }
        while (Vector3.Angle(transform.forward, targetDirection) > 0.5f);
    }

    void UpdateVelocity()
    {
        IsGrounded = Velocity.y <= 0.0f && Physics.CheckSphere(transform.position + GroundCheckSphereOffset, GroundCheckSphereRadius, TerrainLayers);

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
