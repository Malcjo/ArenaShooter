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
        if (ctx.dashPressed && ctx.canDash) { ctx.StateMachine.SetState(new DashState(ctx)); return; }
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

        Vector3 wish = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.moveInput) * ctx.moveSpeed;
        
        Vector3 flat = new Vector3(ctx.velocity.x, 0f, ctx.velocity.z);

        if (ctx.useAcceleration)
        {
            flat = Vector3.MoveTowards(flat, wish, ctx.groundAccel * Time.deltaTime);
        }
        else {
            flat = Vector3.Lerp(flat, wish, ctx.groundSnapLerp);
        }
        
        ctx.velocity.x = flat.x; 
        ctx.velocity.z = flat.z;

        //is the player grounded?
        if (!ctx.characterController.isGrounded)
        {
            ctx.StateMachine.SetState(new AirborneState(ctx));
            return;
        }

        if (ctx.velocity.y < -2f) ctx.velocity.y = -2f;

        ctx.characterController.Move(ctx.velocity * Time.deltaTime);
    }

    public void FixedTick() { }
}
