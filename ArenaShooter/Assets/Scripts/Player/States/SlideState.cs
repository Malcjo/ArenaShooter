using UnityEngine;

public class SlideState : IPlayerState
{
    private readonly PlayerContext ctx;
    private float _timer;

    public SlideState(PlayerContext c) { ctx = c; }

    public void Enter()
    {
        _timer = 0f;

        // shrink controller to crouch height
        ctx.motor.SetVertical(Mathf.Min(0f, ctx.motor.Velocity.y));
        ctx.characterController.height = ctx.stats.CrouchHeight;

        ctx.characterController.stepOffset = 0f;

        var center = ctx.characterController.center;
        center.y = ctx.stats.CrouchHeight * 0.5f;
        ctx.characterController.center = center;
    }

    public void Exit()
    {
        ctx.characterController.height = ctx.normalHeight;
        var center = ctx.characterController.center;
        center.y = ctx.normalHeight * 0.5f;
        ctx.characterController.center = center;
        if (ctx.characterController.isGrounded) ctx.characterController.stepOffset = 0.3f;
    }

    public void HandleInput()
    {
        if (ctx.input.Frame.DashPressedEdge) { ctx.input.ConsumeDash(); ctx.fsm.SetState(new DashState(ctx)); return; }
    }

    public void Tick()
    {
        float dt = Time.deltaTime;
        _timer += dt;

        Vector3 wish = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.input.Frame.Move);
        ctx.motor.SlideStep(wish, ctx.stats.EffectiveSlideSpeed, ctx.stats.SlideFrictionEffective, dt);
        ctx.motor.AddVertical(ctx.stats.Gravity * dt);

        bool timeUp = _timer >= ctx.stats.SlideDuration;
        bool leftGround = !ctx.characterController.isGrounded;

        if (timeUp || leftGround)
        {
            if (ctx.characterController.isGrounded) ctx.fsm.SetState(new GroundedState(ctx));
            else ctx.fsm.SetState(new AirborneState(ctx));
        }
    }

    public void FixedTick() { }
}
