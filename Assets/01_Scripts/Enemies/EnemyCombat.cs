using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Arma del tirador")]
    public WeaponView weaponView;

    private Enemy enemy;
    private PlayerHealth player;
    private Collider playerCollider;
    private Collider[] enemyColliders;
    private EnemyLocomotionAnimation locomotionAnimation;
    private ShooterVisualController shooterVisual;
    private float nextAttackTime;

    private EnemyData enemyData => enemy.enemyData;
    private bool IsDead => enemy.IsDead;
    private bool IsResolved => enemy.IsResolved;
    public bool CanAct => isActiveAndEnabled && enemy != null && enemy.isActiveAndEnabled && enemyData != null && !IsDead && !IsResolved;
    public bool HasTarget => player != null && !player.IsDead;
    public Vector3 TargetPosition => playerCollider != null ? playerCollider.bounds.center : player != null ? player.transform.position + Vector3.up : transform.position;
    public WeaponView Weapon => weaponView;
    public bool IsInMeleeRange => IsWithinMeleeRange(0f);
    public bool CanContinueMeleeAttack => IsWithinMeleeRange(0.2f);

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyColliders = GetComponentsInChildren<Collider>(true);
        locomotionAnimation = GetComponentInChildren<EnemyLocomotionAnimation>();
        shooterVisual = GetComponent<ShooterVisualController>();
        if (weaponView == null) weaponView = GetComponentInChildren<WeaponView>();
        player = FindAnyObjectByType<PlayerHealth>();
        if (player != null) playerCollider = player.GetComponent<Collider>();
    }

    private void Start()
    {
        if (enemyData != null) nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void LateUpdate()
    {
        if (!CanAct || enemyData.enemyType != EnemyType.Shooter) return;
        if (shooterVisual != null && shooterVisual.isActiveAndEnabled)
            shooterVisual.UpdatePose(HasTarget, TargetPosition);
        if (!HasTarget || weaponView == null || !weaponView.HasFirePoint) return;
        if ((TargetPosition - weaponView.FirePosition).sqrMagnitude < 0.001f) return;
        if (Time.time < nextAttackTime) return;
        FireProjectile();
        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!CanAct || other.isTrigger) return;
        if (IsDead || IsResolved || player == null || player.IsDead) return;
        if (enemyData.enemyType != EnemyType.Normal && enemyData.enemyType != EnemyType.Kamikaze) return;
        if (enemyData.enemyType == EnemyType.Kamikaze && locomotionAnimation != null) return;
        if (Time.time < nextAttackTime) return;
        if (other.GetComponentInParent<PlayerHealth>() != player) return;

        player.TakeDamage(enemyData.contactDamage);
        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private bool IsWithinMeleeRange(float margin)
    {
        if (!CanAct || player == null || player.IsDead || enemyData.enemyType != EnemyType.Kamikaze) return false;
        Vector3 offset = TargetPosition - transform.position;
        if (Mathf.Abs(offset.y) > 1.2f) return false;
        offset.y = 0f;
        float range = Mathf.Max(0.8f, enemyData.stoppingDistance) + 0.15f + margin;
        return offset.sqrMagnitude <= range * range;
    }

    public void ApplyAnimatedMeleeHit()
    {
        if (locomotionAnimation == null || !IsInMeleeRange || Time.time < nextAttackTime) return;
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

        projectile.Initialize(bulletData, direction, firedByEnemy: true);

        if (bulletData.shootEffectPrefab != null && weaponView != null)
        {
            GameObject shootVfx = Instantiate(bulletData.shootEffectPrefab, weaponView.FirePosition, weaponView.FireRotation);
            ParticleSystem ps = shootVfx.GetComponent<ParticleSystem>();
            float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 1f;
            Destroy(shootVfx, Mathf.Max(lifetime, 0.5f));
        }
    }
}
