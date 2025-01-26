using UnityEngine;

public class KeyboardMouseInputHandler : InputHandler
{
    private float leftHoldTime = 0f;
    private float rightHoldTime = 0f;
    private bool isLeftActive = false;  // Flag to check if left button is active
    private bool isRightActive = false; // Flag to check if right button is active


    private void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        OnMovementInput?.Invoke(movement);

        // Handle Left Shift for Sprint
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        OnSprintInput?.Invoke(isSprinting);

        // Left Mouse Button Handling
        if (Input.GetMouseButtonDown(0) && !isRightActive)
        {
            isLeftActive = true;
            leftHoldTime = 0f; // Reset timer
        }

        if (Input.GetMouseButton(0) && isLeftActive)
        {
            leftHoldTime += Time.deltaTime;
            OnLeftHold?.Invoke(leftHoldTime);
        }

        if (Input.GetMouseButtonUp(0) && isLeftActive)
        {
            if (leftHoldTime < 0.2f)
            {
                OnLeftClick?.Invoke();
            }
            isLeftActive = false; // Reset flag after release
            leftHoldTime = 0f; // Reset after release
        }

        // Right Mouse Button Handling
        if (Input.GetMouseButtonDown(1) && !isLeftActive)
        {
            isRightActive = true;
            rightHoldTime = 0f; // Reset timer
        }

        if (Input.GetMouseButton(1) && isRightActive)
        {
            rightHoldTime += Time.deltaTime;
            OnRightHold?.Invoke(rightHoldTime);
        }

        if (Input.GetMouseButtonUp(1) && isRightActive)
        {
            if (rightHoldTime < 0.2f)
            {
                OnRightClick?.Invoke();
            }
            isRightActive = false; // Reset flag after release
            rightHoldTime = 0f; // Reset after release
        }
    }
}
