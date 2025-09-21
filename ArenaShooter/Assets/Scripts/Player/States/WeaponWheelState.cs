using UnityEngine;

public class WeaponWheelState : IPlayerState
{
    private readonly PlayerContext ctx;
    public WeaponWheelState(PlayerContext c) { ctx = c; }

    public void Enter()
    {
        // optional: slow time or lock look here if you want
        // Time.timeScale = 0.1f;
    }

    public void Exit()
    {
        // Time.timeScale = 1f;
    }

    public void HandleInput()
    {
        // close on release
        if (!ctx.input.Frame.WeaponWheelHeld)
        {
            if (ctx.characterController.isGrounded) ctx.fsm.SetState(new GroundedState(ctx));
            else ctx.fsm.SetState(new AirborneState(ctx));
        }
    }

    public void Tick()
    {
        // keep looking while wheel is open (or disable if you prefer)
        ctx.motor.LookTick(ctx.cam, ctx.input.Frame.Look, ctx.UsingGamepad, ref ctx.yaw);
        // you can draw/select wheel UI here
    }

    public void FixedTick() { }
}
