using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EncounterController : MonoBehaviour
{
    public enum EncounterState { Waiting, Running, Completed, Cancelled }

    [SerializeField] private bool stopPlayerDuringEncounter = true;
    [SerializeField] private EnemySpawnPoint[] spawnPoints = new EnemySpawnPoint[0];
    [SerializeField] private EncounterState state;
    [SerializeField] private int pendingSpawns;
    [SerializeField] private int activeEnemyCount;
    [InspectorName("Enemigos resueltos")]
    [SerializeField] private int resolvedEnemyCount;

    private readonly HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
    private AutomaticMovementVR pausedMovement;

    public EncounterState State => state;
    public bool StopsPlayerDuringEncounter => stopPlayerDuringEncounter;
    public int PendingSpawns => pendingSpawns;
    public int ActiveEnemyCount => activeEnemyCount;
    public int ResolvedEnemyCount => resolvedEnemyCount;

    private void Awake()
    {
        state = EncounterState.Waiting;
        pendingSpawns = 0;
        activeEnemyCount = 0;
        resolvedEnemyCount = 0;
    }

    public void TryBegin(PlayerHealth player)
    {
        if (!isActiveAndEnabled || state != EncounterState.Waiting || player == null || player.IsDead) return;

        var uniquePoints = new HashSet<EnemySpawnPoint>();
        foreach (EnemySpawnPoint point in spawnPoints)
        {
            if (point == null || !point.IsConfigured || !uniquePoints.Add(point))
            {
                return;
            }
        }

        if (stopPlayerDuringEncounter)
        {
            pausedMovement = player.GetComponentInParent<AutomaticMovementVR>();
            if (pausedMovement == null || !pausedMovement.isActiveAndEnabled)
            {
                pausedMovement = null;
                return;
            }
            pausedMovement.PauseMovement(this);
        }

        state = EncounterState.Running;
        pendingSpawns = spawnPoints.Length;
        foreach (EnemySpawnPoint point in spawnPoints)
        {
            StartCoroutine(SpawnAfterDelay(point));
        }
        TryComplete();
    }

    private IEnumerator SpawnAfterDelay(EnemySpawnPoint point)
    {
        if (point.Delay > 0f) yield return new WaitForSeconds(point.Delay);

        Enemy enemy = point != null ? point.Spawn() : null;
        if (enemy != null)
        {
            activeEnemies.Add(enemy);
            enemy.Resolved += HandleEnemyResolved;
            activeEnemyCount = activeEnemies.Count;
        }

        pendingSpawns--;
        TryComplete();
    }

    private void HandleEnemyResolved(Enemy enemy)
    {
        if (!activeEnemies.Remove(enemy)) return;
        enemy.Resolved -= HandleEnemyResolved;
        resolvedEnemyCount++;
        activeEnemyCount = activeEnemies.Count;
        TryComplete();
    }

    public void ResolveRemainingEnemies()
    {
        if (!isActiveAndEnabled || stopPlayerDuringEncounter || state != EncounterState.Running) return;

        StopAllCoroutines();
        pendingSpawns = 0;

        foreach (Enemy enemy in new List<Enemy>(activeEnemies))
        {
            if (enemy != null) enemy.Resolve();
        }
        TryComplete();
    }

    private void TryComplete()
    {
        if (state != EncounterState.Running || pendingSpawns > 0 || activeEnemies.Count > 0) return;
        state = EncounterState.Completed;
        ReleaseMovement();
    }

    private void ReleaseMovement()
    {
        if (pausedMovement == null) return;
        pausedMovement.ResumeMovement(this);
        pausedMovement = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (state == EncounterState.Running) state = EncounterState.Cancelled;
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null) enemy.Resolved -= HandleEnemyResolved;
        }
        activeEnemies.Clear();
        activeEnemyCount = 0;
        pendingSpawns = 0;
        ReleaseMovement();
    }
}
