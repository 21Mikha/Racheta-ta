using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField] private float flatShotSpeed = 20f;
    [SerializeField] private float topspinBaseSpeed = 15f;
    [SerializeField] private float sliceSpeed = 12f;
    [SerializeField] private float lobBaseSpeed = 10f;
    [SerializeField] private float topspinSpin = 500f;
    [SerializeField] private float sliceSpin = -300f;
    [SerializeField] private float maxHoldTime = 2.0f;
    [SerializeField] private float flatShotAngle = 5f;
    [SerializeField] private float topspinAngle = 15f;
    [SerializeField] private float sliceAngle = 10f;
    [SerializeField] private float lobAngle = 45f;

    public bool canPerformShot;
    public static event Action<Vector3, Player> OnSuccessfulShot;
    public static event Action<string> OnCollisionZone; // Use this to send zone messages
    public static event Action OnBallHitNet;
    public static event Action OnBallOutOfBounds;
    public static event Action OnGroundHit;
    public static event Action<Player, Vector3> OnPlayerHitBall; // For AI to subscribe

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    #region Shot Methods

    public bool FlatShot(Vector3 direction, Player player)
    {
        return PerformShot(direction, flatShotSpeed, flatShotAngle, Vector3.zero, player);
    }

    public bool TopspinShot(Vector3 direction, float holdTime, Player player)
    {
        float powerMultiplier = Mathf.Clamp01(holdTime / maxHoldTime);
        float adjustedSpeed = topspinBaseSpeed + powerMultiplier * 5f;
        return PerformShot(direction, adjustedSpeed, topspinAngle, Vector3.right * (topspinSpin * powerMultiplier), player);
    }

    public bool SliceShot(Vector3 direction, Player player)
    {
        return PerformShot(direction, sliceSpeed, sliceAngle, Vector3.right * sliceSpin, player);
    }

    public bool LobShot(Vector3 direction, float holdTime, Player player)
    {
        float powerMultiplier = Mathf.Clamp01(holdTime / maxHoldTime);
        float adjustedSpeed = lobBaseSpeed + powerMultiplier * 5f;
        return PerformShot(direction, adjustedSpeed, lobAngle + (powerMultiplier * 10f), Vector3.zero, player);
    }

    private bool PerformShot(Vector3 direction, float speed, float angle, Vector3 spin, Player player)
    {
        if (!canPerformShot) return false;

        // Reset velocities
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        float radians = angle * Mathf.Deg2Rad;
        Vector3 adjustedDirection = new Vector3(direction.x, Mathf.Sin(radians), direction.z).normalized;
        Vector3 appliedForce = adjustedDirection * speed;

        rb.AddForce(appliedForce, ForceMode.Impulse);
        rb.angularVelocity = spin;

        // Predict landing position if needed
        Vector3 predictedPosition = PredictLandingPosition(appliedForce);

        // Notify that a shot was successful
        if (player != null)
        {
            OnSuccessfulShot?.Invoke(predictedPosition, player);
            // Notify the AI that a player hit the ball, including the shot velocity.
            OnPlayerHitBall?.Invoke(player, rb.velocity);
        }
        return true;
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

    #endregion

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
        if (other.CompareTag("Player"))
        {
            canPerformShot = true;
        }

        else if (other.CompareTag("ZoneA") || other.CompareTag("ZoneB"))
        {
            OnCollisionZone?.Invoke(other.tag);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPerformShot = false;
        }
        else if (other.CompareTag("OutOfBounds"))
        {
            OnBallOutOfBounds?.Invoke();
        }
    }
}
