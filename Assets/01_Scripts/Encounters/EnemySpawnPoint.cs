using UnityEngine;

public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Aparición del enemigo")]
    public Enemy enemyPrefab;
    public float delay;
    [Min(0)] public int waveIndex;

    public float Delay => Mathf.Max(0f, delay);
    public Enemy SpawnedEnemy { get; private set; }
    public bool IsConfigured => enemyPrefab != null && enemyPrefab.enabled && enemyPrefab.gameObject.activeSelf && enemyPrefab.enemyData != null && enemyPrefab.enemyData.maxHealth > 0f;

    public Enemy Spawn()
    {
        if (!IsConfigured) return null;
        SpawnedEnemy = Instantiate(enemyPrefab, transform.position, transform.rotation);
        return SpawnedEnemy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Vector3 tip = transform.position + transform.forward * 2f;
        Gizmos.DrawLine(transform.position, tip);
        Gizmos.DrawLine(tip, tip - transform.forward * 0.4f + transform.right * 0.25f);
        Gizmos.DrawLine(tip, tip - transform.forward * 0.4f - transform.right * 0.25f);
    }
}
