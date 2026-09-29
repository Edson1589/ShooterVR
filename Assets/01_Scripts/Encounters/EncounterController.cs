using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterController : MonoBehaviour
{
    public enum EncounterState { Waiting, Running, Completed, Cancelled }

    [Header("Configuración del encuentro")]
    public bool stopPlayerDuringEncounter = true;
    public EnemySpawnPoint[] spawnPoints = new EnemySpawnPoint[0];

    [Header("Oleadas (opcional)")]
    public bool waitForWaveClear;
    [Min(0f)] public float waveBreakDuration = 3f;
    public bool preventEnemyDespawn;

    [Header("Aviso previo (opcional)")]
    public LevelBriefingUI briefing;
    [TextArea] public string warningMessage;
    [Min(0f)] public float warningDuration;
    [Header("Estado del encuentro")]
    public EncounterState state;
    public int pendingSpawns;
    public int activeEnemyCount;

    [InspectorName("Enemigos resueltos")]
    public int resolvedEnemyCount;

    private readonly HashSet<Enemy> activeEnemies = new HashSet<Enemy>();
    private AutomaticMovementVR pausedMovement;

    public EncounterState State => state;
    public bool StopsPlayerDuringEncounter => stopPlayerDuringEncounter;
    public int PendingSpawns => pendingSpawns;
    public int ActiveEnemyCount => activeEnemyCount;
    public int ResolvedEnemyCount => resolvedEnemyCount;
    public int PlannedEnemyCount => spawnPoints.Length;

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
            if (point == null || !point.IsConfigured || !uniquePoints.Add(point)) return;
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
        if (SceneAudio.Instance != null) SceneAudio.Instance.BeginEncounter(this);
        pendingSpawns = spawnPoints.Length;
        StartCoroutine(BeginSpawns());
    }

    private IEnumerator BeginSpawns()
    {
        if (briefing != null && warningDuration > 0f && !string.IsNullOrWhiteSpace(warningMessage))
        {
            briefing.ShowAmbushWarning(this, "¡EMBOSCADA!", warningMessage, warningDuration);
            yield return new WaitForSeconds(warningDuration);
            briefing.HideMessage(this);
        }

        if (waitForWaveClear)
        {
            var waves = new SortedDictionary<int, List<EnemySpawnPoint>>();
            foreach (EnemySpawnPoint point in spawnPoints)
            {
                if (!waves.TryGetValue(point.waveIndex, out List<EnemySpawnPoint> points))
                {
                    points = new List<EnemySpawnPoint>();
                    waves.Add(point.waveIndex, points);
                }
                points.Add(point);
            }
            bool firstWave = true;
            int unscheduled = spawnPoints.Length;
            foreach (List<EnemySpawnPoint> points in waves.Values)
            {
                if (!firstWave) yield return new WaitForSeconds(waveBreakDuration);
                firstWave = false;
                unscheduled -= points.Count;
                foreach (EnemySpawnPoint point in points) StartCoroutine(SpawnAfterDelay(point));
                while (pendingSpawns > unscheduled || activeEnemies.Count > 0) yield return null;
            }
        }
        else foreach (EnemySpawnPoint point in spawnPoints)
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
            enemy.PreventDespawn = preventEnemyDespawn;
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
        if (SceneAudio.Instance != null) SceneAudio.Instance.EndEncounter(this, true);
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
        if (SceneAudio.Instance != null) SceneAudio.Instance.EndEncounter(this, false);
        StopAllCoroutines();
        if (briefing != null) briefing.HideMessage(this);
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
