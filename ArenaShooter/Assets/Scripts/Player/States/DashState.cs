using UnityEngine;

public class DashState : IPlayerState
{
    private readonly PlayerContext ctx;
    private Vector3 _dashDir;
    private float _timer;

    public DashState(PlayerContext c) { ctx = c; }

    public void Enter()
    {
        // gate by charges and current dash flag
        if (ctx.stats.IsDashing || !ctx.stats.HasDashCharge)
        {
            if (ctx.characterController.isGrounded) ctx.fsm.SetState(new GroundedState(ctx));
            else ctx.fsm.SetState(new AirborneState(ctx));
            return;
        }

        ctx.stats.IsDashing = true;
        ctx.stats.TryConsumeDashCharge();   // triggers refill when it hits 0
        _timer = 0f;

        // direction: keep current flat velocity if moving, else forward
        Vector3 flat = new Vector3(ctx.motor.Velocity.x, 0f, ctx.motor.Velocity.z);
        _dashDir = flat.sqrMagnitude > 0.01f ? flat.normalized : ctx.transform.forward;

        // initial burst
        ctx.motor.DashBurst(_dashDir);
    }

    public void Exit()
    {
        ctx.stats.IsDashing = false;
    }

    public void HandleInput()
    {
        if (ctx.input.Frame.WeaponWheelHeld) { ctx.fsm.SetState(new WeaponWheelState(ctx)); }
    }

    public void Tick()
    {
        float dt = Time.deltaTime;
        _timer += dt;

        // keep burst + gravity
        ctx.motor.DashBurst(_dashDir);
        ctx.motor.AddVertical(ctx.stats.Gravity * dt);
        ctx.characterController.Move(ctx.motor.Velocity * dt);

        if (_timer >= ctx.stats.DashDuration)
        {
            if (ctx.characterController.isGrounded) ctx.fsm.SetState(new GroundedState(ctx));
            else ctx.fsm.SetState(new AirborneState(ctx));
        }
    }

    public void FixedTick() { }
}
