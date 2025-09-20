using UnityEngine;

public class GroundedState : IPlayerState
{
    PlayerContext ctx;
    public GroundedState(PlayerContext c) { ctx = c; }

    public void Enter() 
    {

        ctx.coyoteTimer = ctx.coyoteTime;   // keep jump forgiving after step-off
        ctx.ledgeSnapConsumed = false;      // allow snap again next airtime

        if (ctx.velocity.y < 0) ctx.velocity.y = -2f; 
    }
    public void Exit() 
    {

        ctx.ledgeSnapTimer = ctx.ledgeSnapActiveWindow;
    }

    public void HandleInput()
    {
        if (ctx.weaponWheelHeld) { ctx.StateMachine.SetState(new WeaponWheelState(ctx)); return; }
        if (ctx.dashPressed) { ctx.StateMachine.SetState(new DashState(ctx)); return; }
        if (ctx.slidePressed && ctx.canSlide && ctx.moveInput.sqrMagnitude > 0.2f) { ctx.StateMachine.SetState(new SlideState(ctx)); return; }
        if (ctx.jumpPressed)
        {
            ctx.velocity.y = ctx.jumpForce;
            ctx.jumpPressed = false;
            ctx.StateMachine.SetState(new AirborneState(ctx));
        }
    }

    public void Tick()
    {
        ctx.LookTick();
        float dt = Time.deltaTime;
        
        if (ctx.jumpBufferTimer > 0f) ctx.jumpBufferTimer -= dt;

        bool bufferedJump = ctx.jumpBufferTimer > 0f;
        bool wantJump = ctx.jumpPressed || bufferedJump || (ctx.autoHop && ctx.jumpHeld);

        Vector3 desiredDirection = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.moveInput);
        float desiredSpeed = ctx.EffectiveGroundMoveSpeed;

        // Skip one friction tick if we’re jumping this frame (bhop feel)
        if (!wantJump)
        {
            MovementQuake.ApplyFriction(ref ctx.velocity, ctx.groundFriction, ctx.groundStopSpeed, dt);
        }
        MovementQuake.Accelerate(ref ctx.velocity, desiredDirection, desiredSpeed, ctx.groundAccelQ, dt);
        MovementQuake.ClampHorizontalSpeed(ref ctx.velocity, ctx.EffectiveMaxHorizontalSpeed);

        if (wantJump)
        {
            ctx.velocity.y = ctx.jumpForce;
            ctx.jumpPressed = false;      // consume edge
            ctx.jumpBufferTimer = 0f;     // consume buffer
            ctx.coyoteTimer = 0f;         // no double-consume
            ctx.StateMachine.SetState(new AirborneState(ctx));
            // early return; the rest of Tick will run in the new state next frame
        }
        else
        {
            if (ctx.velocity.y < -2f) ctx.velocity.y = -2f;
            if (!ctx.characterController.isGrounded) ctx.StateMachine.SetState(new AirborneState(ctx));
        }
        //ctx.coyoteTimer = ctx.coyoteTime; not sure if this is right here?
        ctx.characterController.Move(ctx.velocity * dt);

    }


    public void FixedTick() { }
}
