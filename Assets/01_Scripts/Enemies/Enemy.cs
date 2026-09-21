using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour, IDamageable
{
    [Header("Configuración")]
    public EnemyData enemyData;

    [Header("Arma del tirador")]
    public WeaponView weaponView;

    private Rigidbody rb;
    private PlayerHealth player;
    private Collider playerCollider;
    private Collider[] enemyColliders;
    private float currentHealth;
    private float nextAttackTime;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;

    private Vector3 TargetPosition => playerCollider != null ? playerCollider.bounds.center : player.transform.position + Vector3.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyColliders = GetComponentsInChildren<Collider>();

        if (enemyData == null)
        {
            enabled = false;
            return;
        }

        currentHealth = enemyData.maxHealth;
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerHealth>();
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
        }

        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void FixedUpdate()
    {
        if (IsDead || player == null || player.IsDead) return;

        Vector3 direction = TargetPosition - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            rb.MoveRotation(Quaternion.LookRotation(direction));
        }

        if (enemyData.enemyType != EnemyType.Normal) return;

        float distance = direction.magnitude;
        float step = Mathf.Min(enemyData.moveSpeed * Time.fixedDeltaTime, Mathf.Max(0f, distance - enemyData.stoppingDistance));
        rb.MovePosition(rb.position + direction.normalized * step);
    }

    private void Update()
    {
        if (IsDead || player == null || player.IsDead) return;
        if (enemyData.enemyType != EnemyType.Shooter || weaponView == null) return;

        Vector3 direction = TargetPosition - weaponView.FirePosition;
        if (direction.sqrMagnitude < 0.001f) return;

        weaponView.transform.rotation = Quaternion.LookRotation(direction);
        if (Time.time < nextAttackTime) return;

        FireProjectile();
        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsDead || player == null || player.IsDead) return;
        if (enemyData.enemyType != EnemyType.Normal || Time.time < nextAttackTime) return;
        if (other.GetComponentInParent<PlayerHealth>() != player) return;

        player.TakeDamage(enemyData.contactDamage);
        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void FireProjectile()
    {
        BulletData bulletData = enemyData.bulletData;
        if (bulletData == null || bulletData.projectilePrefab == null) return;

        Vector3 direction = (TargetPosition - weaponView.FirePosition).normalized;
        GameObject projectileObject = Instantiate(bulletData.projectilePrefab, weaponView.FirePosition, Quaternion.LookRotation(direction));

        if (!projectileObject.TryGetComponent(out BulletProjectile projectile))
        {
            Destroy(projectileObject);
            return;
        }

        foreach (Collider projectileCollider in projectileObject.GetComponentsInChildren<Collider>())
        {
            foreach (Collider enemyCollider in enemyColliders)
            {
                Physics.IgnoreCollision(projectileCollider, enemyCollider);
            }
        }

        projectile.Initialize(bulletData, direction);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (IsDead)
        {
            Destroy(gameObject);
        }
    }
}
