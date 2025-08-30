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
        if (ctx.dashPressed && ctx.canDash) { ctx.StateMachine.SetState(new DashState(ctx)); return; }
        if (ctx.jumpPressed && ctx.wallSliding) ctx.jumpPressed = false;
    }

    public void Tick()
    {
        ctx.LookTick();

        // wall slide check and gravity
        ctx.wallSliding = ctx.CheckWallSlide(out var _);
        float g = ctx.wallSliding ? ctx.wallSlideGravity : ctx.gravity;
        ctx.velocity.y += g * Time.deltaTime;

        // horizontal with air control
        var wish = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.moveInput) * ctx.moveSpeed;
        Vector3 flat = new Vector3(ctx.velocity.x, 0, ctx.velocity.z);
        flat = Vector3.Lerp(flat, wish, ctx.airControl);
        ctx.velocity.x = flat.x; ctx.velocity.z = flat.z;

        // wall jump
        if (ctx.wallSliding && ctx.jumpPressed)
        {
            Vector3 wjLocal = new Vector3(Mathf.Sign(ctx.moveInput.x) >= 0 ? -ctx.wallJumpDir.x : ctx.wallJumpDir.x, ctx.wallJumpDir.y, 0f);
            Vector3 wj = ctx.transform.TransformDirection(wjLocal.normalized);
            ctx.velocity = new Vector3(wj.x * ctx.moveSpeed, ctx.wallJumpForce, wj.z * ctx.moveSpeed);
            ctx.jumpPressed = false;
        }

        if (ctx.characterController.isGrounded) ctx.StateMachine.SetState(new GroundedState(ctx));
        ctx.characterController.Move(ctx.velocity * Time.deltaTime);
    }

    public void FixedTick() { }
}
