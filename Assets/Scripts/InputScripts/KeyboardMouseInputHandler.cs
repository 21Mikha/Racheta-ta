using UnityEngine;

public class KeyboardMouseInputHandler : InputHandler
{
    private bool isLeftActive = false;  // Flag to check if left button is active
    private bool isRightActive = false; // Flag to check if right button is active

    private bool isClickLocked = false; // Lock state for right button

    private float cooldownTimer = 0f; 


    private Vector2 previousMousePosition;

    [SerializeField]
    private float mouseSensitivity = 0.2f; // Sensitivity factor for deltaX adjustments

    private void Update()
    {
        // Update cooldown timer once.
        if (isClickLocked)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isClickLocked = false;
            }
        }

        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        OnMovementInput?.Invoke(movement);

        // Handle Left Shift for Sprint
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        OnSprintInput?.Invoke(isSprinting);

        // Handle Left and Right Mouse Buttons if not locked.
        if (!isClickLocked)
        {
            HandleLeftMouseButton();
            HandleRightMouseButton();
        }

        // Track mouse delta for launch direction.
        TrackMouseDeltaX();
    }


    private void HandleLeftMouseButton()
    {
        if (Input.GetMouseButtonDown(0) && !isRightActive)
        {
            isLeftActive = true;
            holdTime = 0f; // Reset timer
        }

        if (Input.GetMouseButtonUp(0) && isLeftActive)
        {
            if (holdTime < 0.2f)
            {
                OnLeftClick?.Invoke(); // Should fire only once on a quick click.
            }
            else if (holdTime > 0.2f)
            {
                OnLeftClick?.Invoke();
               // OnLeftHold?.Invoke(holdTime);
            }
            isLeftActive = false;
            holdTime = 0f;
            ActivateCooldown();
        }


        if (Input.GetMouseButtonUp(0) && isLeftActive)
        {
            if (holdTime < 0.2f)
            {
                OnLeftClick?.Invoke(); // Handle quick click
            }
            else if (holdTime > 0.2f)
            {
                OnLeftHold?.Invoke(holdTime); // Invoke with elapsed hold time
                holdTime = 0f;               // Reset the timer
            }
            isLeftActive = false; // Reset flag after release
            holdTime = 0f;    // Reset timer after release

            ActivateCooldown();
        }
    }

    private void HandleRightMouseButton()
    {
        if (Input.GetMouseButtonDown(1) && !isLeftActive)
        {
            isRightActive = true;
            holdTime = 0f; // Reset timer
        }

        if (Input.GetMouseButton(1) && isRightActive)
        {
            holdTime += Time.deltaTime;

            // Check if the hold time is greater than or equal to the max hold time
            if (holdTime >= maxHoldTime)
            {
                OnRightHold?.Invoke(maxHoldTime); // Invoke with max hold time
                holdTime = 0f;              // Reset the timer

                ActivateCooldown();
            }

        }

        if (Input.GetMouseButtonUp(1) && isRightActive)
        {
            if (holdTime < 0.2f)
            {
                OnRightClick?.Invoke(); // Handle quick click
            }
            else if (holdTime > 0.2f)
            {
                OnRightHold?.Invoke(holdTime); // Invoke with elapsed hold time
                holdTime = 0f;               // Reset the timer
            }
            isRightActive = false; // Reset flag after release
            holdTime = 0f;    // Reset timer after release

            ActivateCooldown();
        }
    }

    private void TrackMouseDeltaX()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        float deltaX = (currentMousePosition.x - previousMousePosition.x) * mouseSensitivity;

        // Invoke the event with the scaled deltaX value
        if (Mathf.Abs(deltaX) > Mathf.Epsilon) // Only trigger if there's a noticeable change
        {
            OnLaunchDirectionChanged?.Invoke(deltaX);
        }

        previousMousePosition = currentMousePosition;
    }

    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10.0f); // Clamp sensitivity to a reasonable range
    }

    public override void SetMaxHoldTime(float maxTime)
    {
        maxHoldTime = Mathf.Max(0.1f, maxTime); // Ensure a minimum max hold time
    }

    public override float GetMaxHoldTime()
    {
        return maxHoldTime;
    }

    public override float GetHoldTime()
    {
        return holdTime;
    }
    public override void ActivateCooldown()
    {
        isClickLocked = true;
        cooldownTimer = cooldownPeriod;
    }
}
