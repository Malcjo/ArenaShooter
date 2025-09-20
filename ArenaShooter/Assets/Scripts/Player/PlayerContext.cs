using UnityEngine.InputSystem;
using UnityEngine;

public class PlayerContext : MonoBehaviour
{
    public Camera cam;
    public PlayerStateMachine fsm;
    public CharacterController cc;
    public PlayerInput playerInput;

    public InputBuffer input;
    public PlayerSensors sensors;
    public PlayerStats stats;
    public PlayerMotor motor;

    [HideInInspector] public float yaw;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        fsm = GetComponent<PlayerStateMachine>();
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Start() { fsm.SetState(new GroundedState(this)); }
    public bool UsingGamepad => playerInput && playerInput.currentControlScheme == "Gamepad";
}




//using System.Collections;
//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(CharacterController), typeof(PlayerStateMachine))]
//public class PlayerContext : MonoBehaviour
//{
//    [Header("Refs")]
//    public Camera cam;
//    [HideInInspector] public CharacterController characterController;
//    [HideInInspector] public PlayerStateMachine StateMachine;
//    [HideInInspector] public PlayerInput playerInput;

//    [Header("Move")]
//    public float moveSpeed = 7f;
//    public float jumpForce = 8f;
//    public float gravity = -18f;
//    public float airControl = 0.6f;

//    [Header("Jump Quality")]
//    [Tooltip("Allow pressing jump slightly before landing (seconds). Set 0 to disable.")]
//    public float jumpBufferTime = 0.10f; // 0 disables buffering
//    [HideInInspector] public float jumpBufferTimer;


//    [Header("Dash")]
//    public float dashSpeed = 22f;
//    public float dashDuration = 0.18f;

//    [Header("Dash Charges")]
//    public int maxDashCharges = 2;
//    [HideInInspector] public int dashCharges;
//    public float dashRefillCooldown = 0.9f;   // time to refill after you’re out
//    public bool groundedRequiredForRefill = true;

//    [HideInInspector] public bool isDashing;   // blocks queuing while in dash
//    Coroutine dashRefillRoutine;

//    [Header("Slide")]
//    public float slideSpeed = 12f;
//    public float slideDuration = 0.5f;
//    public float slideFriction = 8f;
//    public float crouchHeight = 1.0f;
//    [HideInInspector] public float normalHeight;

//    [Header("Wall Slide")]
//    public float wallCheckDistance = 0.6f;
//    public LayerMask wallMask;
//    public float wallSlideGravity = -4f;
//    public float wallJumpForce = 9f;
//    public Vector2 wallJumpDir = new Vector2(0.8f, 0.6f);

//    //LOOK SENSITIVITY
//    [Header("Look")]
//    public float mouseSensitivity = 0.08f;   // multiply mouse delta
//    public float stickSensitivity = 120f;    // degrees/second for gamepad


//    [Header("Quake Movement")]
//    public float maxHorizSpeed = 18f;  // soft safety cap for XZ speed
//    public float groundFriction = 6f;   // how quickly you bleed speed on ground
//    public float groundStopSpeed = 2f;   // minimum speed used by friction
//    public float groundAccelQ = 14f;  // ground accelerate
//    public float airAccelQ = 12f;  // air accelerate (strafe-jump power)
//    public float airMaxSpeed = 14f;  // cap wishspeed in air (prevents instant spikes)
//    public float airControlQ = 0f;   // 0..1 (optional extra turning gain in air)
//    public bool autoHop = false; // hold jump to auto-bhop

//    [Header("Grounding Forgiveness")]
//    [Tooltip("Max vertical distance we will 'snap up' onto a ledge when almost on top.")]
//    public float ledgeSnapUpDistance = 0.35f;

//    [Tooltip("How far ahead of feet we probe when checking for a ledge.")]
//    public float ledgeSnapProbeForward = 0.30f;

//    [Tooltip("Probe radius for the ledge check (keep <= CharacterController.radius).")]
//    public float ledgeSnapProbeRadius = 0.20f;

//    [Tooltip("Layers considered 'walkable ground' for ledge snap.")]
//    public LayerMask groundMask = ~0; // set in Inspector

//    [Tooltip("Extra forgiveness for jumping after leaving ground.")]
//    public float coyoteTime = 0.10f; // optional

//    [HideInInspector] public float coyoteTimer; // runtime

//    [Header("Ledge Snap Gating")]
//    [Tooltip("Master switch for ledge snap. Turn off to test.")]
//    public bool enableLedgeSnap = true;

//    [Tooltip("Time window after leaving ground when ledge snap is allowed.")]
//    public float ledgeSnapActiveWindow = 0.25f;

//    [Tooltip("Min forward horizontal speed to consider snapping.")]
//    public float ledgeSnapMinForwardSpeed = 1.0f;

//    [Tooltip("Min alignment with forward (0..1). 0.35 ~ roughly facing forward.")]
//    [Range(0f, 1f)] public float ledgeSnapMinForwardDot = 0.35f;

//    [Tooltip("Debug draw snap probes in Scene view.")]
//    public bool ledgeSnapDebugDraw = false;

//    [HideInInspector] public float ledgeSnapTimer;   // counts down in air
//    [HideInInspector] public bool ledgeSnapConsumed; // avoid multiple snaps per airtime



//    // === Roguelike Speed Modifiers ===
//    // Global affects ALL movement-related speeds.
//    [Header("Roguelike Speed Modifiers (Movement)")]
//    [Tooltip("Flat amount added to all movement speeds (usually 0).")]
//    public float globalMovementFlatBonus = 0f;

//    [Tooltip("Additive percentage across all movement speeds. 0.10 = +10%.")]
//    public float globalMovementPercentIncrease = 0f;

//    [Tooltip("Extra multiplicative factor across all movement speeds. Stacks with percent. (e.g., 1.05 = +5%).")]
//    public float globalMovementMultiplier = 1f;

//    // Channel-specific modifiers (stack WITH global)
//    [Tooltip("Ground running speed bonuses.")]
//    public float groundMoveSpeedFlatBonus = 0f;
//    public float groundMoveSpeedPercentIncrease = 0f;

//    [Tooltip("AIR: per-frame desired-speed cap (controls how hard air-accel can push).")]
//    public float airPerFrameDesiredSpeedCapFlatBonus = 0f;
//    public float airPerFrameDesiredSpeedCapPercentIncrease = 0f;

//    [Tooltip("Dash burst speed bonuses.")]
//    public float dashSpeedFlatBonus = 0f;
//    public float dashSpeedPercentIncrease = 0f;

//    [Tooltip("Slide target speed bonuses.")]
//    public float slideSpeedFlatBonus = 0f;
//    public float slideSpeedPercentIncrease = 0f;

//    [Tooltip("Horizontal clamp: absolute max run speed cap bonuses.")]
//    public float maxHorizontalSpeedFlatBonus = 0f;
//    public float maxHorizontalSpeedPercentIncrease = 0f;

//    // -------- Helpers --------
//    public float GlobalMovementFactor => (1f + globalMovementPercentIncrease) * globalMovementMultiplier;

//    // Effective values that states should use:
//    public float EffectiveGroundMoveSpeed
//        => (moveSpeed + globalMovementFlatBonus + groundMoveSpeedFlatBonus)
//           * (1f + groundMoveSpeedPercentIncrease)
//           * GlobalMovementFactor;

//    public float EffectiveDashSpeed
//        => (dashSpeed + globalMovementFlatBonus + dashSpeedFlatBonus)
//           * (1f + dashSpeedPercentIncrease)
//           * GlobalMovementFactor;

//    public float EffectiveSlideSpeed
//        => (slideSpeed + globalMovementFlatBonus + slideSpeedFlatBonus)
//           * (1f + slideSpeedPercentIncrease)
//           * GlobalMovementFactor;

//    public float EffectiveMaxHorizontalSpeed
//        => (maxHorizSpeed + maxHorizontalSpeedFlatBonus)
//           * (1f + maxHorizontalSpeedPercentIncrease)
//           * GlobalMovementFactor;

//    public float EffectiveAirPerFrameDesiredSpeedCap
//        => (airMaxSpeed + airPerFrameDesiredSpeedCapFlatBonus)
//           * (1f + airPerFrameDesiredSpeedCapPercentIncrease)
//           * GlobalMovementFactor;



//    // Runtime
//    [HideInInspector] public Vector2 moveInput, lookInput;
//    [HideInInspector] public bool jumpPressed, dashPressed, slidePressed, fireHeld, weaponWheelHeld, interactPressed;
//    [HideInInspector] public Vector3 velocity;
//    [HideInInspector] public bool canSlide = true, wallSliding = false;
//    [HideInInspector] public bool jumpHeld;

//    float yaw;

//    void Awake()
//    {
//        characterController = GetComponent<CharacterController>();
//        StateMachine = GetComponent<PlayerStateMachine>();
//        playerInput = GetComponent<PlayerInput>();
//        normalHeight = characterController.height;
//        Cursor.lockState = CursorLockMode.Locked;
//    }

//    void Start()
//    {
//        dashCharges = maxDashCharges; // start with full dashes
//        StateMachine.SetState(new GroundedState(this));
//    }

//    public void LookTick()
//    {
//        bool usingGamepad = playerInput && playerInput.currentControlScheme == "Gamepad";
//        if (usingGamepad)
//        {
//            yaw += lookInput.x * stickSensitivity * Time.deltaTime;
//            float pitch = cam.transform.localEulerAngles.x;
//            if (pitch > 180) pitch -= 360;
//            pitch = Mathf.Clamp(pitch - (lookInput.y * stickSensitivity * Time.deltaTime), -85f, 85);
//            cam.transform.localEulerAngles = new Vector3(pitch,0,0);
//        }
//        else
//        {
//            yaw += lookInput.x * mouseSensitivity;
//            float pitch = cam.transform.localEulerAngles.x;
//            if (pitch > 180) pitch -= 360;
//            pitch = Mathf.Clamp(pitch - (lookInput.y * mouseSensitivity), -85f, 85);
//            cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
//        }
//        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
//    }

//    public bool CheckWallSlide(out RaycastHit hit)
//    {
//        hit = default;
//        if (characterController.isGrounded) return false;
//        if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallMask)) return true;
//        if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallMask)) return true;
//        return false;
//    }

//    /// <summary>
//    /// If we are descending and very close to a flat, walkable top surface
//    /// slightly ahead of our feet, snap the controller upward onto it.
//    /// Returns true if we snapped.
//    /// </summary>
//    public bool TryLedgeSnapUp()
//    {
//        if (!enableLedgeSnap) return false;
//        if (characterController.isGrounded) return false;    // only in air
//        if (velocity.y > 0f) return false;                   // only while falling
//        if (ledgeSnapConsumed) return false;                 // one snap per airtime
//        if (ledgeSnapTimer <= 0f) return false;              // only shortly after leaving ground

//        // Need forward intent
//        Vector3 flatVel = new Vector3(velocity.x, 0f, velocity.z);
//        float horizSpeed = flatVel.magnitude;
//        if (horizSpeed < ledgeSnapMinForwardSpeed) return false;

//        Vector3 fwd = transform.forward;
//        float forwardDot = horizSpeed > 0f ? Vector3.Dot(flatVel.normalized, fwd) : 0f;
//        if (forwardDot < ledgeSnapMinForwardDot) return false;

//        // Feet position
//        var cc = characterController;
//        float footY = transform.position.y + cc.center.y - (cc.height * 0.5f);

//        // 1) Quick check: something in front to act as a ledge face
//        Vector3 frontProbeOrigin = new Vector3(transform.position.x, footY + 0.1f, transform.position.z);
//        float frontDistance = ledgeSnapProbeForward + ledgeSnapProbeRadius + 0.05f;
//        if (!Physics.SphereCast(frontProbeOrigin, ledgeSnapProbeRadius, fwd, out RaycastHit frontHit, frontDistance, groundMask, QueryTriggerInteraction.Ignore))
//            return false;

//        // 2) Downward probe slightly ahead & above feet to find a walkable top
//        Vector3 downOrigin =
//            new Vector3(transform.position.x, footY + ledgeSnapUpDistance, transform.position.z) +
//            fwd * ledgeSnapProbeForward;

//        float castDist = ledgeSnapUpDistance + 0.05f;
//        if (Physics.SphereCast(downOrigin, ledgeSnapProbeRadius, Vector3.down,
//                               out RaycastHit topHit, castDist, groundMask, QueryTriggerInteraction.Ignore))
//        {
//            // Flat enough?
//            if (Vector3.Angle(topHit.normal, Vector3.up) <= cc.slopeLimit + 0.1f)
//            {
//                float targetFootY = topHit.point.y + cc.skinWidth + 0.001f;
//                float deltaY = targetFootY - footY;
//                if (deltaY > 0f && deltaY <= ledgeSnapUpDistance)
//                {
//                    // Move up and kill downward velocity
//                    cc.Move(new Vector3(0f, deltaY, 0f));
//                    if (velocity.y < 0f) velocity.y = 0f;
//                    ledgeSnapConsumed = true;

//                    if (ledgeSnapDebugDraw)
//                    {
//                        Debug.DrawLine(downOrigin, downOrigin + Vector3.down * castDist, Color.green, 0.1f);
//                        Debug.DrawLine(frontProbeOrigin, frontProbeOrigin + fwd * frontDistance, Color.cyan, 0.1f);
//                    }
//                    return true;
//                }
//            }
//        }

//        if (ledgeSnapDebugDraw)
//        {
//            Debug.DrawLine(downOrigin, downOrigin + Vector3.down * castDist, Color.red, 0.1f);
//            Debug.DrawLine(frontProbeOrigin, frontProbeOrigin + fwd * frontDistance, Color.magenta, 0.1f);
//        }
//        return false;
//    }




//    public bool CanPressDash()
//    {
//        // No pressing while currently dashing, and need at least one charge
//        return !isDashing && dashCharges > 0;
//    }

//    public void ConsumeDashCharge()
//    {
//        dashCharges = Mathf.Max(0, dashCharges - 1);
//        if (dashCharges == 0) StartDashRefillIfNeeded();
//    }

//    public void StartDashRefillIfNeeded()
//    {
//        if (dashRefillRoutine != null) return;
//        dashRefillRoutine = StartCoroutine(DashRefill());
//    }

//    IEnumerator DashRefill()
//    {
//        // Must be grounded (if required) to begin refill
//        if (groundedRequiredForRefill)
//        {
//            while (!characterController.isGrounded) yield return null;
//        }
//        // Wait cooldown, then refill all
//        yield return new WaitForSeconds(dashRefillCooldown);
//        dashCharges = maxDashCharges;
//        dashRefillRoutine = null;
//    }

//    // A simple API to apply upgrades in code:
//    public void ApplyMovementSpeedUpgrade(
//        float globalPercentIncrease = 0f,
//        float globalFlatBonus = 0f,
//        float multiplyAllBy = 1f,
//        float groundPercentIncrease = 0f,
//        float airCapPercentIncrease = 0f,
//        float dashPercentIncrease = 0f,
//        float slidePercentIncrease = 0f,
//        float maxHorizontalPercentIncrease = 0f)
//    {
//        globalMovementPercentIncrease += globalPercentIncrease;
//        globalMovementFlatBonus += globalFlatBonus;
//        if (multiplyAllBy != 1f) globalMovementMultiplier *= multiplyAllBy;

//        groundMoveSpeedPercentIncrease += groundPercentIncrease;
//        airPerFrameDesiredSpeedCapPercentIncrease += airCapPercentIncrease;
//        dashSpeedPercentIncrease += dashPercentIncrease;
//        slideSpeedPercentIncrease += slidePercentIncrease;
//        maxHorizontalSpeedPercentIncrease += maxHorizontalPercentIncrease;
//    }

//    public void ResetMovementSpeedModifiers()
//    {
//        globalMovementFlatBonus = 0f;
//        globalMovementPercentIncrease = 0f;
//        globalMovementMultiplier = 1f;

//        groundMoveSpeedFlatBonus = 0f;
//        groundMoveSpeedPercentIncrease = 0f;

//        airPerFrameDesiredSpeedCapFlatBonus = 0f;
//        airPerFrameDesiredSpeedCapPercentIncrease = 0f;

//        dashSpeedFlatBonus = 0f;
//        dashSpeedPercentIncrease = 0f;

//        slideSpeedFlatBonus = 0f;
//        slideSpeedPercentIncrease = 0f;

//        maxHorizontalSpeedFlatBonus = 0f;
//        maxHorizontalSpeedPercentIncrease = 0f;
//    }


//    public void OnMove(InputAction.CallbackContext context)
//        => moveInput = context.ReadValue<Vector2>();

//    public void OnLook(InputAction.CallbackContext context)
//        => lookInput = context.ReadValue<Vector2>();



//    public void OnJump(InputAction.CallbackContext context)
//    {
//        if (context.performed)
//        {
//            jumpHeld = true;

//            // Is a jump actionable RIGHT NOW?
//            bool canJumpNow = characterController.isGrounded || coyoteTimer > 0f || wallSliding;

//            if (canJumpNow)
//            {
//                // Edge-trigger jump immediately
//                jumpPressed = true;
//            }
//            else
//            {
//                // Not actionable: start (or refresh) a short buffer that will expire
//                if (jumpBufferTime > 0f)
//                    jumpBufferTimer = jumpBufferTime;
//                // If jumpBufferTime == 0, we simply ignore mid-air presses (no storage)
//            }
//        }
//        else if (context.canceled)
//        {
//            jumpHeld = false;
//        }
//    }

//    public void OnDash(InputAction.CallbackContext context)
//    {
//        if (!context.performed) return;
//        if(!CanPressDash()) return;
//        dashPressed = true;
//    }

//    public void OnSlide(InputAction.CallbackContext context)
//    {
//        if (context.performed) slidePressed = true;
//    }

//    public void OnFire(InputAction.CallbackContext context)
//    {
//        // held or tap — choose what you want:
//        fireHeld = context.phase == InputActionPhase.Performed;
//    }

//    public void OnWeaponWheel(InputAction.CallbackContext context)
//    {
//        weaponWheelHeld = context.phase == InputActionPhase.Performed;
//    }

//    public void OnInteract(InputAction.CallbackContext context)
//    {
//        if (context.performed) interactPressed = true;
//    }
//}
