using UnityEngine;

public class PlayerSensors : MonoBehaviour
{
    public CharacterController cc;
    public LayerMask wallMask, groundMask = ~0;
    public float wallCheckDistance = 0.6f, slopeLimitPadding = 0.1f;

    [Header("Forgiveness")]
    public float coyoteTime = 0.10f;
    public float ledgeSnapUpDistance = 0.35f;
    public float ledgeSnapProbeForward = 0.30f;
    public float ledgeSnapProbeRadius = 0.20f;
    public bool enableLedgeSnap = true;
    public float ledgeSnapActiveWindow = 0.25f;
    public float ledgeSnapMinForwardSpeed = 1.0f;
    [Range(0, 1)] public float ledgeSnapMinForwardDot = 0.35f;

    public float CoyoteTimer { get; private set; }
    public float LedgeSnapTimer { get; private set; }
    public bool LedgeSnapConsumed { get; private set; }

    void Awake() { if (!cc) cc = GetComponent<CharacterController>(); }

    public void OnGroundedEnter()
    {
        CoyoteTimer = coyoteTime;
        LedgeSnapConsumed = false;
    }
    public void OnGroundedExit()
    {
        LedgeSnapTimer = ledgeSnapActiveWindow;
    }

    public void TickTimers(float dt)
    {
        if (CoyoteTimer > 0f) CoyoteTimer -= dt;
        if (LedgeSnapTimer > 0f) LedgeSnapTimer -= dt;
    }

    public bool CheckWallSlide(Vector3 origin, Transform t, out RaycastHit hit)
    {
        hit = default;
        if (cc.isGrounded) return false;
        if (Physics.Raycast(origin, t.right, out hit, wallCheckDistance, wallMask)) return true;
        if (Physics.Raycast(origin, -t.right, out hit, wallCheckDistance, wallMask)) return true;
        return false;
    }

    public bool TryLedgeSnap(Vector3 position, Vector3 velocity, Transform t)
    {
        if (!enableLedgeSnap || cc.isGrounded || LedgeSnapConsumed) return false;
        if (velocity.y > 0f || LedgeSnapTimer <= 0f) return false;

        Vector3 flatVel = new Vector3(velocity.x, 0, velocity.z);
        float sp = flatVel.magnitude;
        if (sp < ledgeSnapMinForwardSpeed) return false;
        Vector3 fwd = t.forward;
        if (Vector3.Dot(flatVel.normalized, fwd) < ledgeSnapMinForwardDot) return false;

        float footY = position.y + cc.center.y - (cc.height * 0.5f);
        Vector3 frontOrigin = new Vector3(position.x, footY + 0.1f, position.z);
        float frontDist = ledgeSnapProbeForward + ledgeSnapProbeRadius + 0.05f;
        if (!Physics.SphereCast(frontOrigin, ledgeSnapProbeRadius, fwd, out _, frontDist, groundMask, QueryTriggerInteraction.Ignore))
            return false;

        Vector3 downOrigin = new Vector3(position.x, footY + ledgeSnapUpDistance, position.z) + fwd * ledgeSnapProbeForward;
        float castDist = ledgeSnapUpDistance + 0.05f;

        if (Physics.SphereCast(downOrigin, ledgeSnapProbeRadius, Vector3.down, out RaycastHit topHit, castDist, groundMask, QueryTriggerInteraction.Ignore))
        {
            if (Vector3.Angle(topHit.normal, Vector3.up) <= cc.slopeLimit + slopeLimitPadding)
            {
                float targetFootY = topHit.point.y + cc.skinWidth + 0.001f;
                float deltaY = targetFootY - footY;
                if (deltaY > 0f && deltaY <= ledgeSnapUpDistance)
                {
                    cc.Move(new Vector3(0f, deltaY, 0f));
                    LedgeSnapConsumed = true;
                    return true;
                }
            }
        }
        return false;
    }
}
