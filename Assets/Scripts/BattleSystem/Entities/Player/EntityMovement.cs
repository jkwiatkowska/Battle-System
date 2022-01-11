using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    [SerializeField] float Speed = 4.0f;
    [SerializeField] float JumpHeight = 1.0f;
    [SerializeField] CapsuleCollider Collider;
    [SerializeField] LayerMask TerrainLayers;
    [SerializeField] float GroundCheckSphereRadius;
    PlayerCamera Camera;

    Vector3 Velocity;
    Vector3 GroundCheckSphereOffset;

    float GravitationalForce;
    bool IsGrounded;

    float MovementLock;

    public float LastMoved { get; private set; }

    private void Awake()
    {
        Camera = FindObjectOfType<PlayerCamera>();
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
        transform.position += movementVector * Speed * Time.fixedDeltaTime;
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
            Velocity.y += Mathf.Sqrt(JumpHeight * -2.0f * GravitationalForce);
        }
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
