using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] EntityPlayer Player;
    [SerializeField] float InputCooldown = 0.25f;
    PlayerCamera PlayerCamera;
    MovementPlayer PlayerMovement;
    TargetingSystemPlayer PlayerTargetingSystem;

    float LastInput;

    void Awake()
    {
        PlayerCamera = FindObjectOfType<PlayerCamera>();
        PlayerMovement = Player.GetComponentInChildren<MovementPlayer>();
        PlayerTargetingSystem = Player.GetComponentInChildren<TargetingSystemPlayer>();
    }

    void Update()
    {
        // Cursor
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (Input.GetMouseButtonDown(0))
            {
                PlayerTargetingSystem.SelectWithMouse();
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Camera
            if (Input.GetAxis("Mouse X") != 0.0f || UnityEngine.Input.GetAxis("Mouse Y") != 0.0f)
            {
                PlayerCamera.Rotate(new Vector2(UnityEngine.Input.GetAxis("Mouse X"), UnityEngine.Input.GetAxis("Mouse Y")));

            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                PlayerCamera.Zoom(UnityEngine.Input.GetAxis("Mouse ScrollWheel"));
            }
        }

        PlayerControls();

        // Pause/quit debug mode
        if (Input.GetKey(KeyCode.P))
        {
            Debug.Break();
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }

    void FixedUpdate()
    {
        PlayerMovementControls();
    }

    void PlayerMovementControls()
    {
        if (Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f)
        {
            PlayerMovement.MovePlayer(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }

        if (Input.GetButton("Jump"))
        {
            PlayerMovement.Jump();
        }
    }

    void PlayerControls()
    {
        if (ReadInput())
        {
            if (Input.GetKey(KeyCode.Tab))
            {
                PlayerTargetingSystem.SwitchTarget();
                OnInput();
            }

            foreach (var skill in Player.PlayerSkills)
            {
                if (Input.GetKeyDown(skill.Key))
                {
                    Player.TryUseSkill(skill.Value);
                    OnInput();
                }
            }
        }
    }

    void OnInput()
    {
        LastInput = BattleSystem.Time + InputCooldown;
    }

    bool ReadInput()
    {
        return LastInput - BattleSystem.Time <= 0.0f;
    }
}