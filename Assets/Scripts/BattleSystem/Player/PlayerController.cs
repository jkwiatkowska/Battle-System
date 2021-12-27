using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    const float Gravity = -9.8f;

    [SerializeField] float Speed = 4.0f;
    [SerializeField] float JumpHeight = 1.0f;
    [SerializeField] CapsuleCollider Collider;
    [SerializeField] LayerMask TerrainLayers;
    [SerializeField] PlayerCamera Camera;
    [SerializeField] Rigidbody Rigidbody;
    [SerializeField] float GroundCheckSphereRadius;

    Vector3 MovementVector;
    Vector3 Velocity;
    Vector3 GroundCheckSphereOffset;

    float GravitationalForce;
    bool IsGrounded;

    private void Awake()
    {
        GravitationalForce = Gravity;
        Velocity = new Vector3();
        GroundCheckSphereOffset = new Vector3(0.0f, GroundCheckSphereRadius, 0.0f);
    }

    void FixedUpdate()
    {
        ControlPlayer();
    }

    void ControlPlayer()
    {
        IsGrounded = Velocity.y <= 0.0f && Physics.CheckSphere(transform.position + GroundCheckSphereOffset, GroundCheckSphereRadius, TerrainLayers);

        if (Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f)
        {
            MovementVector = (Input.GetAxis("Horizontal") * Camera.GetPlayerXVector() + -Input.GetAxis("Vertical") * Camera.GetPlayerZVector());

            MovementVector.Normalize();
            transform.position += MovementVector * Speed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.LookRotation(MovementVector, Vector3.up);
        }
        else
        {
            MovementVector = Vector3.zero;
        }

        if (IsGrounded)
        {
            if (Velocity.y < 0.0f)
            {
                Velocity.y = 0.0f;
            }

            if (Input.GetButton("Jump"))
            {
                Velocity.y += Mathf.Sqrt(JumpHeight * -2.0f * GravitationalForce);
            }
        }
        else
        {
            Velocity.y += GravitationalForce * Time.deltaTime;
        }

        gameObject.transform.position += (Velocity * Time.deltaTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collided with {collision.gameObject.name}");
    }
}
