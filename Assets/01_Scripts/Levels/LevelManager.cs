using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [InspectorName("Movimiento del jugador")]
    [SerializeField] private AutomaticMovementVR playerMovement;
    [InspectorName("Es el último nivel")]
    [SerializeField] private bool isFinalLevel;
    [InspectorName("Ruta de la siguiente escena")]
    [Tooltip("Ruta completa de una escena habilitada en la lista de compilación, incluida la extensión .unity.")]
    [SerializeField] private string nextScenePath;
    [InspectorName("Al completar la partida")]
    [SerializeField] private UnityEvent onGameCompleted = new UnityEvent();
    [InspectorName("Nivel completado")]
    [SerializeField] private bool levelCompleted;
    [SerializeField] private GameOverManager completionScreen;

    private readonly List<EncounterController> requiredEncounters = new List<EncounterController>();
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
            Debug.LogError($"{name}: asigna el movimiento del jugador de esta escena.", this);
            enabled = false;
            return;
        }

        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>(true))
            {
                if (encounter.StopsPlayerDuringEncounter) requiredEncounters.Add(encounter);
            }
        }

        if (isFinalLevel || completionScreen != null) return;
        nextSceneBuildIndex = string.IsNullOrWhiteSpace(nextScenePath) ? -1 : SceneUtility.GetBuildIndexByScenePath(nextScenePath);
        if (nextSceneBuildIndex < 0)
        {
            Debug.LogError($"{name}: la siguiente escena no está habilitada en la compilación: {nextScenePath}", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (!levelCompleted && playerMovement != null && (reachedExit || playerMovement.HasCompletedPath))
            TryCompleteLevel(player);
    }

    public void ReachExit(PlayerHealth enteringPlayer)
    {
        if (!isActiveAndEnabled || levelCompleted || enteringPlayer == null
            || enteringPlayer != player || player.IsDead) return;

        // Entering the finish volume is sufficient; reaching its exact centre is not required.
        reachedExit = true;
        TryCompleteLevel(enteringPlayer);
    }

    public void GoToNextLevel()
    {
        if (!levelCompleted || isFinalLevel || completionScreen == null
            || string.IsNullOrWhiteSpace(nextScenePath)) return;

        completionScreen.LoadNextLevel(nextScenePath);
    }

    public bool TryCompleteLevel(PlayerHealth enteringPlayer)
    {
        if (!isActiveAndEnabled || levelCompleted || enteringPlayer == null || enteringPlayer != player || player.IsDead) return false;
        if ((!reachedExit && !playerMovement.HasCompletedPath) || playerMovement.IsMovementPaused) return false;

        foreach (EncounterController encounter in requiredEncounters)
        {
            if (encounter == null || encounter.State != EncounterController.EncounterState.Completed) return false;
        }

        levelCompleted = true;
        playerMovement.StopMovement();
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
        catch (System.Exception exception)
        {
            levelCompleted = false;
            enabled = false;
            Debug.LogError($"{name}: no se pudo cargar la siguiente escena. {exception.Message}", this);
            return false;
        }
    }
}
