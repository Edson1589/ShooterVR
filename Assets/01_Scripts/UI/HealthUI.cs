using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [Header("Referencias")]
    public TMP_Text healthText;

    public PlayerHealth playerHealth;
    public UnityEngine.UI.Image healthFill;

    public void UpdateHealthDisplay(float currentHealth)
    {
        int max = playerHealth != null ? Mathf.RoundToInt(playerHealth.MaxHealth) : 0;
        if (healthText != null) healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {max}";
        if (healthFill != null)
        {
            float fraction = playerHealth != null && playerHealth.MaxHealth > 0f ? Mathf.Clamp01(currentHealth / playerHealth.MaxHealth) : 0f;
            healthFill.fillAmount = fraction;
            healthFill.color = fraction <= 0.25f ? new Color(1f, 0.25f, 0.25f) : fraction <= 0.5f ? new Color(1f, 0.75f, 0.2f) : new Color(0.15f, 0.9f, 0.7f);
        }
    }
}
