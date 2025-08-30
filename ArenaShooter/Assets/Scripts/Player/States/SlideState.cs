using UnityEngine;
using System.Collections;

public class SlideState : IPlayerState
{
    PlayerContext ctx;
    float t;

    public SlideState(PlayerContext c) { ctx = c; }

    public void Enter()
    {
        ctx.slidePressed = false;
        ctx.canSlide = false;
        ctx.characterController.height = ctx.crouchHeight;
        t = 0f;
        ctx.StartCoroutine(EnableSlideSoon());
    }

    public void Exit() { ctx.characterController.height = ctx.normalHeight; }

    public void HandleInput()
    {
        if (ctx.jumpPressed)
        {
            ctx.jumpPressed = false;
            ctx.StateMachine.SetState(new AirborneState(ctx));
        }
    }

    public void Tick()
    {
        ctx.LookTick();

        var wish = MovementUtility.CamAlignedWishdir(ctx.cam, ctx.transform, ctx.moveInput) * ctx.slideSpeed;
        Vector3 flat = new Vector3(ctx.velocity.x, 0, ctx.velocity.z);
        flat = Vector3.MoveTowards(flat, wish, ctx.slideFriction * Time.deltaTime);
        ctx.velocity.x = flat.x; ctx.velocity.z = flat.z;

        if (!ctx.characterController.isGrounded) { ctx.StateMachine.SetState(new AirborneState(ctx)); return; }

        t += Time.deltaTime;
        if (t >= ctx.slideDuration || ctx.moveInput.sqrMagnitude < 0.05f)
        {
            ctx.StateMachine.SetState(new GroundedState(ctx));
            return;
        }

        ctx.characterController.Move(ctx.velocity * Time.deltaTime);
    }

    public void FixedTick() { }

    IEnumerator EnableSlideSoon()
    {
        yield return new WaitForSeconds(0.35f);
        ctx.canSlide = true;
    }
}
