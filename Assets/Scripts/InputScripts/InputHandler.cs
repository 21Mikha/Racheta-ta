using UnityEngine;
using System;

public abstract class InputHandler : MonoBehaviour
{
    public Action<Vector2> OnMovementInput;  // Movement event
    public Action OnLeftClick;              // Left click event
    public Action OnRightClick;             // Right click event
    public Action<float> OnLeftHold;        // Left hold event
    public Action<float> OnRightHold;       // Right hold event
    public Action<bool> OnSprintInput;
    public Action<float> OnLaunchDirectionChanged;


    protected float maxHoldTime = 1.0f; // Maximum time for holding the button
    protected float holdTime = 0f;
    protected float cooldownPeriod = 1.0f; // Cooldown period in seconds
    public abstract void SetMaxHoldTime(float maxTime);
    public abstract float GetMaxHoldTime();
    public abstract float GetHoldTime();
    public abstract void ActivateCooldown();
}
