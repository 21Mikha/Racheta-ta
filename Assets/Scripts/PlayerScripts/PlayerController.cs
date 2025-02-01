using UnityEngine;

public class PlayerController : Player
{
    [Header("Input Settings")]
    [SerializeField] private InputHandler inputHandler;

    protected override void Awake()
    {
        base.Awake();
        if (inputHandler == null)
        {
            inputHandler = GetComponent<InputHandler>();
        }
        SetBounds(-50,50,-100,0);
    }

    private void OnEnable()
    {
        inputHandler.OnMovementInput += HandleMovement;
        inputHandler.OnSprintInput += HandleSprint;
        inputHandler.OnLeftClick += HandleLeftClick;
        inputHandler.OnRightClick += HandleRightClick;
        inputHandler.OnLeftHold += HandleLeftHold;
        inputHandler.OnRightHold += HandleRightHold;
        inputHandler.OnLaunchDirectionChanged += HandleLaunchDirectionChanged;
    }

    private void OnDisable()
    {
        inputHandler.OnMovementInput -= HandleMovement;
        inputHandler.OnSprintInput -= HandleSprint;
        inputHandler.OnLeftClick -= HandleLeftClick;
        inputHandler.OnRightClick -= HandleRightClick;
        inputHandler.OnLeftHold -= HandleLeftHold;
        inputHandler.OnRightHold -= HandleRightHold;
        inputHandler.OnLaunchDirectionChanged -= HandleLaunchDirectionChanged;
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
        if (canPerformShot)
        shootingMechanic.PerformFlatShot(GetLaunchDirection());
    }

    private void HandleRightClick()
    {
        //Debug.Log("Right click detected.");
        if (canPerformShot)
            shootingMechanic.PerformSliceShot(GetLaunchDirection());

    }

    private void HandleLeftHold(float duration)
    {
        // Debug.Log($"Left hold duration: {duration}");
        if (canPerformShot)
            shootingMechanic.PerformTopspinShot(GetLaunchDirection(), duration);
    }

    private void HandleRightHold(float duration)
    {
        //Debug.Log($"Right hold duration: {duration}");
        if (canPerformShot)
            shootingMechanic.PerformLobShot(GetLaunchDirection(), duration);
    }
    private void HandleLaunchDirectionChanged(float deltaX)
    {
        // Update the launch angle based on deltaX
        // Assuming deltaX maps directly to angles between -90 and 90
        launchAngle = Mathf.Clamp(launchAngle + deltaX, -90f, 90f);

        // Convert the launch angle to a direction vector
        launchDirection = Quaternion.Euler(0f, launchAngle, 0f) * Vector3.forward;
    }

}
