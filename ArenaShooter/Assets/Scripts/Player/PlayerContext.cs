using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController), typeof(PlayerStateMachine))]
public class PlayerContext : MonoBehaviour
{
    [Header("Refs")]
    public Camera cam;
    [HideInInspector] public CharacterController characterController;
    [HideInInspector] public PlayerStateMachine StateMachine;
    [HideInInspector] public PlayerInput playerInput;

    [Header("Move")]
    public float moveSpeed = 7f;
    public float jumpForce = 8f;
    public float gravity = -18f;
    public float airControl = 0.6f;

    [Header("Dash")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.7f;

    [Header("Slide")]
    public float slideSpeed = 12f;
    public float slideDuration = 0.5f;
    public float slideFriction = 8f;
    public float crouchHeight = 1.0f;
    [HideInInspector] public float normalHeight;

    [Header("Wall Slide")]
    public float wallCheckDistance = 0.6f;
    public LayerMask wallMask;
    public float wallSlideGravity = -4f;
    public float wallJumpForce = 9f;
    public Vector2 wallJumpDir = new Vector2(0.8f, 0.6f);

    //LOOK SENSITIVITY
    [Header("Look")]
    public float mouseSensitivity = 0.08f;   // multiply mouse delta
    public float stickSensitivity = 120f;    // degrees/second for gamepad

    //ACCELERATION TOGGLES
    [Header("Acceleration")]
    public bool useAcceleration = true;      // turn on/off acceleration
    public float groundAccel = 60f;          // how fast horizontal velocity changes (units/s^2)
    public float airAccel = 30f;             // same, but airborne
    [Range(0.01f, 1f)]
    public float groundSnapLerp = 1f;        // used if useAcceleration == false (1 = very snappy)


    // Runtime
    [HideInInspector] public Vector2 moveInput, lookInput;
    [HideInInspector] public bool jumpPressed, dashPressed, slidePressed, fireHeld, weaponWheelHeld, interactPressed;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public bool canDash = true, canSlide = true, wallSliding = false;

    float yaw;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        StateMachine = GetComponent<PlayerStateMachine>();
        playerInput = GetComponent<PlayerInput>();
        normalHeight = characterController.height;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        StateMachine.SetState(new GroundedState(this));
    }

    public void LookTick()
    {
        bool usingGamepad = playerInput && playerInput.currentControlScheme == "Gamepad";
        if (usingGamepad)
        {
            yaw += lookInput.x * stickSensitivity * Time.deltaTime;
            float pitch = cam.transform.localEulerAngles.x;
            if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch - (lookInput.y * stickSensitivity * Time.deltaTime), -85f, 85);
            cam.transform.localEulerAngles = new Vector3(pitch,0,0);
        }
        else
        {
            yaw += lookInput.x * mouseSensitivity;
            float pitch = cam.transform.localEulerAngles.x;
            if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch - (lookInput.y * mouseSensitivity), -85f, 85);
            cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    public bool CheckWallSlide(out RaycastHit hit)
    {
        hit = default;
        if (characterController.isGrounded) return false;
        if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallMask)) return true;
        if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallMask)) return true;
        return false;
    }

    public void OnMove(InputAction.CallbackContext context)
        => moveInput = context.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext context)
        => lookInput = context.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) jumpPressed = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed) dashPressed = true;
    }

    public void OnSlide(InputAction.CallbackContext context)
    {
        if (context.performed) slidePressed = true;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        // held or tap — choose what you want:
        fireHeld = context.phase == InputActionPhase.Performed;
    }

    public void OnWeaponWheel(InputAction.CallbackContext context)
    {
        weaponWheelHeld = context.phase == InputActionPhase.Performed;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed) interactPressed = true;
    }
}
