using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSceneRestart : MonoBehaviour
{
    private bool isRestarting;

    // Se conecta al evento de muerte de PlayerHealth desde el Inspector.
    public void RestartScene()
    {
        if (isRestarting) return;
        isRestarting = true;
        SceneManager.LoadScene(gameObject.scene.path);
    }
}
