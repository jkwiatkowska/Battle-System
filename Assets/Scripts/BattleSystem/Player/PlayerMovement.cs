using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    const float Gravity = -9.8f;

    [SerializeField] float Speed = 4.0f;
    [SerializeField] float JumpHeight = 1.0f;
    [SerializeField] CapsuleCollider Collider;
    [SerializeField] LayerMask TerrainLayers;
    [SerializeField] PlayerCamera Camera;
    [SerializeField] Rigidbody Rigidbody;
    [SerializeField] float GroundCheckSphereRadius;

    Vector3 Velocity;
    Vector3 GroundCheckSphereOffset;

    float GravitationalForce;
    bool IsGrounded;

    public float LastMoved { get; private set; }

    private void Awake()
    {
        GravitationalForce = Gravity;
        Velocity = new Vector3();
        GroundCheckSphereOffset = new Vector3(0.0f, GroundCheckSphereRadius, 0.0f);
    }

    void FixedUpdate()
    {
        UpdateVelocity();
    }

    public void Move(Vector2 input)
    {
        var movementVector = input.x * Camera.GetPlayerXVector() + -input.y * Camera.GetPlayerZVector();

        movementVector.Normalize();
        transform.position += movementVector * Speed * Time.fixedDeltaTime;
        transform.rotation = Quaternion.LookRotation(movementVector, Vector3.up);

        LastMoved = Time.realtimeSinceStartup;
    }

    public void Jump()
    {
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
