using TMPro;
using UnityEngine;

public class GameplayHUD : MonoBehaviour
{
    [Header("Datos del jugador")]
    public PlayerHealth player;
    public ScoreManager scoreManager;
    [Header("Elementos de la interfaz")]
    public HealthUI healthUI;
    public TMP_Text scoreText;
    public Canvas hudCanvas;

    private void OnEnable()
    {
        if (hudCanvas != null) hudCanvas.enabled = true;
        if (player != null) player.OnHealthChanged.AddListener(UpdateHealth);
        if (scoreManager != null) scoreManager.onScoreChanged.AddListener(UpdateScore);
    }

    private void Start()
    {
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
