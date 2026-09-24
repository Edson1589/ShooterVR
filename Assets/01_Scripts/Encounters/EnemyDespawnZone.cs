using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class EnemyDespawnZone : MonoBehaviour
{
    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled || other.isTrigger) return;
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null) enemy.Resolve();
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null) return;
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(zone.center, zone.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
