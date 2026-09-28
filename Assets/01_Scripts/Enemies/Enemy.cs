using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour, IDamageable
{
    public static event Action<EnemyData> OnEnemyDied;
    public event Action<Enemy> Resolved;

    [Header("Configuración")]
    public EnemyData enemyData;

    [Header("Arma del tirador")]
    public WeaponView weaponView;

    [Header("Apoyo del modelo del tirador")]
    [Tooltip("Distancia vertical desde el hueso del pie hasta la suela, en unidades del enemigo.")]
    [Min(0f)] [SerializeField] private float footSoleOffset = 0.08f;

    private Rigidbody rb;
    private PlayerHealth player;
    private Collider playerCollider;
    private Collider[] enemyColliders;
    private Vector3 movementDirection;
    private float currentHealth;
    private float nextAttackTime;
    private Transform shooterVisual;
    private Transform shooterHead;
    private Transform shooterHand;
    private Transform shooterLeftFoot;
    private Transform shooterRightFoot;
    private CapsuleCollider bodyCollider;
    private Vector3 visualRestPosition;
    private Quaternion visualRestRotation;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsResolved { get; private set; }

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
        if (enemyData.enemyType == EnemyType.Shooter && weaponView == null)
        {
            weaponView = GetComponentInChildren<WeaponView>();
        }
        if (enemyData.enemyType == EnemyType.Shooter)
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null && animator.isHuman && animator.avatar != null && animator.avatar.isValid)
            {
                shooterVisual = animator.transform != transform ? animator.transform : null;
                shooterHead = animator.GetBoneTransform(HumanBodyBones.Head);
                shooterHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                shooterLeftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                shooterRightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                bodyCollider = GetComponent<CapsuleCollider>();
                if (shooterVisual != null)
                {
                    visualRestRotation = shooterVisual.localRotation;
                    visualRestPosition = shooterVisual.localPosition;
                }
            }
        }
    }

    private void Start()
    {
        movementDirection = transform.forward;
        movementDirection.y = 0f;
        movementDirection.Normalize();

        player = FindAnyObjectByType<PlayerHealth>();
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (enemyData.enemyType == EnemyType.Shooter)
            {
                Vector3 facing = TargetPosition - transform.position;
                facing.y = 0f;
                if (facing.sqrMagnitude > 0.001f)
                {
                    Quaternion rotation = Quaternion.LookRotation(facing);
                    rb.rotation = rotation;
                    transform.rotation = rotation;
                }
            }
        }

        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void OnDisable()
    {
        NotifyResolved();
    }

    private void NotifyResolved()
    {
        if (IsResolved) return;
        IsResolved = true;
        Resolved?.Invoke(this);
    }

    public void Resolve()
    {
        if (IsResolved) return;
        NotifyResolved();
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        if (IsDead || IsResolved) return;

        if (enemyData.enemyType == EnemyType.Normal)
        {
            rb.MovePosition(rb.position + movementDirection * enemyData.moveSpeed * Time.fixedDeltaTime);
            return;
        }

        if (player == null || player.IsDead) return;

        Vector3 direction = TargetPosition - rb.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            rb.MoveRotation(Quaternion.LookRotation(direction));
        }

        if (enemyData.enemyType != EnemyType.Kamikaze) return;

        float distance = direction.magnitude;
        float step = Mathf.Min(enemyData.moveSpeed * Time.fixedDeltaTime, Mathf.Max(0f, distance - enemyData.stoppingDistance));
        rb.MovePosition(rb.position + direction.normalized * step);
    }

    private void LateUpdate()
    {
        if (IsDead || IsResolved) return;
        if (enemyData.enemyType != EnemyType.Shooter) return;

        GroundShooterVisual();
        if (player == null || player.IsDead) return;
        AlignShooterVisual();
        if (weaponView == null || !weaponView.HasFirePoint) return;

        Vector3 direction = TargetPosition - weaponView.FirePosition;
        if (direction.sqrMagnitude < 0.001f) return;

        // Rotate the gripping hand with its weapon instead of twisting the gun out of the grip.
        Transform aimPivot = shooterHand != null && weaponView.transform.IsChildOf(shooterHand)
            ? shooterHand : weaponView.transform;
        aimPivot.rotation = Quaternion.FromToRotation(weaponView.FireDirection, direction) * aimPivot.rotation;
        if (Time.time < nextAttackTime) return;

        FireProjectile();
        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    private void GroundShooterVisual()
    {
        if (shooterVisual == null || bodyCollider == null || !bodyCollider.enabled
            || shooterLeftFoot == null || shooterRightFoot == null) return;

        // Use the animated feet rather than mesh bounds to place the lower sole
        // on the capsule base, without moving the physics body or accumulating offsets.
        shooterVisual.localPosition = visualRestPosition;
        float soleY = Mathf.Min(shooterLeftFoot.position.y, shooterRightFoot.position.y)
            - footSoleOffset * Mathf.Abs(transform.lossyScale.y);
        shooterVisual.position += Vector3.up * (bodyCollider.bounds.min.y - soleY);
    }

    private void AlignShooterVisual()
    {
        if (shooterHead == null) return;
        if (shooterVisual != null)
        {
            // The clip turns the torso independently of the physics root. Correct its visual yaw
            // from the current animated pose without accumulating rotation between frames.
            shooterVisual.localRotation = visualRestRotation;
            Vector3 forward = Vector3.ProjectOnPlane(shooterHead.forward, Vector3.up);
            Vector3 target = Vector3.ProjectOnPlane(TargetPosition - shooterHead.position, Vector3.up);
            if (forward.sqrMagnitude > 0.001f && target.sqrMagnitude > 0.001f)
                shooterVisual.rotation = Quaternion.FromToRotation(forward, target) * shooterVisual.rotation;
        }

        Vector3 lookDirection = TargetPosition - shooterHead.position;
        if (lookDirection.sqrMagnitude > 0.001f)
            shooterHead.rotation = Quaternion.FromToRotation(shooterHead.forward, lookDirection) * shooterHead.rotation;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger) return;
        if (IsDead || IsResolved || player == null || player.IsDead) return;
        if (enemyData.enemyType != EnemyType.Normal && enemyData.enemyType != EnemyType.Kamikaze) return;
        if (Time.time < nextAttackTime) return;
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
        if (IsDead || IsResolved || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (IsDead)
        {
            Resolve();
            OnEnemyDied?.Invoke(enemyData);
        }
    }
}
