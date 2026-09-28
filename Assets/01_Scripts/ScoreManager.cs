using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Tracks the player's score by listening to Enemy.OnEnemyDied and adding
/// each enemy's configured scoreValue. Exposes onScoreChanged for the HUD
/// to bind to without needing a direct reference to this component.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Serializable]
    public class ScoreChangedEvent : UnityEvent<int> { }

    [Header("Events")]
    [Tooltip("Invoked whenever the score changes, passing the new total.")]
    [SerializeField] private ScoreChangedEvent onScoreChanged;

    private int currentScore;
    private int defeatedEnemies;
    private int totalEnemies;

    public int CurrentScore => currentScore;
    public int DefeatedEnemies => defeatedEnemies;
    public int TotalEnemies => totalEnemies;

    private void Awake()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            totalEnemies += root.GetComponentsInChildren<Enemy>().Length;
            foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>())
                totalEnemies += encounter.PlannedEnemyCount;
        }
    }

    private void OnEnable()
    {
        Enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
    }

    private void Start()
    {
        onScoreChanged?.Invoke(currentScore);
    }

    private void HandleEnemyDied(EnemyData enemyData)
    {
        defeatedEnemies++;
        currentScore += enemyData.scoreValue;
        onScoreChanged?.Invoke(currentScore);
    }
}
