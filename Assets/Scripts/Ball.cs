using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rb;

    [SerializeField]
    private float flatShotSpeed = 20f;

    [SerializeField]
    private float topspinBaseSpeed = 15f;

    [SerializeField]
    private float sliceSpeed = 12f;

    [SerializeField]
    private float lobBaseSpeed = 10f;

    [SerializeField]
    private float topspinSpin = 500f; // Spin for topspin shot
    [SerializeField]
    private float sliceSpin = -300f; // Spin for slice shot

    [SerializeField]
    private float maxHoldTime = 2.0f; // Maximum hold time for holds

    [SerializeField]
    private float flatShotAngle = 5f; // Low angle for flat shots
    [SerializeField]
    private float topspinAngle = 15f; // Medium angle for topspin shots
    [SerializeField]
    private float sliceAngle = 10f; // Low angle for slice shots
    [SerializeField]
    private float lobAngle = 45f; // High angle for lob shots

    public bool canPerformShot;
    public event System.Action<Vector3,Player> OnSuccessfulShot;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Executes a flat shot.
    /// </summary>
    public bool FlatShot(Vector3 direction, Player player)
    {
        return PerformShot(direction, flatShotSpeed, flatShotAngle, Vector3.zero,player);
    }

    /// <summary>
    /// Executes a topspin shot.
    /// </summary>
    public bool TopspinShot(Vector3 direction, float holdTime, Player player)
    {
        float powerMultiplier = Mathf.Clamp01(holdTime / maxHoldTime); // Normalize hold time (0 to 1)
        float adjustedSpeed = topspinBaseSpeed + powerMultiplier * 5f; // Increase speed with hold
        return PerformShot(direction, adjustedSpeed, topspinAngle, Vector3.right * (topspinSpin * powerMultiplier), player);
    }

    /// <summary>
    /// Executes a slice shot.
    /// </summary>
    public bool SliceShot(Vector3 direction, Player player)
    {
        return PerformShot(direction, sliceSpeed, sliceAngle, Vector3.right * sliceSpin, player);
    }

    /// <summary>
    /// Executes a lob shot.
    /// </summary>
    public bool LobShot(Vector3 direction, float holdTime, Player player)
    {
        float powerMultiplier = Mathf.Clamp01(holdTime / maxHoldTime); // Normalize hold time (0 to 1)
        float adjustedSpeed = lobBaseSpeed + powerMultiplier * 5f; // Increase speed with hold
        return PerformShot(direction, adjustedSpeed, lobAngle + (powerMultiplier * 10f), Vector3.zero, player);
    }

    /// <summary>
    /// Generalized method to perform a shot with a specific angle and spin.
    /// </summary>
    private bool PerformShot(Vector3 direction, float speed, float angle, Vector3 spin,Player player)
    {
        if (!canPerformShot) return false;

        // Freeze the ball's velocity and angular velocity
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Calculate the force direction and magnitude
        float radians = angle * Mathf.Deg2Rad;
        Vector3 adjustedDirection = new Vector3(direction.x, Mathf.Sin(radians), direction.z).normalized;
        Vector3 appliedForce = adjustedDirection * speed;

        // Apply the force to the ball
        rb.AddForce(appliedForce, ForceMode.Impulse);
        rb.angularVelocity = spin;

        // Predict the landing position using the calculated force
        Vector3 predictedPosition = PredictLandingPosition(appliedForce);
        Debug.Log($"Applied Force: {appliedForce}, Predicted Landing: {predictedPosition}, Position: {transform.position}");

        // Trigger the successful shot event with the predicted position

        if (player != null)
        {
            OnSuccessfulShot?.Invoke(predictedPosition, player);

        }
        return true;
    }






    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPerformShot = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canPerformShot = false;
        }
    }

    /// <summary>
    /// Predicts the landing position of the ball.
    /// </summary>
    public Vector3 PredictLandingPosition(Vector3 force)
    {
        // Starting position and mass of the ball
        Vector3 initialPosition = transform.position;
        float mass = rb.mass;

        // Gravity in Unity
        float gravity = Mathf.Abs(Physics.gravity.y);

        // Calculate initial velocity from the applied force
        Vector3 initialVelocity = force / mass;

        // Log the initial state of the ball
        Debug.Log($"Initial Position: {initialPosition}, Initial Velocity: {initialVelocity}, Mass: {mass}, Force: {force}");

        // Vertical motion variables
        float initialHeight = initialPosition.y;
        float verticalVelocity = initialVelocity.y;

        // Log vertical motion setup
        Debug.Log($"Initial Height: {initialHeight}, Vertical Velocity: {verticalVelocity}, Gravity: {gravity}");

        // Quadratic equation to solve for time to fall
        float discriminant = verticalVelocity * verticalVelocity + 2 * gravity * initialHeight;

        // If the discriminant is negative, the ball will not hit the ground
        if (discriminant < 0)
        {
            Debug.LogWarning("Ball will not hit the ground (e.g., shot directly upward indefinitely).");
            return Vector3.zero;
        }

        // Calculate time to fall (positive root only)
        float timeToFall = (verticalVelocity + Mathf.Sqrt(discriminant)) / gravity;

        // Log time to fall
        Debug.Log($"Discriminant: {discriminant}, Time to Fall: {timeToFall}");

        // Horizontal motion: Calculate horizontal displacement during timeToFall
        Vector3 horizontalVelocity = new Vector3(initialVelocity.x, 0, initialVelocity.z);
        Vector3 horizontalDisplacement = horizontalVelocity * timeToFall;

        // Log horizontal motion calculations
        Debug.Log($"Horizontal Velocity: {horizontalVelocity}, Horizontal Displacement: {horizontalDisplacement}");

        // Final landing position
        Vector3 landingPosition = initialPosition + horizontalDisplacement;

        // Ensure the Y component of the landing position is zero (ground level)
        landingPosition.y = 0;

        // Log the final predicted landing position
        Debug.Log($"Predicted Landing Position: {landingPosition}");

        // Return the final landing position
        return landingPosition;
    }








}
