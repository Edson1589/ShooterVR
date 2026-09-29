using UnityEngine;

public class EncounterTrigger : MonoBehaviour
{
    [Header("Activación del encuentro")]
    public EncounterController encounter;
    public bool resolveOnEnter;

    private void Reset()
    {
        encounter = GetComponentInParent<EncounterController>();
        ConfigurePhysics();
    }

    private void Awake()
    {
        if (encounter == null) encounter = GetComponentInParent<EncounterController>();
        ConfigurePhysics();
    }

    private void ConfigurePhysics()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled || encounter == null || other.isTrigger) return;
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.IsDead) return;

        if (resolveOnEnter) encounter.ResolveRemainingEnemies();
        else encounter.TryBegin(player);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null) return;
        Gizmos.color = resolveOnEnter ? Color.magenta : Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(zone.center, zone.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
