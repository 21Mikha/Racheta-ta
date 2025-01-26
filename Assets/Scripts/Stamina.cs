using UnityEngine;

public class Stamina
{
    public float MaxStamina { get; private set; }
    public float CurrentStamina { get; private set; }
    public float RegenRate { get; private set; }
    public float DepletionRate { get; private set; }

    public event System.Action<float, float> OnStaminaChanged; // Event for UI updates

    public Stamina(float maxStamina, float regenRate, float depletionRate)
    {
        MaxStamina = maxStamina;
        CurrentStamina = maxStamina;
        RegenRate = regenRate;
        DepletionRate = depletionRate;
    }

    public void Update(bool isDepleting)
    {
        if (isDepleting)
        {
            DepleteStamina(Time.fixedDeltaTime);
        }
        else
        {
            RegenerateStamina(Time.fixedDeltaTime);
        }

        // Notify subscribers of the stamina change
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    public bool CanPerformAction(float requiredStamina)
    {
        return CurrentStamina >= requiredStamina;
    }

    private void DepleteStamina(float deltaTime)
    {
        CurrentStamina -= DepletionRate * deltaTime;
        if (CurrentStamina < 0)
        {
            CurrentStamina = 0;
        }
    }

    private void RegenerateStamina(float deltaTime)
    {
        CurrentStamina += RegenRate * deltaTime;
        if (CurrentStamina > MaxStamina)
        {
            CurrentStamina = MaxStamina;
        }
    }
}
