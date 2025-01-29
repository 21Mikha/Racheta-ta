using UnityEngine;

public class LandingIndicator : MonoBehaviour
{
    [SerializeField]
    private Ball ball; // Reference to the Ball object

    private SpriteRenderer spriteRenderer; // SpriteRenderer of the indicator

    private void Awake()
    {
        // Get the SpriteRenderer component attached to this object
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is missing on the LandingIndicator object!");
        }
    }

    private void Start()
    {
        if (ball != null)
        {
            // Subscribe to the OnSuccessfulShot event
            ball.OnSuccessfulShot += HandleSuccessfulShot;
        }
    }

    private void OnDestroy()
    {
        if (ball != null)
        {
            // Unsubscribe from the event to avoid memory leaks
            ball.OnSuccessfulShot -= HandleSuccessfulShot;
        }
    }

    /// <summary>
    /// Event handler for when a successful shot happens.
    /// </summary>
    /// <param name="predictedPosition">The predicted landing position of the ball.</param>
    private void HandleSuccessfulShot(Vector3 predictedPosition,Player player)
    {
        // Update the indicator's position
        transform.position = new Vector3(predictedPosition.x, 0.1f, predictedPosition.z);

        // Show the indicator
        spriteRenderer.enabled = true;
    }

    /// <summary>
    /// Hides the indicator. Call this when necessary, e.g., when the ball is no longer in motion.
    /// </summary>
    public void HideIndicator()
    {
        spriteRenderer.enabled = false;
    }
}
