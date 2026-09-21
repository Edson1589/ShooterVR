using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Shows the Game Over screen when the player dies and freezes gameplay
/// (player movement, enemies, physics) via Time.timeScale, without needing
/// to know how those systems are implemented internally.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root GameObject of the Game Over panel (child of the camera, hidden until death).")]
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        // TODO: cargar la escena del menu principal cuando exista (tarjeta "Crear menu principal").
        Debug.LogWarning("[GameOverManager] Menu principal todavia no implementado.");
    }
}
