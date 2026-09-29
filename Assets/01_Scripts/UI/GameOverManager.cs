using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject gameOverPanel;
    public Transform playerCamera;
    public PlayerHealth player;
    public ScoreManager scoreManager;
    [Header("Resultados de derrota")]
    public TMP_Text scoreText;
    public TMP_Text gameOverEnemiesText;
    [Header("Control de la partida")]
    public Behaviour[] gameplayToDisable = new Behaviour[0];
    public string mainMenuScenePath = "Assets/00_Scenes/MainMenu.unity";

    [Header("Victoria")]
    public GameObject victoryPanel;
    public TMP_Text victoryScoreText;
    public TMP_Text victoryEnemiesText;

    [Header("Tiempo del nivel")]
    public TMP_Text gameOverTimeText;
    public TMP_Text victoryTimeText;
    private double levelStartedAt;
    private bool shown;
    private bool loading;
    private float previousTimeScale = 1f;

    private int ElapsedSeconds => (int)System.Math.Max(0d, System.Math.Floor(Time.timeAsDouble - levelStartedAt));

    public SaveData.LevelResult GetLevelResult()
    {
        return new SaveData.LevelResult
        {
            scene = gameObject.scene.path,
            score = scoreManager != null ? scoreManager.CurrentScore : 0,
            defeatedEnemies = scoreManager != null ? scoreManager.DefeatedEnemies : 0,
            elapsedSeconds = ElapsedSeconds
        };
    }

    private void Awake()
    {
        levelStartedAt = Time.timeAsDouble;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (shown || gameOverPanel == null || playerCamera == null) return;
        if (gameOverEnemiesText != null) gameOverEnemiesText.text = $"{(scoreManager != null ? scoreManager.DefeatedEnemies : 0)} / {(scoreManager != null ? scoreManager.TotalEnemies : 0)}";
        if (scoreText != null) scoreText.text = $"Puntuación: {(scoreManager != null ? scoreManager.CurrentScore : 0)}";
        ShowResult(gameOverPanel);
    }

    public void ShowVictory()
    {
        if (shown || victoryPanel == null || playerCamera == null) return;
        if (victoryScoreText != null) victoryScoreText.text = $"Puntuación: {(scoreManager != null ? scoreManager.CurrentScore : 0)}";
        if (victoryEnemiesText != null) victoryEnemiesText.text = $"{(scoreManager != null ? scoreManager.DefeatedEnemies : 0)} / {(scoreManager != null ? scoreManager.TotalEnemies : 0)}";
        ShowResult(victoryPanel);
    }

    private void ShowResult(GameObject resultPanel)
    {
        shown = true;
        if (SceneAudio.Instance != null) SceneAudio.Instance.Finish(resultPanel == victoryPanel);
        int seconds = ElapsedSeconds;
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
        if (resultPanel == gameOverPanel) HideEnemiesOnGameOver();
        Vector3 forward = Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();
        resultPanel.transform.SetPositionAndRotation(playerCamera.position + forward * 2f, Quaternion.LookRotation(forward));
        resultPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void HideEnemiesOnGameOver()
    {
        GameObject[] roots = gameObject.scene.GetRootGameObjects();

        foreach (GameObject root in roots)
            foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>(true))
                encounter.enabled = false;

        foreach (GameObject root in roots)
            foreach (Enemy enemy in root.GetComponentsInChildren<Enemy>(true))
                enemy.gameObject.SetActive(false);
    }

    public void Retry()
    {
        LoadScene(gameObject.scene.path);
    }

    public void GoToMainMenu()
    {
        LoadScene(mainMenuScenePath);
    }

    public void LoadNextLevel(string scenePath)
    {
        if (victoryPanel == null || !victoryPanel.activeInHierarchy) return;
        LoadScene(scenePath);
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
        catch (System.Exception)
        {
            loading = false;
            Time.timeScale = 0f;
        }
    }

    private void OnDestroy()
    {
        if (shown && !loading) Time.timeScale = previousTimeScale;
    }
}
