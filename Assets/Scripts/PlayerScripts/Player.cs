using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public abstract class Player : MonoBehaviour
{


    [Header("Movement Settings")]
    [SerializeField] protected float movementSpeed = 15f;
    protected float currentSpeed;
    protected Vector2 currentDirection;
    protected bool isMoving = false;
    protected bool isSprinting = false;

    [Header("Stamina Settings")]
    [SerializeField, Range(0, 100)] private float maxStamina = 100f;
    [SerializeField] protected float staminaRegenRate = 10f;
    [SerializeField] protected float staminaDepletionRate = 20f;
    protected Stamina stamina;
    public Stamina Stamina => stamina;

    [Header("Ball Settings")]
    [SerializeField] protected Ball ball;

    protected Rigidbody rb;


    [Header("Launching Settings")]
    protected ShootingMechanic shootingMechanic;
    protected float launchAngle = 0f;
    protected Vector3 launchDirection = Vector3.forward;
    protected bool canPerformShot;
    [Header("Bounds Settings")]
    protected float minX = -50, maxX = 50, minZ = -100, maxZ = 0;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        shootingMechanic = GetComponent<ShootingMechanic>();
        if (shootingMechanic != null)
        {
            shootingMechanic.Initialize(ball);
        }
        currentSpeed = movementSpeed;
        stamina = new Stamina(maxStamina, staminaRegenRate, staminaDepletionRate);

    }

    public void SetBounds(float minX, float maxX , float minZ , float maxZ)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minZ = minZ;
        this.maxZ = maxZ;
    }


    protected void HandleLaunchDirectionChanged(float deltaX)
    {
        launchAngle = Mathf.Clamp(launchAngle + deltaX, -90f, 90f);
        launchDirection = Quaternion.Euler(0f, launchAngle, 0f) * Vector3.forward;
    }

    public float GetLaunchAngle() => launchAngle;
    public Vector3 GetLaunchDirection() => launchDirection;

    protected virtual void FixedUpdate()
    {
        if (stamina == null)
        {
            Debug.LogError($"Stamina is NULL in {gameObject.name} - Player.Awake() might not have run!");
            return;
        }
        stamina.Update(isSprinting);

        if (isMoving)
        {
            Vector3 movement = new Vector3(currentDirection.x, 0f, currentDirection.y) * currentSpeed;
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
        else
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }

        currentSpeed = isSprinting ? movementSpeed * 2 : movementSpeed;

        if (stamina.CurrentStamina <= 0)
        {
            isSprinting = false;
        }

        Vector3 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
        rb.position = clampedPosition;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            canPerformShot = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            canPerformShot = false;
        }
    }

    }