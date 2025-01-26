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
}
