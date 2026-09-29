using TMPro;
using UnityEngine;

/// <summary>
/// Displays current/max health and an optional health bar.
/// Can be driven by GameplayHUD or PlayerHealth's Inspector event.
/// </summary>
public class HealthUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Text element that shows the current/max health.")]
    [SerializeField] private TMP_Text healthText;

    [Tooltip("Used to read MaxHealth for the '/max' part of the display.")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private UnityEngine.UI.Image healthFill;

    public void UpdateHealthDisplay(float currentHealth)
    {
        int max = playerHealth != null ? Mathf.RoundToInt(playerHealth.MaxHealth) : 0;
        if (healthText != null) healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {max}";
        if (healthFill != null)
        {
            float fraction = playerHealth != null && playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(currentHealth / playerHealth.MaxHealth) : 0f;
            healthFill.fillAmount = fraction;
            healthFill.color = fraction <= 0.25f ? new Color(1f, 0.25f, 0.25f)
                : fraction <= 0.5f ? new Color(1f, 0.75f, 0.2f) : new Color(0.15f, 0.9f, 0.7f);
        }
    }
}
