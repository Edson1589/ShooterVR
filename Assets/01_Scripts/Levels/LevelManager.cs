using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Referencias")]
    public AutomaticMovementVR playerMovement;
    public GameOverManager completionScreen;

    [Tooltip("Si se asigna, comienza al terminar el recorrido y es el único encuentro obligatorio.")]
    public EncounterController finalEncounter;

    [Header("Progresión del nivel")]
    public bool isFinalLevel;

    public string nextScenePath;

    [Header("Eventos")]
    public UnityEvent onGameCompleted = new UnityEvent();

    [Header("Estado del nivel")]
    public bool levelCompleted;

    private readonly List<EncounterController> requiredEncounters = new List<EncounterController>();
    private readonly List<EncounterController> routeEncounters = new List<EncounterController>();
    private PlayerHealth player;
    private int nextSceneBuildIndex;
    private bool reachedExit;

    public bool IsLevelCompleted => levelCompleted;
    public bool IsGameCompleted => isFinalLevel && levelCompleted;

    private void Awake()
    {
        levelCompleted = false;
        reachedExit = false;
        player = playerMovement != null ? playerMovement.GetComponent<PlayerHealth>() : null;
        if (player == null || player.gameObject.scene != gameObject.scene)
        {
            enabled = false;
            return;
        }

        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>(true))
            {
                if (finalEncounter != null)
                {
                    if (encounter != finalEncounter) routeEncounters.Add(encounter);
                }
                else if (encounter.StopsPlayerDuringEncounter) requiredEncounters.Add(encounter);
            }
        }
        if (finalEncounter != null) requiredEncounters.Add(finalEncounter);

        if (isFinalLevel) return;
        nextSceneBuildIndex = string.IsNullOrWhiteSpace(nextScenePath) ? -1 : SceneUtility.GetBuildIndexByScenePath(nextScenePath);
        if (nextSceneBuildIndex < 0)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        if (!levelCompleted && finalEncounter != null && player != null && !player.IsDead
            && playerMovement.HasCompletedPath && !playerMovement.IsMovementPaused
            && finalEncounter.State == EncounterController.EncounterState.Waiting)
        {
            foreach (EncounterController encounter in routeEncounters) encounter.ResolveRemainingEnemies();
            finalEncounter.TryBegin(player);
        }
        if (!levelCompleted && playerMovement != null && (reachedExit || playerMovement.HasCompletedPath)) TryCompleteLevel(player);
    }

    public void ReachExit(PlayerHealth enteringPlayer)
    {
        if (!isActiveAndEnabled || levelCompleted || enteringPlayer == null || enteringPlayer != player || player.IsDead) return;
        reachedExit = true;
        TryCompleteLevel(enteringPlayer);
    }

    public void GoToNextLevel()
    {
        if (!levelCompleted || isFinalLevel || completionScreen == null || string.IsNullOrWhiteSpace(nextScenePath)) return;

        completionScreen.LoadNextLevel(nextScenePath);
    }

    public bool TryCompleteLevel(PlayerHealth enteringPlayer)
    {
        if (!isActiveAndEnabled || levelCompleted || enteringPlayer == null || enteringPlayer != player || player.IsDead) return false;
        if (finalEncounter != null && !playerMovement.HasCompletedPath) return false;
        if ((!reachedExit && !playerMovement.HasCompletedPath) || playerMovement.IsMovementPaused) return false;

        foreach (EncounterController encounter in requiredEncounters)
        {
            if (encounter == null || encounter.State != EncounterController.EncounterState.Completed) return false;
        }

        levelCompleted = true;
        playerMovement.StopMovement();
        SaveSystem.CompleteLevel(nextScenePath, isFinalLevel, completionScreen != null ? completionScreen.GetLevelResult() : null);
        if (completionScreen != null)
        {
            completionScreen.ShowVictory();
            if (isFinalLevel) onGameCompleted.Invoke();
            return true;
        }
        if (isFinalLevel)
        {
            onGameCompleted.Invoke();
            return true;
        }

        try
        {
            SceneManager.LoadSceneAsync(nextSceneBuildIndex, LoadSceneMode.Single);
            return true;
        }
        catch (System.Exception)
        {
            levelCompleted = false;
            enabled = false;
            return false;
        }
    }
}
