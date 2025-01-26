using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Gradient staminaGradient;
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        if (playerController != null)
        {
            playerController.Stamina.OnStaminaChanged += UpdateStaminaUI;
            InitializeStaminaUI(playerController.Stamina.MaxStamina);
        }
    }

    private void InitializeStaminaUI(float maxStamina)
    {
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;
        fillImage.color = staminaGradient.Evaluate(1f);
    }

    private void UpdateStaminaUI(float currentStamina, float maxStamina)
    {
        staminaSlider.value = currentStamina;
        fillImage.color = staminaGradient.Evaluate(currentStamina / maxStamina);
    }
}
