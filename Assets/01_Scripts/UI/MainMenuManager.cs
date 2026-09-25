using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu button actions: starting a new game and quitting.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Name of the scene to load when starting a new game (must be in Build Settings).")]
    [SerializeField] private string newGameSceneName = "Level_1";

    public void NewGame()
    {
        SceneManager.LoadScene(newGameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
