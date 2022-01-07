using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Input : MonoBehaviour
{
    [SerializeField] PlayerCamera PlayerCamera;
    [SerializeField] EntityMovement PlayerMovement;
    [SerializeField] PlayerTargetingSystem PlayerTargetingSystem;
    void Update()
    {
        // Cursor
        if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                PlayerTargetingSystem.SelectWithMouse();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Camera
            if (UnityEngine.Input.GetAxis("Mouse X") != 0.0f || UnityEngine.Input.GetAxis("Mouse Y") != 0.0f)
            {
                PlayerCamera.Rotate(new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y")));

            }

            if (UnityEngine.Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                PlayerCamera.Zoom(UnityEngine.Input.GetAxis("Mouse ScrollWheel"));
            }
        }

        // Pause/quit debug mode
        if (UnityEngine.Input.GetKey(KeyCode.P))
        {
            Debug.Break();
        }

        if (UnityEngine.Input.GetKey(KeyCode.Escape))
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    private void FixedUpdate()
    {
        // Player controls
        if (UnityEngine.Input.GetAxis("Horizontal") != 0.0f || UnityEngine.Input.GetAxis("Vertical") != 0.0f)
        {
            PlayerMovement.Move(new Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical")));
        }

        if (UnityEngine.Input.GetButton("Jump"))
        {
            PlayerMovement.Jump();
        }

        if (UnityEngine.Input.GetKey(KeyCode.Tab))
        {
            PlayerTargetingSystem.SwitchTarget();
        }
    }
}