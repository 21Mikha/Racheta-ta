using UnityEngine;

public class TestTrajectory : MonoBehaviour
{
    public float restitution = 0.8f;
    public float gravity = 9.81f;

    public Transform LandingTracer;
    public Transform BounceTracer;

    private void Start()
    {
        BallPhysics.OnGroundHit += HandleBallBounce;
        BallPhysics.OnPlayerHitBall += HandlePlayerHitBall;
    }

    public void HandlePlayerHitBall(Vector3 initialPosition, Vector3 initialVelocity,Player player)
    {
        // 1) Where does it first land?
        Vector3 firstLanding = BallLandingPredictor.PredictLandingPosition(initialPosition, initialVelocity, gravity);
        LandingTracer.position = firstLanding;
        Debug.Log("First landing = " + firstLanding);
    }
    public void HandleBallBounce(Vector3 pos, Vector3 vel)
    {
        // We already know the ball is at ground level, velocity has been flipped
        // So we just find the NEXT ground contact from that new state.
        Vector3 nextLanding = BallLandingPredictor.PredictNextGroundContact(pos, vel, gravity);
        BounceTracer.position = nextLanding;
    }



}
