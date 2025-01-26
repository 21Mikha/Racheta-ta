using UnityEngine;

public class LaunchDirectionIndicator : MonoBehaviour
{
    [SerializeField] private PlayerController playerController; // Reference to the PlayerController

    private void Awake()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController is not assigned!");
        }

    }

    private void Update()
    {
        if (playerController == null)
            return;

            // Update indicator based on angle
            float launchAngle = playerController.GetLaunchAngle();
            this.transform.rotation = Quaternion.Euler(90f, launchAngle, 0f);

    }
}
