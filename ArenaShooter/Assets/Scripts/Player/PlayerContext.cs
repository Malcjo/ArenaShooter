using System.Collections;
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

    [Header("Dash Charges")]
    public int maxDashCharges = 2;
    [HideInInspector] public int dashCharges;
    public float dashRefillCooldown = 0.9f;   // time to refill after you’re out
    public bool groundedRequiredForRefill = true;

    [HideInInspector] public bool isDashing;   // blocks queuing while in dash
    Coroutine dashRefillRoutine;

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


    [Header("Quake Movement")]
    public float maxHorizSpeed = 18f;  // soft safety cap for XZ speed
    public float groundFriction = 6f;   // how quickly you bleed speed on ground
    public float groundStopSpeed = 2f;   // minimum speed used by friction
    public float groundAccelQ = 14f;  // ground accelerate
    public float airAccelQ = 12f;  // air accelerate (strafe-jump power)
    public float airMaxSpeed = 14f;  // cap wishspeed in air (prevents instant spikes)
    public float airControlQ = 0f;   // 0..1 (optional extra turning gain in air)
    public bool autoHop = false; // hold jump to auto-bhop


    // === Roguelike Speed Modifiers ===
    // Global affects ALL movement-related speeds.
    [Header("Roguelike Speed Modifiers (Movement)")]
    [Tooltip("Flat amount added to all movement speeds (usually 0).")]
    public float globalMovementFlatBonus = 0f;

    [Tooltip("Additive percentage across all movement speeds. 0.10 = +10%.")]
    public float globalMovementPercentIncrease = 0f;

    [Tooltip("Extra multiplicative factor across all movement speeds. Stacks with percent. (e.g., 1.05 = +5%).")]
    public float globalMovementMultiplier = 1f;

    // Channel-specific modifiers (stack WITH global)
    [Tooltip("Ground running speed bonuses.")]
    public float groundMoveSpeedFlatBonus = 0f;
    public float groundMoveSpeedPercentIncrease = 0f;

    [Tooltip("AIR: per-frame desired-speed cap (controls how hard air-accel can push).")]
    public float airPerFrameDesiredSpeedCapFlatBonus = 0f;
    public float airPerFrameDesiredSpeedCapPercentIncrease = 0f;

    [Tooltip("Dash burst speed bonuses.")]
    public float dashSpeedFlatBonus = 0f;
    public float dashSpeedPercentIncrease = 0f;

    [Tooltip("Slide target speed bonuses.")]
    public float slideSpeedFlatBonus = 0f;
    public float slideSpeedPercentIncrease = 0f;

    [Tooltip("Horizontal clamp: absolute max run speed cap bonuses.")]
    public float maxHorizontalSpeedFlatBonus = 0f;
    public float maxHorizontalSpeedPercentIncrease = 0f;

    // -------- Helpers --------
    public float GlobalMovementFactor => (1f + globalMovementPercentIncrease) * globalMovementMultiplier;

    // Effective values that states should use:
    public float EffectiveGroundMoveSpeed
        => (moveSpeed + globalMovementFlatBonus + groundMoveSpeedFlatBonus)
           * (1f + groundMoveSpeedPercentIncrease)
           * GlobalMovementFactor;

    public float EffectiveDashSpeed
        => (dashSpeed + globalMovementFlatBonus + dashSpeedFlatBonus)
           * (1f + dashSpeedPercentIncrease)
           * GlobalMovementFactor;

    public float EffectiveSlideSpeed
        => (slideSpeed + globalMovementFlatBonus + slideSpeedFlatBonus)
           * (1f + slideSpeedPercentIncrease)
           * GlobalMovementFactor;

    public float EffectiveMaxHorizontalSpeed
        => (maxHorizSpeed + maxHorizontalSpeedFlatBonus)
           * (1f + maxHorizontalSpeedPercentIncrease)
           * GlobalMovementFactor;

    public float EffectiveAirPerFrameDesiredSpeedCap
        => (airMaxSpeed + airPerFrameDesiredSpeedCapFlatBonus)
           * (1f + airPerFrameDesiredSpeedCapPercentIncrease)
           * GlobalMovementFactor;

    // Runtime
    [HideInInspector] public Vector2 moveInput, lookInput;
    [HideInInspector] public bool jumpPressed, dashPressed, slidePressed, fireHeld, weaponWheelHeld, interactPressed;
    [HideInInspector] public Vector3 velocity;
    [HideInInspector] public bool canSlide = true, wallSliding = false;

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
        dashCharges = maxDashCharges; // start with full dashes
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

    public bool CanPressDash()
    {
        // No pressing while currently dashing, and need at least one charge
        return !isDashing && dashCharges > 0;
    }

    public void ConsumeDashCharge()
    {
        dashCharges = Mathf.Max(0, dashCharges - 1);
        if (dashCharges == 0) StartDashRefillIfNeeded();
    }

    public void StartDashRefillIfNeeded()
    {
        if (dashRefillRoutine != null) return;
        dashRefillRoutine = StartCoroutine(DashRefill());
    }

    IEnumerator DashRefill()
    {
        // Must be grounded (if required) to begin refill
        if (groundedRequiredForRefill)
        {
            while (!characterController.isGrounded) yield return null;
        }
        // Wait cooldown, then refill all
        yield return new WaitForSeconds(dashRefillCooldown);
        dashCharges = maxDashCharges;
        dashRefillRoutine = null;
    }

    // A simple API to apply upgrades in code:
    public void ApplyMovementSpeedUpgrade(
        float globalPercentIncrease = 0f,
        float globalFlatBonus = 0f,
        float multiplyAllBy = 1f,
        float groundPercentIncrease = 0f,
        float airCapPercentIncrease = 0f,
        float dashPercentIncrease = 0f,
        float slidePercentIncrease = 0f,
        float maxHorizontalPercentIncrease = 0f)
    {
        globalMovementPercentIncrease += globalPercentIncrease;
        globalMovementFlatBonus += globalFlatBonus;
        if (multiplyAllBy != 1f) globalMovementMultiplier *= multiplyAllBy;

        groundMoveSpeedPercentIncrease += groundPercentIncrease;
        airPerFrameDesiredSpeedCapPercentIncrease += airCapPercentIncrease;
        dashSpeedPercentIncrease += dashPercentIncrease;
        slideSpeedPercentIncrease += slidePercentIncrease;
        maxHorizontalSpeedPercentIncrease += maxHorizontalPercentIncrease;
    }

    public void ResetMovementSpeedModifiers()
    {
        globalMovementFlatBonus = 0f;
        globalMovementPercentIncrease = 0f;
        globalMovementMultiplier = 1f;

        groundMoveSpeedFlatBonus = 0f;
        groundMoveSpeedPercentIncrease = 0f;

        airPerFrameDesiredSpeedCapFlatBonus = 0f;
        airPerFrameDesiredSpeedCapPercentIncrease = 0f;

        dashSpeedFlatBonus = 0f;
        dashSpeedPercentIncrease = 0f;

        slideSpeedFlatBonus = 0f;
        slideSpeedPercentIncrease = 0f;

        maxHorizontalSpeedFlatBonus = 0f;
        maxHorizontalSpeedPercentIncrease = 0f;
    }


    public void OnMove(InputAction.CallbackContext context)
        => moveInput = context.ReadValue<Vector2>();

    public void OnLook(InputAction.CallbackContext context)
        => lookInput = context.ReadValue<Vector2>();


    [HideInInspector] public bool jumpHeld;
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) jumpPressed = true;
        jumpHeld = context.phase == InputActionPhase.Performed || context.phase == InputActionPhase.Started;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if(!CanPressDash()) return;
        dashPressed = true;
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
