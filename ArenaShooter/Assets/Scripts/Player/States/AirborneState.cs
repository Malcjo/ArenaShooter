using UnityEngine;

public class AirborneState : IPlayerState
{
    PlayerContext ctx;
    public AirborneState(PlayerContext c) { ctx = c; }

    public void Enter() { }
    public void Exit() { }

    public void HandleInput()
    {
        if (ctx.weaponWheelHeld) { ctx.StateMachine.SetState(new WeaponWheelState(ctx)); return; }
        if (ctx.dashPressed) { ctx.StateMachine.SetState(new DashState(ctx)); return; }
        if (ctx.jumpPressed && ctx.wallSliding) ctx.jumpPressed = false;
    }

    public void Tick()
    {
        ctx.LookTick();
        float dt = Time.deltaTime;



        // Wall-slide support (if you keep it) chooses gravity
        ctx.wallSliding = ctx.CheckWallSlide(out var _);
        float g = ctx.wallSliding ? ctx.wallSlideGravity : ctx.gravity;
        ctx.velocity.y += g * dt;

        // --- ledge snap: try before applying our Move ---
        if (ctx.coyoteTimer > 0f) ctx.coyoteTimer -= dt;
        if (ctx.ledgeSnapTimer > 0f) ctx.ledgeSnapTimer -= dt;

        //decrement jump buffer timer
        if (ctx.jumpBufferTimer > 0f) ctx.jumpBufferTimer -= dt;

        // TryLedgeSnapUp() stays exactly where you have it — before Move:
        bool snapped = ctx.TryLedgeSnapUp();



        Vector3 desiredDirection = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.moveInput);
        float desiredSpeed = ctx.EffectiveGroundMoveSpeed;

        MovementQuake.AirAccelerate(ref ctx.velocity, desiredDirection, desiredSpeed, ctx.airAccelQ, ctx.EffectiveAirPerFrameDesiredSpeedCap, dt);

        if (ctx.airControlQ > 0f)
        {
            MovementQuake.AirControl(ref ctx.velocity, desiredDirection, desiredSpeed, ctx.airControlQ, dt);
        }
        MovementQuake.ClampHorizontalSpeed(ref ctx.velocity, ctx.EffectiveMaxHorizontalSpeed);

        // Wall jump (keep if you like)
        if (ctx.wallSliding && ctx.jumpPressed)
        {
            Vector3 wjLocal = new Vector3(Mathf.Sign(ctx.moveInput.x) >= 0 ? -ctx.wallJumpDir.x : ctx.wallJumpDir.x, ctx.wallJumpDir.y, 0f);
            Vector3 wj = ctx.transform.TransformDirection(wjLocal.normalized);
            ctx.velocity = new Vector3(wj.x * ctx.moveSpeed, ctx.wallJumpForce, wj.z * ctx.moveSpeed);
            ctx.jumpPressed = false;
        }

        // Coyote jump: allow a grounded-style jump for a short window after leaving ground
        if (ctx.coyoteTimer > 0f && ctx.jumpPressed)
        {
            ctx.velocity.y = ctx.jumpForce;
            ctx.jumpPressed = false;
            ctx.coyoteTimer = 0f; // consume the grace
        }

        if (ctx.characterController.isGrounded)
        {
            ctx.StateMachine.SetState(new GroundedState(ctx));
        }

        ctx.characterController.Move(ctx.velocity * dt);
    }


    public void FixedTick() { }
}
