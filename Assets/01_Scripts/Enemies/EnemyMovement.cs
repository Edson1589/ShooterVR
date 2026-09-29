using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private Enemy enemy;
    private EnemyCombat combat;
    private Rigidbody rb;
    private EnemyLocomotionAnimation locomotionAnimation;
    private Vector3 movementDirection;
    private EnemyData enemyData => enemy.enemyData;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        combat = GetComponent<EnemyCombat>();
        rb = GetComponent<Rigidbody>();
        locomotionAnimation = GetComponentInChildren<EnemyLocomotionAnimation>();
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
        {
            gameObject.layer = enemyLayer;
            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
                collider.gameObject.layer = enemyLayer;
        }
    }

    private void Start()
    {
        movementDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (enemyData == null || enemyData.enemyType != EnemyType.Shooter || !combat.HasTarget) return;
        Vector3 facing = Vector3.ProjectOnPlane(combat.TargetPosition - transform.position, Vector3.up);
        if (facing.sqrMagnitude > 0.001f)
        {
            Quaternion rotation = Quaternion.LookRotation(facing);
            rb.rotation = rotation;
            transform.rotation = rotation;
        }
    }

    private void FixedUpdate()
    {
        if (!enemy.isActiveAndEnabled || enemyData == null || enemy.IsDead || enemy.IsResolved) return;

        if (enemyData.enemyType == EnemyType.Normal)
        {
            rb.MovePosition(rb.position + movementDirection * enemyData.moveSpeed * Time.fixedDeltaTime);
            return;
        }

        if (!combat.HasTarget) return;

        Vector3 direction = combat.TargetPosition - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            rb.MoveRotation(Quaternion.LookRotation(direction));
        }

        if (enemyData.enemyType != EnemyType.Kamikaze) return;
        if (locomotionAnimation != null && (locomotionAnimation.IsAttacking || combat.IsInMeleeRange)) return;

        float distance = direction.magnitude;
        float step = Mathf.Min(enemyData.moveSpeed * Time.fixedDeltaTime, Mathf.Max(0f, distance - enemyData.stoppingDistance));
        rb.MovePosition(rb.position + direction.normalized * step);
    }
}
