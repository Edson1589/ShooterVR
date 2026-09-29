using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneRestart : MonoBehaviour
{
    private bool isRestarting;

    public void RestartScene()
    {
        if (isRestarting) return;
        isRestarting = true;
        SceneManager.LoadScene(gameObject.scene.path);
    }
}
