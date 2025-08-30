using UnityEngine;

public class WeaponWheelState : IPlayerState
{
    PlayerContext ctx;
    public WeaponWheelState(PlayerContext c) { ctx = c; }

    public void Enter() { /* show wheel UI later */ }
    public void Exit() { /* hide wheel UI later */ }

    public void HandleInput() { }
    public void Tick()
    {
        ctx.LookTick();
        if (!ctx.weaponWheelHeld)
        {
            if (ctx.characterController.isGrounded) ctx.StateMachine.SetState(new GroundedState(ctx));
            else ctx.StateMachine.SetState(new AirborneState(ctx));
        }
    }
    public void FixedTick() { }
}
