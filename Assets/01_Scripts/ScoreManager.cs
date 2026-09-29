using System;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    [Serializable]
    public class ScoreChangedEvent : UnityEvent<int> { }

    [Header("Eventos")]
    public ScoreChangedEvent onScoreChanged;

    private int currentScore;
    private int defeatedEnemies;
    private int totalEnemies;

    public int CurrentScore => currentScore;
    public int DefeatedEnemies => defeatedEnemies;
    public int TotalEnemies => totalEnemies;

    private void Awake()
    {
        CountTotalEnemies();
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

    private void CountTotalEnemies()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            totalEnemies += root.GetComponentsInChildren<Enemy>().Length;

            foreach (EncounterController encounter in root.GetComponentsInChildren<EncounterController>())
            {
                totalEnemies += encounter.PlannedEnemyCount;
            }
        }
    }

    private void HandleEnemyDied(EnemyData enemyData)
    {
        if (enemyData == null) return;

        defeatedEnemies++;
        currentScore += enemyData.scoreValue;

        onScoreChanged?.Invoke(currentScore);
    }
}