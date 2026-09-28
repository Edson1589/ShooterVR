using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Shows the Game Over screen when the player dies and freezes gameplay
/// (player movement, enemies, physics) via Time.timeScale, without needing
/// to know how those systems are implemented internally.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("World-space panel positioned in front of the headset once on death.")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private PlayerHealth player;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Behaviour[] gameplayToDisable = new Behaviour[0];
    [SerializeField] private string mainMenuScenePath = "Assets/00_Scenes/MainMenu.unity";
    [Header("Victoria")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text victoryScoreText;
    [SerializeField] private TMP_Text victoryEnemiesText;
    [Header("Tiempo del nivel")]
    [SerializeField] private TMP_Text gameOverTimeText;
    [SerializeField] private TMP_Text victoryTimeText;
    private double levelStartedAt;
    private bool shown;
    private bool loading;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        levelStartedAt = Time.timeAsDouble;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (shown || gameOverPanel == null || playerCamera == null) return;
        if (scoreText != null) scoreText.text = $"Puntuación: {(scoreManager != null ? scoreManager.CurrentScore : 0)}";
        ShowResult(gameOverPanel);
    }

    public void ShowVictory()
    {
        if (shown || victoryPanel == null || playerCamera == null) return;
        if (victoryScoreText != null)
            victoryScoreText.text = $"Puntuación: {(scoreManager != null ? scoreManager.CurrentScore : 0)}";
        if (victoryEnemiesText != null)
            victoryEnemiesText.text = $"Enemigos eliminados: {(scoreManager != null ? scoreManager.DefeatedEnemies : 0)} / {(scoreManager != null ? scoreManager.TotalEnemies : 0)}";
        ShowResult(victoryPanel);
    }

    private void ShowResult(GameObject resultPanel)
    {
        shown = true;
        int seconds = (int)System.Math.Max(0d, System.Math.Floor(Time.timeAsDouble - levelStartedAt));
        string elapsed = $"Tiempo: {seconds / 60:00}:{seconds % 60:00}";
        if (gameOverTimeText != null) gameOverTimeText.text = elapsed;
        if (victoryTimeText != null) victoryTimeText.text = elapsed;
        previousTimeScale = Time.timeScale;
        if (scoreManager != null) scoreManager.enabled = false;
        foreach (Behaviour behaviour in gameplayToDisable)
            if (behaviour != null) behaviour.enabled = false;
        if (player != null)
        {
            foreach (WeaponShooter shooter in player.GetComponentsInChildren<WeaponShooter>(true)) shooter.enabled = false;
            foreach (MeleeAttack melee in player.GetComponentsInChildren<MeleeAttack>(true)) melee.enabled = false;
        }
        Vector3 forward = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();
        resultPanel.transform.SetPositionAndRotation(playerCamera.position + forward * 2f, Quaternion.LookRotation(forward));
        resultPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        LoadScene(gameObject.scene.path);
    }

    public void GoToMainMenu()
    {
        LoadScene(mainMenuScenePath);
    }

    private void LoadScene(string path)
    {
        if (!shown || loading) return;
        loading = true;
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadSceneAsync(path, LoadSceneMode.Single);
        }
        catch (System.Exception exception)
        {
            loading = false;
            Time.timeScale = 0f;
            Debug.LogError($"No se pudo cargar {path}: {exception.Message}", this);
        }
    }

    private void OnDestroy()
    {
        if (shown && !loading) Time.timeScale = previousTimeScale;
    }
}
