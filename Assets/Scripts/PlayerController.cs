using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private float movementSpeed = 15f; // Base movement speed
    [SerializeField] float currentSpeed; // The actual movement speed


    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaDepletionRate = 20f;
    private Stamina stamina; // Stamina system
    public Stamina Stamina => stamina;

    private Rigidbody rb;
    private Vector2 currentDirection;
    private bool isMoving = false;
    private bool isSprinting = false;

    // Rectangle bounds
    private float minX=-50, maxX = 50, minZ=-100, maxZ = 0;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = movementSpeed; // Set initial speed to base movement speed
        stamina = new Stamina(maxStamina, staminaRegenRate, staminaDepletionRate);
        if (inputHandler == null)
        {
            inputHandler = GetComponent<InputHandler>();
        }
    }

    private void OnEnable()
    {
        inputHandler.OnMovementInput += HandleMovement;
        inputHandler.OnSprintInput += HandleSprint;
        inputHandler.OnLeftClick += HandleLeftClick;
        inputHandler.OnRightClick += HandleRightClick;
        inputHandler.OnLeftHold += HandleLeftHold;
        inputHandler.OnRightHold += HandleRightHold;
    }

    private void OnDisable()
    {
        inputHandler.OnMovementInput -= HandleMovement;
        inputHandler.OnSprintInput -= HandleSprint;
        inputHandler.OnLeftClick -= HandleLeftClick;
        inputHandler.OnRightClick -= HandleRightClick;
        inputHandler.OnLeftHold -= HandleLeftHold;
        inputHandler.OnRightHold -= HandleRightHold;
    }

    // Method to define the rectangle bounds
    public void SetBounds(Vector3 bottomLeft, Vector3 topRight)
    {
        minX = bottomLeft.x;
        maxX = topRight.x;
        minZ = bottomLeft.z;
        maxZ = topRight.z;
    }
    private void HandleMovement(Vector2 direction)
    {
        currentDirection = direction; // Update the current direction
        isMoving = direction.sqrMagnitude > 0.01f; // Check if input is active
    }
    private void HandleSprint(bool sprintInput)
    {
        isSprinting = sprintInput && stamina.CanPerformAction(1f);
    }

    private void HandleLeftClick()
    {
        Debug.Log("Left click detected.");
    }

    private void HandleRightClick()
    {
        Debug.Log("Right click detected.");
    }

    private void HandleLeftHold(float duration)
    {
        Debug.Log($"Left hold duration: {duration}");
    }

    private void HandleRightHold(float duration)
    {
        Debug.Log($"Right hold duration: {duration}");
    }

    private void FixedUpdate()
    {
        // Update stamina system
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

        // Adjust speed based on sprinting
        currentSpeed = isSprinting ? movementSpeed * 2 : movementSpeed;

        // Stop sprinting if stamina is depleted
        if (stamina.CurrentStamina <= 0)
        {
            isSprinting = false;
        }


        // Clamp the player's position within the rectangle bounds
        Vector3 clampedPosition = rb.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, minZ, maxZ);
        rb.position = clampedPosition;



    }

}
