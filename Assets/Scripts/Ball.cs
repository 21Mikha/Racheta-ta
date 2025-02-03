using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rb;


    public static event Action<Vector3, Player> OnSuccessfulShot;
    public static event Action<string> OnCollisionZone; // Use this to send zone messages
    public static event Action OnBallHitNet;
    public static event Action OnBallOutOfBounds;
    public static event Action OnGroundHit;
    public static event Action<Player, Vector3> OnPlayerHitBall; // For AI to subscribe


 
    Vector3 predictedPosition;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyShot(Vector3 appliedForce, Vector3 spin, Player player)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(appliedForce, ForceMode.Impulse);
        rb.angularVelocity = spin;
        // Predict landing position if needed
        predictedPosition = PredictLandingPosition(appliedForce);

        // Notify that a shot was successful
        if (player != null)
        {
            OnSuccessfulShot?.Invoke(predictedPosition, player);
            // Notify the AI that a player hit the ball, including the shot velocity.
            OnPlayerHitBall?.Invoke(player, rb.velocity);
        }
    }

    public Vector3 PredictLandingPosition(Vector3 force)
    {
        Vector3 initialPosition = transform.position;
        float mass = rb.mass;
        float gravity = Mathf.Abs(Physics.gravity.y);
        Vector3 initialVelocity = force / mass;
        float initialHeight = initialPosition.y;
        float verticalVelocity = initialVelocity.y;
        float discriminant = verticalVelocity * verticalVelocity + 2 * gravity * initialHeight;
        if (discriminant < 0)
        {
            Debug.LogWarning("Ball will not hit the ground.");
            return Vector3.zero;
        }
        float timeToFall = (verticalVelocity + Mathf.Sqrt(discriminant)) / gravity;
        Vector3 horizontalVelocity = new Vector3(initialVelocity.x, 0, initialVelocity.z);
        Vector3 horizontalDisplacement = horizontalVelocity * timeToFall;
        Vector3 landingPosition = initialPosition + horizontalDisplacement;
        landingPosition.y = 0;
        return landingPosition;
    }

    public Vector3 GetPredictedLandingPosition()
    {
        return predictedPosition;
    }

    #region Collision Handling with OnCollisionEnter

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("GroundArea"))
        {
            OnGroundHit?.Invoke();

        }
        else if (collision.collider.CompareTag("Net"))
        {
            OnBallHitNet?.Invoke();
        }

    }

    private void OnCollisionExit(Collision collision)
    {

    }

    #endregion



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ZoneA") || other.CompareTag("ZoneB"))
        {
            OnCollisionZone?.Invoke(other.tag);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("OutOfBounds"))
        {
            OnBallOutOfBounds?.Invoke();
        }
    }

}