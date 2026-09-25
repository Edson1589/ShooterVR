using TMPro;
using UnityEngine;

/// <summary>
/// Displays the player's current health on a wrist-mounted canvas, bound
/// to PlayerHealth's onHealthChanged event via the Inspector.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Text element that shows the current/max health.")]
    [SerializeField] private TMP_Text healthText;

    [Tooltip("Used to read MaxHealth for the '/max' part of the display.")]
    [SerializeField] private PlayerHealth playerHealth;

    public void UpdateHealthDisplay(float currentHealth)
    {
        int max = playerHealth != null ? Mathf.RoundToInt(playerHealth.MaxHealth) : 0;
        healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {max}";
    }
}
