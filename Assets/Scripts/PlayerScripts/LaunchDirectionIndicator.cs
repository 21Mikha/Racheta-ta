using UnityEngine;

public class LaunchDirectionIndicator : MonoBehaviour
{
    [SerializeField] private PlayerController playerController; // Reference to the PlayerController
    [SerializeField] private Transform indicatorObject;         // The object to rotate (e.g., an arrow)

    private void Awake()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is not assigned!");
        }

        if (indicatorObject == null)
        {
            Debug.LogError("Indicator object is not assigned!");
        }
    }

    private void Update()
    {
        if (playerController == null || indicatorObject == null)
            return;

            // Update indicator based on angle
            float launchAngle = playerController.GetLaunchAngle();
            indicatorObject.rotation = Quaternion.Euler(90f, launchAngle, 0f);

    }
}
