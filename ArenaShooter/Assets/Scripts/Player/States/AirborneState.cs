using UnityEngine;

public class AirborneState : IPlayerState
{
    private readonly PlayerContext ctx;
    public AirborneState(PlayerContext c) { ctx = c; }

    public void Enter() {
        ctx.characterController.stepOffset = 0f;
    }
    public void Exit() { }

    public void HandleInput()
    {
        var f = ctx.input.Frame;

        
        if (f.DashPressedEdge) { ctx.input.ConsumeDash(); ctx.fsm.SetState(new DashState(ctx)); return; }
    }

    public void Tick()
    {
        float dt = Time.deltaTime;
        ctx.sensors.TickTimers(dt);

        // Look
        ctx.motor.LookTick(ctx.cam, ctx.input.Frame.Look, ctx.UsingGamepad, ref ctx.yaw);

        // ledge snap forgiveness (does a small upward CC.Move if valid)
        ctx.sensors.TryLedgeSnap(ctx.transform.position, ctx.motor.Velocity, ctx.transform);

        // air move
        Vector3 wish = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.input.Frame.Move);
        ctx.motor.AirStep(wish, dt, useReorient: true);

        // coyote jump
        if (ctx.sensors.CoyoteTimer > 0f && ctx.input.Frame.JumpPressedEdge)
        {
            ctx.motor.SetVertical(ctx.stats.JumpForce);
            ctx.input.ConsumeJump();
        }

        // land
        if (ctx.characterController.isGrounded) ctx.fsm.SetState(new GroundedState(ctx));
    }

    public void FixedTick() { }
}
