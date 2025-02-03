using UnityEngine;
using System;
public class BallPhysics : MonoBehaviour
{
    [Header("Physics Parameters")]
    [Tooltip("Gravity acceleration (m/s^2), acting in negative Y")]
    [SerializeField] private static float gravity = 9.81f;

    [Tooltip("Coefficient of restitution for ground bounces (0..1)")]
    [SerializeField] private float groundBounceCoeff = 0.8f;

    [Tooltip("Coefficient of restitution for net/wall collisions (0..1)")]
    [SerializeField] private float netBounceCoeff = 0.8f;

    [SerializeField] private float frictionFactor = 0.9f;

    [SerializeField] private float SurfaceLevel = 0;
    [SerializeField] private float BallRadius = 0.5f;
    public static event Action<Vector3, Vector3> OnGroundHit;
    public static event Action<Vector3, Vector3,Player> OnPlayerHitBall;
    // Current velocity of the ball
    private Vector3 velocity;

    // Is the ball currently in motion?
    private bool isLaunched = false;


    void Update()
    {
        float deltaTime = Time.deltaTime;
        ApplyGravity(deltaTime);
        UpdatePosition(deltaTime);
    }

    public Vector3 GetVelocity() { return velocity; }
    public Vector3 GetPosition() { return this.transform.position; }

    public static float GetGravity() { return gravity; }
    private void ApplyGravity(float deltaTime)
    {
        velocity.y -= gravity * deltaTime;

    }
    private void ApplyFriction()
    {
        velocity.x *= frictionFactor; 
        velocity.z *= frictionFactor;

    }

    public void ApplyShot(Vector3 appliedForce,Player player)
    {
        velocity = appliedForce;
        isLaunched = true;
        OnPlayerHitBall?.Invoke(transform.position, velocity,player);
    }

    private void UpdatePosition(float deltaTime)
    {
        Vector3 newPosition = transform.position + velocity * deltaTime;
        if (newPosition.y <= SurfaceLevel + BallRadius)
        {

            // Snap to ground
            newPosition.y = SurfaceLevel + BallRadius;

            // Flip vertical velocity & apply ground restitution
            velocity.y = -velocity.y * groundBounceCoeff;
            ApplyFriction();


            OnGroundHit?.Invoke(newPosition, velocity);
        }
        transform.position = newPosition;

    }

    /// <summary>
    /// Called when a collision occurs (non-trigger).
    /// We reflect velocity based on the collision normal if colliding with net or wall.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Net"))
        {
            Debug.Log("Collided with net");
            // Average the contact normals (if multiple contact points)
            Vector3 normal = Vector3.zero;
            foreach (var contact in collision.contacts)
            {
                normal += contact.normal;
            }
            normal.Normalize();

            // Reflect the velocity around the collision normal and apply netBounceCoeff
            velocity = Vector3.Reflect(velocity, normal) * netBounceCoeff;
        }

    }

}
