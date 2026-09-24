using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawnPoint : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [Min(0f)] [SerializeField] private float delay;

    public float Delay => Mathf.Max(0f, delay);
    public bool IsConfigured => enemyPrefab != null && enemyPrefab.enabled
        && enemyPrefab.gameObject.activeSelf && enemyPrefab.enemyData != null
        && enemyPrefab.enemyData.maxHealth > 0f;

    public Enemy Spawn()
    {
        if (!IsConfigured) return null;
        return Instantiate(enemyPrefab, transform.position, transform.rotation);
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
