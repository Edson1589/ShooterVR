using TMPro;
using UnityEngine;

/// <summary>Connects the compact headset HUD to the level's existing health and score.</summary>
[DisallowMultipleComponent]
public class GameplayHUD : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Canvas hudCanvas;

    private void OnEnable()
    {
        if (hudCanvas != null) hudCanvas.enabled = true;
        if (player != null) player.OnHealthChanged.AddListener(UpdateHealth);
        if (scoreManager != null) scoreManager.onScoreChanged.AddListener(UpdateScore);
    }

    private void Start()
    {
        // All Awake methods have initialized health before the first display refresh.
        if (player != null) UpdateHealth(player.CurrentHealth);
        if (scoreManager != null) UpdateScore(scoreManager.CurrentScore);
    }

    private void UpdateHealth(float value)
    {
        if (healthUI != null) healthUI.UpdateHealthDisplay(value);
    }

    private void UpdateScore(int value)
    {
        if (scoreText != null) scoreText.text = value.ToString();
    }

    private void OnDisable()
    {
        if (player != null) player.OnHealthChanged.RemoveListener(UpdateHealth);
        if (scoreManager != null) scoreManager.onScoreChanged.RemoveListener(UpdateScore);
        if (hudCanvas != null) hudCanvas.enabled = false;
    }
}
