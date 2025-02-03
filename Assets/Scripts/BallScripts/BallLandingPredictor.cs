using UnityEngine;

public static class BallLandingPredictor
{
    /// <summary>
    /// Predicts the FIRST landing position (where y=0) given an initial position/velocity,
    /// ignoring air drag (only gravity). If it never lands, returns Vector3.positiveInfinity.
    /// </summary>
    /// <param name="initialPosition">Starting world position of the ball.</param>
    /// <param name="initialVelocity">Starting velocity (m/s).</param>
    /// <param name="gravity">Gravity acceleration (positive), acting downward on y.</param>
    /// <returns>The (x,0,z) landing position, or Vector3.positiveInfinity if no ground hit.</returns>
    public static Vector3 PredictLandingPosition(Vector3 initialPosition, Vector3 initialVelocity, float gravity = 9.81f)
    {
        float tGround = TimeToGround(initialPosition, initialVelocity, gravity);
        if (tGround < 0f)
        {
            // No real solution => never hits y=0
            return Vector3.positiveInfinity;
        }

        // Evaluate position at time tGround
        return PositionAtTime(initialPosition, initialVelocity, tGround, gravity);
    }

    /// <summary>
    /// Predicts the SECOND landing position (y=0) after exactly ONE bounce on the ground.
    /// Steps:
    ///   1) Ball travels from initial pos/vel to first ground contact.
    ///   2) Bounce: flip vertical velocity * restitution.
    ///   3) Ball travels until it hits ground again.
    /// Returns that second ground-contact position or Vector3.positiveInfinity if none.
    /// </summary>
    /// <param name="initialPosition">Starting world position of the ball.</param>
    /// <param name="initialVelocity">Starting velocity (m/s).</param>
    /// <param name="restitution">Coefficient of restitution (0..1) for the bounce.</param>
    /// <param name="gravity">Gravity acceleration (positive), acting downward on y.</param>
    /// <returns>(x,0,z) position of the second ground contact, or Vector3.positiveInfinity if no second hit.</returns>
    public static Vector3 PredictBouncePositionAfterContactWithGround(
        Vector3 initialPosition,
        Vector3 initialVelocity,
        float restitution,
        float gravity = 9.81f)
    {
        // --- 1) Find time to first ground contact ---
        float tFirst = TimeToGround(initialPosition, initialVelocity, gravity);
        if (tFirst < 0f)
        {
            // No ground collision at all
            return Vector3.positiveInfinity;
        }

        // Position and velocity at first contact
        Vector3 posFirst = PositionAtTime(initialPosition, initialVelocity, tFirst, gravity);
        Vector3 velFirst = VelocityAtTime(initialVelocity, tFirst, gravity);

        // --- 2) Apply bounce (flip vertical velocity) ---
        Vector3 velAfterBounce = velFirst;
        velAfterBounce.y = -restitution * velFirst.y;

        // --- 3) From that bounce, find next time to ground ---
        float tSecond = TimeToGround(posFirst, velAfterBounce, gravity);
        if (tSecond < 0f)
        {
            // No second collision
            return Vector3.positiveInfinity;
        }

        // Evaluate position at the second ground contact
        Vector3 posSecond = PositionAtTime(posFirst, velAfterBounce, tSecond, gravity);
        return posSecond;
    }

    public static Vector3 PredictNextGroundContact(
    Vector3 currentPos,
    Vector3 currentVel,
    float gravity = 9.81f)
    {
        // Just time to ground from now, ignoring any bounces before
        float tGround = TimeToGround(currentPos, currentVel, gravity);
        if (tGround < 0f)
            return Vector3.positiveInfinity;

        // Evaluate position at that time
        return PositionAtTime(currentPos, currentVel, tGround, gravity);
    }



    // ------------------------------------------------------------------
    // Helper Methods 
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the time (in seconds) until the ball (pos, vel) reaches y=0.
    /// If there's no real positive solution, returns -1.
    /// </summary>
    private static float TimeToGround(Vector3 pos, Vector3 vel, float gravity)
    {
        // y(t) = y0 + vy0*t - 0.5*g*t^2 = 0
        // Quadratic: -0.5*g * t^2 + vy0*t + y0 = 0
        float y0 = pos.y;
        float vy0 = vel.y;
        float halfG = 0.5f * gravity;

        // a = -halfG, b = vy0, c = y0
        float a = -halfG;
        float b = vy0;
        float c = y0;

        float discriminant = b * b - 4f * a * c;
        if (discriminant < 0f)
        {
            // No real solutions => never hits ground
            return -1f;
        }

        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-b + sqrtDisc) / (2f * a);
        float t2 = (-b - sqrtDisc) / (2f * a);

        // We want the earliest positive time
        float t = -1f;
        if (t1 > 0f && t2 > 0f) t = Mathf.Min(t1, t2);
        else if (t1 > 0f) t = t1;
        else if (t2 > 0f) t = t2;

        return t;
    }

    /// <summary>
    /// Returns the position after time t, given initial (pos, vel), ignoring air drag.
    /// x(t) = x0 + vx0*t
    /// y(t) = y0 + vy0*t - 0.5*g*t^2
    /// z(t) = z0 + vz0*t
    /// </summary>
    private static Vector3 PositionAtTime(Vector3 startPos, Vector3 startVel, float t, float gravity)
    {
        float x = startPos.x + startVel.x * t;
        float y = startPos.y + startVel.y * t - 0.5f * gravity * t * t;
        float z = startPos.z + startVel.z * t;
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Returns the velocity after time t, given an initial velocity, ignoring air drag.
    /// vx(t) = vx0
    /// vy(t) = vy0 - g*t
    /// vz(t) = vz0
    /// </summary>
    private static Vector3 VelocityAtTime(Vector3 startVel, float t, float gravity)
    {
        float vx = startVel.x;
        float vy = startVel.y - gravity * t;
        float vz = startVel.z;
        return new Vector3(vx, vy, vz);
    }
}
