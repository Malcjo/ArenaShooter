using UnityEngine;
using System.Collections;

public class DashState : IPlayerState
{
    PlayerContext ctx;
    float t;
    Vector3 dir;

    public DashState(PlayerContext c) { ctx = c; }

    public void Enter()
    {
        ctx.dashPressed = false;
        ctx.isDashing = true;



        Vector3 flat = new Vector3(ctx.velocity.x, 0, ctx.velocity.z);
        dir = flat.sqrMagnitude < 0.1f ? ctx.transform.forward : flat.normalized;
        t = 0f;

        ctx.ConsumeDashCharge();
    }

    public void Exit() 
    {
        ctx.isDashing = false;
    }
    public void HandleInput() { }

    public void Tick()
    {
        ctx.LookTick();

        t += Time.deltaTime;
        ctx.velocity.x = dir.x * ctx.EffectiveDashSpeed;
        ctx.velocity.z = dir.z * ctx.EffectiveDashSpeed;
        ctx.velocity.y += ctx.gravity * Time.deltaTime;

        if (t >= ctx.dashDuration)
        {
            if (ctx.characterController.isGrounded) ctx.StateMachine.SetState(new GroundedState(ctx));
            else ctx.StateMachine.SetState(new AirborneState(ctx));
        }

        ctx.characterController.Move(ctx.velocity * Time.deltaTime);
    }

    public void FixedTick() { }

}
