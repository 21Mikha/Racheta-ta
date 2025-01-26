using UnityEngine;
using System.Collections;

public class LaunchPowerIndicator : MonoBehaviour
{
    [SerializeField] private Material launchPowerMaterial; // The material with the circular indicator shader
    [SerializeField] private InputHandler inputHandler;    // Reference to the input handler
    private float maxHoldTime;

    private float previousFillAmount = 0f; // To track the previous frame's fill amount
    private bool isFading = false;         // Prevents multiple fade coroutines

    private void Awake()
    {
        maxHoldTime = inputHandler.GetMaxHoldTime();
        launchPowerMaterial.SetFloat("_FillAmount", 0);
    }

    private void UpdateFillAmount(float holdTime)
    {
        if (launchPowerMaterial == null || isFading ) return;

        // Normalize the hold time to the range [0, 0.5]
        float currentFillAmount = Mathf.Clamp01(holdTime / maxHoldTime) * 0.5f;

        // Trigger FadeAndReset when previous fill amount > 0.2 and current fill amount == 0
        if (previousFillAmount > 0.2f && holdTime <=0.2f)
        {
            StartCoroutine(FadeAndReset());
        }
        else
        {
            // Update the shader's Fill Amount property
            launchPowerMaterial.SetFloat("_FillAmount", currentFillAmount);
        }

        // Update the previous fill amount for the next frame
        previousFillAmount = currentFillAmount;
    }

    private void Update()
    {
        UpdateFillAmount(inputHandler.GetHoldTime());
    }

    private IEnumerator FadeAndReset()
    {
        if (isFading) yield break; // Prevent multiple fade coroutines
        isFading = true;

        // Fade out the material's alpha
        Color materialColor = launchPowerMaterial.color;
        float fadeDuration = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            materialColor.a = alpha;
            launchPowerMaterial.color = materialColor;
            yield return null;
        }

        // Reset alpha to 1 and fill amount to 0
        materialColor.a = 1f;
        launchPowerMaterial.color = materialColor;
        launchPowerMaterial.SetFloat("_FillAmount", 0);

        isFading = false;
    }
}
