using UnityEngine;

public class GroundedState : IPlayerState
{
    PlayerContext ctx;
    public GroundedState(PlayerContext c) { ctx = c; }

    public void Enter() { if (ctx.velocity.y < 0) ctx.velocity.y = -2f; }
    public void Exit() { }

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

        bool wantJump = ctx.jumpPressed || (ctx.autoHop && ctx.jumpHeld);

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
            ctx.jumpPressed = false;
            ctx.StateMachine.SetState(new AirborneState(ctx));
        }
        else
        {
            if (ctx.velocity.y < -2f) ctx.velocity.y = -2f;
            if (!ctx.characterController.isGrounded) ctx.StateMachine.SetState(new AirborneState(ctx));
        }

        ctx.characterController.Move(ctx.velocity * dt);
    }


    public void FixedTick() { }
}
