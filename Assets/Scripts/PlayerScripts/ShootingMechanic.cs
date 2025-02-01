using UnityEngine;

public class ShootingMechanic : MonoBehaviour
{
    [SerializeField] private Ball ball;

    // Shot parameters

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

    public void Initialize(Ball ballReference)
    {
        ball = ballReference;
    }
    #region Shot Methods
    public bool PerformFlatShot(Vector3 direction)
    {
        return Shoot(direction, flatShotSpeed, flatShotAngle, Vector3.zero);
    }

    public bool PerformTopspinShot(Vector3 direction, float holdTime)
    {
        float powerMultiplier = Mathf.Clamp01(holdTime / maxHoldTime);
        float adjustedSpeed = topspinBaseSpeed + powerMultiplier * 5f;
        return Shoot(direction, adjustedSpeed, topspinAngle, Vector3.right * (topspinSpin * powerMultiplier));
    }

    public bool PerformSliceShot(Vector3 direction)
    {
        return Shoot(direction, sliceSpeed, sliceAngle, Vector3.right * sliceSpin);
    }
    public bool PerformLobShot(Vector3 direction, float holdTime)
    {
        float powerMultiplier = Mathf.Clamp01(holdTime / maxHoldTime);
        float adjustedSpeed = lobBaseSpeed + powerMultiplier * 5f;
        return Shoot(direction, adjustedSpeed, lobAngle + (powerMultiplier * 10f), Vector3.zero);
    }


    #endregion

    private bool Shoot(Vector3 direction, float speed, float angle, Vector3 spin)
    {
        if (ball == null)
        {
            Debug.LogError("Ball reference is missing!");
            return false;
        }

        // Calculate the adjusted direction based on the angle.
        float radians = angle * Mathf.Deg2Rad;
        Vector3 adjustedDirection = new Vector3(direction.x, Mathf.Sin(radians), direction.z).normalized;
        Vector3 appliedForce = adjustedDirection * speed;

        // Delegate the shot to the ball.
        ball.ApplyShot(appliedForce, spin,this.GetComponent<Player>());
        return true;
    }
}
