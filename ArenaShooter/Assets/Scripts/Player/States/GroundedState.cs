using TMPro;
using UnityEngine;

public class GroundedState : IPlayerState
{
    private readonly PlayerContext ctx;
    private bool _autoHopQueued;

    public GroundedState(PlayerContext c) { ctx = c; }

    public void Enter()
    {
        ctx.sensors.OnGroundedEnter();
        ctx.characterController.stepOffset = 0.3f;   // good default
        // queue auto-hop once on landing (fires next Tick if still held)
        _autoHopQueued = ctx.stats.AutoHop && ctx.input.Frame.JumpHeld;
    }

    public void Exit()
    {
        ctx.sensors.OnGroundedExit();
    }

    public void HandleInput()
    {
        var f = ctx.input.Frame;

        if (f.WeaponWheelHeld) { ctx.fsm.SetState(new WeaponWheelState(ctx)); return; }
        if (f.DashPressedEdge) { ctx.input.ConsumeDash(); Debug.Log("dash pressed"); ctx.fsm.SetState(new DashState(ctx));  return; }
        if (f.SlidePressedEdge && f.Move.sqrMagnitude > 0.2f) { ctx.fsm.SetState(new SlideState(ctx)); return; }
    }

    public void Tick()
    {
        float dt = Time.deltaTime;
        ctx.sensors.TickTimers(dt);

        // Look
        ctx.motor.LookTick(ctx.cam, ctx.input.Frame.Look, ctx.UsingGamepad, ref ctx.yaw);

        // jump intent (edge or one-shot auto-hop)
        bool wantJump = ctx.input.Frame.JumpPressedEdge || _autoHopQueued;
        if (wantJump)
        {
            _autoHopQueued = false;   // consume queued auto-hop
            ctx.input.ConsumeJump();  // consume any buffered/edge press
        }

        // move
        Vector3 wish = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.input.Frame.Move);
        if (ctx.sensors.LowOverhangAhead(ctx.transform))
        {
            // remove the forward part that pushes into the overhang
            Vector3 fwd = ctx.transform.forward;
            wish -= Vector3.Project(wish, fwd);
        }
        ctx.motor.GroundStep(wish, wantJump, dt);

        // state change
        if (!ctx.characterController.isGrounded) ctx.fsm.SetState(new AirborneState(ctx));
    }

    public void FixedTick() { }
}
