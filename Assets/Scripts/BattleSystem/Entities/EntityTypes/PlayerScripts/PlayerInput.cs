using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] EntityPlayer Player;
    PlayerCamera PlayerCamera;
    MovementPlayer PlayerMovement;
    TargetingSystemPlayer PlayerTargetingSystem;

    Dictionary<KeyCode, string> SkillSets = new Dictionary<KeyCode, string>()
    {
        [KeyCode.F1] = "Player Skills",
        [KeyCode.F2] = "Player Status Effects",
        [KeyCode.F3] = "Player Skills 2",
        [KeyCode.F4] = "Player Advanced Skills",
        [KeyCode.F5] = "Player Noelle",
    };

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

        SkillSetChange();

#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.Escape))
        {
            UnityEditor.EditorApplication.isPlaying = false;
        }
#endif
    }

    void FixedUpdate()
    {
        PlayerMovementControls();
    }

    void SkillSetChange()
    {
        foreach (var set in SkillSets)
        {
            if (Input.GetKey(set.Key))
            {
                Player.OnDeath();

                MessageHUD.Instance.DisplayMessage($"Skill Set: [{set.Value}]", Color.cyan);
                Player.Setup(set.Value, Player.Level);
            }
        }
    }

    void PlayerMovementControls()
    {
        if (Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                Player.Movement.SetRunning(true);
            }
            else
            {
                Player.Movement.SetRunning(false);
            }

            PlayerMovement.MovePlayer(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }

        if (Input.GetButton("Jump"))
        {
            PlayerMovement.Jump();
        }
    }

    void PlayerControls()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            PlayerTargetingSystem.SwitchTarget();
            OnInput();
        }
    }

    void OnInput()
    {
        
    }

}