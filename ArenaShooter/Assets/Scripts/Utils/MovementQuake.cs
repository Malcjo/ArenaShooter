using UnityEngine;

public static class MovementQuake
{
    static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

    // Ground friction similar to Quake: reduces horizontal speed toward zero
    public static void ApplyFriction(ref Vector3 velocity, float friction/* Higher stickier */, float stopSpeed, float deltaTime)
    {
        Vector3 horizontalVelocity = Flat(velocity);
        float speed = horizontalVelocity.magnitude;
        if (speed < 1e-4f) return;

        float speedControl = speed < stopSpeed ? stopSpeed : speed; // if player is moving slow speeds then stop, otherwise keep speed
        float speedToReduce = speedControl * friction * deltaTime;

        float newSpeed = Mathf.Max(0f, speed - speedToReduce);
        if (newSpeed != speed)
        {
            float scale = newSpeed / speed;
            velocity.x *= scale; velocity.z *= scale; // Y movement untouched
        }
    }

    // Ground accelerate toward a direction/speed
    public static void Accelerate(ref Vector3 velocity, Vector3 desiredDirection, float desiredSpeed, float accel, float deltaTime)
    {
        Vector3 horizontalVelocity = Flat(velocity);
        float currentSpeed = Vector3.Dot(horizontalVelocity, desiredDirection);     // component along wishdir
        float addSpeed = desiredSpeed - currentSpeed;
        if (addSpeed <= 0f) return; //at top speed don't add any more

        float accelSpeed = accel * deltaTime * desiredSpeed;
        if (accelSpeed > addSpeed) accelSpeed = addSpeed;

        velocity.x += desiredDirection.x * accelSpeed;
        velocity.z += desiredDirection.z * accelSpeed;
    }

    // Air accelerate (strafe-jump)
    public static void AirAccelerate(ref Vector3 vel, Vector3 wishdir, float wishspeed, float accel, float airMaxSpeed, float dt)
    {
        //if (wishspeed > airMaxSpeed) wishspeed = airMaxSpeed;
        Accelerate(ref vel, wishdir, wishspeed, accel, dt);
    }

    // Optional "air control" (extra turning when moving generally forward)
    public static void AirControl(ref Vector3 velocity, Vector3 desiredDirection, float wishspeed, float airControl, float deltaTime)
    {
        

        if (airControl <= 0f || wishspeed <= 0f) return;

        float originalY = velocity.y; 
        velocity.y = 0f;

        float horizontalSpeed = velocity.magnitude;
        if (horizontalSpeed < 0.01f) { velocity.y = originalY; return; }

        float alignment = Vector3.Dot(velocity.normalized, desiredDirection);
        // Gain scales with (dot^2), classic rough approximation
        float turnStrength = 30f * airControl * alignment * alignment * deltaTime;
        
        velocity += desiredDirection * turnStrength * horizontalSpeed;
        velocity = velocity.normalized * horizontalSpeed;
        velocity.y = originalY;
    }

    public static void ReorientVelocityPreserveSpeed(ref Vector3 velocity, Vector3 desiredDirection, float maxRadiansToRotate)
    {
        if (maxRadiansToRotate <= 0f) return;

        // Work in XZ
        float originalY = velocity.y;
        Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
        float horizontalSpeed = flatVelocity.magnitude;
        if (horizontalSpeed < 1e-4f) return;

        // Normalize desired direction (so input magnitude doesn't skew rotation)
        Vector3 dir = desiredDirection.sqrMagnitude > 0f ? desiredDirection.normalized : Vector3.zero;
        if (dir.sqrMagnitude < 1e-6f) return;

        // Target vector has same magnitude, new direction
        Vector3 targetFlat = dir * horizontalSpeed;

        // Rotate toward target by a limited angle (no magnitude change)
        flatVelocity = Vector3.RotateTowards(flatVelocity, targetFlat, maxRadiansToRotate, 0f);

        velocity.x = flatVelocity.x;
        velocity.z = flatVelocity.z;
        velocity.y = originalY;
    }

    public static void ClampHorizontalSpeed(ref Vector3 velocity, float maxHorizontalSpeed)
    {
        if (maxHorizontalSpeed <= 0f) return;
        Vector3 horizontalVelocity = Flat(velocity);

        float horizontalSpeed = horizontalVelocity.magnitude;


        if (horizontalSpeed > maxHorizontalSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxHorizontalSpeed;
            velocity.x = horizontalVelocity.x; 
            velocity.z = horizontalVelocity.z;
        }
    }
}
