using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public struct InputFrame
{
    public Vector2 Move; public Vector2 Look;
    public bool JumpPressedEdge; public bool JumpHeld;
    public bool DashPressedEdge; public bool SlidePressedEdge;
    public bool WeaponWheelHeld; public bool InteractPressedEdge;
   public bool fireHeld;

}

public class InputBuffer : MonoBehaviour
{
    [Header("Buffers")]
    public float jumpBufferTime = 0.10f;
    public float dashBufferTime = 0.10f;

    float _jumpBufferTimer, _dashBufferTimer;
    bool _jumpHeld;


    bool _jumpEdgeImmediate, _dashEdgeImmediate;

    // Current frame snapshot (read-only from outside)
    public InputFrame Frame;

    // --- Input System hooks (wire PlayerInput → Unity Events to these) ---
    public void OnMove(InputAction.CallbackContext ctx) => Frame.Move = ctx.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext ctx) => Frame.Look = ctx.ReadValue<Vector2>();

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) { _jumpHeld = true; _jumpBufferTimer = jumpBufferTime; }
        else if (ctx.canceled) _jumpHeld = false;
    }
    public void OnDash(InputAction.CallbackContext ctx) { if (ctx.performed) 
        { 
            _dashBufferTimer = dashBufferTime;
            _dashEdgeImmediate = true;
            Debug.Log("[InputBuffer] Dash started"); 

        } 
    }
    public void OnSlide(InputAction.CallbackContext ctx) { if (ctx.performed) Frame.SlidePressedEdge = true; }
    public void OnWeaponWheel(InputAction.CallbackContext ctx) { Frame.WeaponWheelHeld = ctx.phase == InputActionPhase.Performed; }
    public void OnInteract(InputAction.CallbackContext ctx) { if (ctx.performed) Frame.InteractPressedEdge = true; }
    // held or tap — choose what you want:
    public void OnFire(InputAction.CallbackContext ctx) { Frame.fireHeld = ctx.phase == InputActionPhase.Performed;}

    void Update()
    {
        float dt = Time.deltaTime;
        if (_jumpBufferTimer > 0f) _jumpBufferTimer -= dt;
        if (_dashBufferTimer > 0f) _dashBufferTimer -= dt;

        Frame.JumpHeld = _jumpHeld;


        bool jumpEdge = _jumpEdgeImmediate || _jumpBufferTimer > 0f;
        bool dashEdge = _dashEdgeImmediate || _dashBufferTimer > 0f;


        // Edge flags are consumed by Brain each frame; reset here
        Frame.JumpPressedEdge = _jumpBufferTimer > 0f;
        Frame.DashPressedEdge = _dashBufferTimer > 0f;

        Frame.JumpPressedEdge = jumpEdge;
        Frame.DashPressedEdge = dashEdge;

        if (dashEdge) Debug.Log("[InputBuffer] dash edge visible to FSM");

        ResetFrames();

    }

    // Called by Brain when it consumes the buffered action
    public void ConsumeJump() => _jumpBufferTimer = 0f;
    public void ConsumeDash() => _dashBufferTimer = 0f;


    void ResetFrames()
    {
        _jumpEdgeImmediate = false;
        _dashEdgeImmediate = false;

        Frame.SlidePressedEdge = false;
        Frame.InteractPressedEdge = false;
    }
}
