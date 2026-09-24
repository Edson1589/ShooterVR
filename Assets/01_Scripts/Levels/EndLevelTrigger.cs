using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class EndLevelTrigger : MonoBehaviour
{
    [InspectorName("Gestor del nivel")]
    [SerializeField] private LevelManager levelManager;

    private void Reset()
    {
        levelManager = GetComponentInParent<LevelManager>();
        ConfigurePhysics();
    }

    private void Awake()
    {
        if (levelManager == null) levelManager = GetComponentInParent<LevelManager>();
        ConfigurePhysics();
    }

    private void ConfigurePhysics()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnTriggerEnter(Collider other) => TryFinish(other);

    private void OnTriggerStay(Collider other) => TryFinish(other);

    private void TryFinish(Collider other)
    {
        if (!isActiveAndEnabled || other.isTrigger || levelManager == null || levelManager.IsLevelCompleted) return;
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player != null) levelManager.TryCompleteLevel(player);
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider zone = GetComponent<BoxCollider>();
        if (zone == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(zone.center, zone.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
