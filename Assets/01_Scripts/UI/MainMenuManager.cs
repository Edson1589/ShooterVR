using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the main menu button actions: starting a new game and quitting.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Tooltip("Name of the scene to load when starting a new game (must be in Build Settings).")]
    [SerializeField] private string newGameSceneName = "Level_1";

    [Tooltip("Name of the level 2 scene to load (must be in Build Settings).")]
    [SerializeField] private string level2SceneName = "Level_2";

    public void NewGame()
    {
        SceneManager.LoadScene(newGameSceneName);
    }

    public void StartLevel2()
    {
        SceneManager.LoadScene(level2SceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
