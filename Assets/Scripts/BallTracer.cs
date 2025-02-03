using UnityEngine;

public class BallTracer : MonoBehaviour
{
    [Header("Line Renderer Settings")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int maxPositions = 20; // Trail length
    [SerializeField] private float updateInterval = 0.05f; // Smoother trail

    [Header("Trail Behavior")]
    [SerializeField] private float minSpeedToShowTrail = 5f;
    [SerializeField] private float heightOffset = 0.1f; // Prevent z-fighting

    private Vector3[] positions;
    private float timer;
    private BallPhysics ball;
    private void Awake()
    {
        ball = GetComponent<BallPhysics>();
        InitializeLineRenderer();
    }

    private void InitializeLineRenderer()
    {
        positions = new Vector3[maxPositions];
        lineRenderer.positionCount = maxPositions;
        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (ball.GetVelocity().magnitude < minSpeedToShowTrail)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        UpdateTrailPositions();
    }

    private void UpdateTrailPositions()
    {
        timer += Time.deltaTime;

        // Only update positions at intervals for smoother performance
        if (timer >= updateInterval)
        {
            ShiftPositionsArray();
            positions[0] = transform.position + Vector3.up * heightOffset;
            lineRenderer.SetPositions(positions);
            timer = 0;
        }
    }

    private void ShiftPositionsArray()
    {
        // Shift all positions forward in the array
        for (int i = positions.Length - 1; i > 0; i--)
        {
            positions[i] = positions[i - 1];
        }
    }
}