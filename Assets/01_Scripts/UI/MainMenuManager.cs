using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("Escenas de juego")]
    public string newGameSceneName = "Level_1";

    public string level2SceneName = "Level_2";

    [Header("Continuar partida")]
    public Button continueButton;
    public TMP_Text continueDescription;
    private bool loading;

    private void OnEnable()
    {
        RefreshContinueButton();
    }

    private void RefreshContinueButton()
    {
        bool hasSave = SaveSystem.TryLoad(out SaveData data);
        if (continueButton != null) continueButton.interactable = hasSave && !data.gameCompleted;
        if (continueDescription != null) continueDescription.text = !hasSave ? "SIN PARTIDA GUARDADA" : data.gameCompleted ? "CAMPAÑA COMPLETADA" : $"RETOMAR {System.IO.Path.GetFileNameWithoutExtension(data.nextScene).Replace('_', ' ')} DESDE EL INICIO";
    }

    public void NewGame()
    {
        StartNewGame(newGameSceneName);
    }

    public void StartLevel2()
    {
        StartNewGame(level2SceneName);
    }

    public void ContinueGame()
    {
        if (loading) return;
        if (SaveSystem.TryLoad(out SaveData data) && !data.gameCompleted) LoadLevel(data.nextScene);
        else RefreshContinueButton();
    }

    private void StartNewGame(string scene)
    {
        if (loading) return;
        if (SaveSystem.StartNewGame(scene)) LoadLevel(scene);
        else if (continueDescription != null) continueDescription.text = "NO SE PUDO GUARDAR. INTENTA DE NUEVO.";
    }

    private void LoadLevel(string scene)
    {
        loading = true;
        Time.timeScale = 1f;
        try
        {
            SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        }
        catch (System.Exception)
        {
            loading = false;
            RefreshContinueButton();
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
