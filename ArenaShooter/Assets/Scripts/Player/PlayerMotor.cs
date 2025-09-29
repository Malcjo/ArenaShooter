using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public CharacterController cc;
    public PlayerSensors sensors;
    public PlayerStats stats;

    // Backing field + read-only property
    private Vector3 _velocity;
    public Vector3 Velocity => _velocity;




    public void SetHorizontal(Vector3 xz) { _velocity.x = xz.x; _velocity.z = xz.z; }
    public void AddVertical(float y) { _velocity.y += y; }
    public void SetVertical(float y) { _velocity.y = y; }

    public void LookTick(Camera cam, Vector2 look, bool usingGamepad, ref float yaw)
    {
        if (usingGamepad)
        {
            yaw += look.x * stats.StickSensitivity * Time.deltaTime;
            float pitch = cam.transform.localEulerAngles.x; if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch - (look.y * stats.StickSensitivity * Time.deltaTime), -85f, 85f);
            cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
        else
        {
            yaw += look.x * stats.MouseSensitivity;
            float pitch = cam.transform.localEulerAngles.x; if (pitch > 180) pitch -= 360;
            pitch = Mathf.Clamp(pitch - (look.y * stats.MouseSensitivity), -85f, 85f);
            cam.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }

    public void GroundStep(Vector3 wishDir, bool wantJump, float dt)
    {
        if (!wantJump)
        {
            MovementQuake.ApplyFriction(ref _velocity, stats.GroundFriction, stats.GroundStopSpeed, dt);
        }
        MovementQuake.Accelerate(ref _velocity, wishDir, stats.EffectiveGroundMoveSpeed, stats.GroundAccelQ, dt);
        MovementQuake.ClampHorizontalSpeed(ref _velocity, stats.EffectiveMaxHorizSpeed);

        if (wantJump) SetVertical(stats.JumpForce);
        if (_velocity.y < -2f) _velocity.y = -2f;

        cc.Move(_velocity * dt);

        //After a Move, if you hit something above, kill upward speed so you don’t bob downward on the next frame:
        if ((cc.collisionFlags & CollisionFlags.Above) != 0)
        {
            if (_velocity.y > 0f) _velocity.y = 0f;
        }
    }

    public void AirStep(Vector3 wishDir, float dt, bool useReorient = true)
    {
        AddVertical(stats.Gravity * dt);
        MovementQuake.AirAccelerate(ref _velocity, wishDir, stats.EffectiveGroundMoveSpeed, stats.AirAccelQ, stats.EffectiveAirPerFrame, dt);
        if (useReorient)
        {
            MovementQuake.ReorientVelocityPreserveSpeed(ref _velocity, wishDir, stats.AirTurnRateRad * dt);
        }
        MovementQuake.ClampHorizontalSpeed(ref _velocity, stats.EffectiveMaxHorizSpeed);
        cc.Move(_velocity * dt);

        //After a Move, if you hit something above, kill upward speed so you don’t bob downward on the next frame:
        if ((cc.collisionFlags & CollisionFlags.Above) != 0)
        {
            if (_velocity.y > 0f) _velocity.y = 0f;
        }
    }

    public void DashBurst(Vector3 dir)
    {
        _velocity.x = dir.x * stats.EffectiveDashSpeed;
        _velocity.z = dir.z * stats.EffectiveDashSpeed;
    }

    public void SlideStep(Vector3 wishDir, float slideSpeed, float slideFriction, float dt)
    {
        Vector3 flat = new Vector3(_velocity.x, 0f, _velocity.z);
        Vector3 target = wishDir * slideSpeed;
        flat = Vector3.MoveTowards(flat, target, slideFriction * dt);
        SetHorizontal(flat);
        cc.Move(_velocity * dt);
    }

    // (Optional) make sure refs are assigned
    void Awake()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        if (!sensors) sensors = GetComponent<PlayerSensors>();
        if (!stats) stats = GetComponent<PlayerStats>();
    }
}
