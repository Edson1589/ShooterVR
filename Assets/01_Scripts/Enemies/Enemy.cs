using System;
using System.Collections;
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
    private EnemyLocomotionAnimation locomotionAnimation;
    private Renderer[] enemyRenderers;
    private MaterialPropertyBlock propBlock;
    private Coroutine flashCoroutine;
    private Color[] originalBaseColors;
    private Texture[] originalBaseMaps;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsResolved { get; private set; }

    private Vector3 TargetPosition => playerCollider != null ? playerCollider.bounds.center : player.transform.position + Vector3.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyColliders = GetComponentsInChildren<Collider>(true);
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0)
        {
            gameObject.layer = enemyLayer;
            foreach (Collider enemyCollider in enemyColliders)
                enemyCollider.gameObject.layer = enemyLayer;
        }

        if (enemyData == null)
        {
            enabled = false;
            return;
        }

        currentHealth = enemyData.maxHealth;
        locomotionAnimation = GetComponentInChildren<EnemyLocomotionAnimation>();
        enemyRenderers = GetComponentsInChildren<Renderer>(true);
        propBlock = new MaterialPropertyBlock();
        CacheOriginalColors();
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
        PlaySpawnEffect();
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        ResetRenderersColor();
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
        PlayDeathEffect();
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
        if (locomotionAnimation != null && (locomotionAnimation.IsAttacking || IsInMeleeRange)) return;

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
        if (enemyData.enemyType == EnemyType.Kamikaze && locomotionAnimation != null) return;
        if (Time.time < nextAttackTime) return;
        if (other.GetComponentInParent<PlayerHealth>() != player) return;

        player.TakeDamage(enemyData.contactDamage);
        nextAttackTime = Time.time + enemyData.attackCooldown;
    }

    public bool IsInMeleeRange => IsWithinMeleeRange(0f);
    public bool CanContinueMeleeAttack => IsWithinMeleeRange(0.2f);

    private bool IsWithinMeleeRange(float margin)
    {
        if (!isActiveAndEnabled || IsDead || IsResolved || player == null || player.IsDead
            || enemyData.enemyType != EnemyType.Kamikaze) return false;
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

    public void TakeDamage(float amount)
    {
        if (IsDead || IsResolved || amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        PlayHitEffect();
        TriggerHitFlash();

        if (IsDead)
        {
            Resolve();
            OnEnemyDied?.Invoke(enemyData);
        }
    }

    private void PlaySpawnEffect()
    {
        if (enemyData == null || enemyData.spawnEffectPrefab == null) return;
        GameObject fx = Instantiate(enemyData.spawnEffectPrefab, transform.position, transform.rotation);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2.5f;
        Destroy(fx, Mathf.Max(lifetime, 1f));
    }

    private void PlayHitEffect()
    {
        if (enemyData == null || enemyData.hitEffectPrefab == null) return;
        Vector3 hitPosition = transform.position + Vector3.up * 0.9f;
        GameObject fx = Instantiate(enemyData.hitEffectPrefab, hitPosition, Quaternion.identity);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 1f;
        Destroy(fx, Mathf.Max(lifetime, 0.5f));
    }

    private void PlayDeathEffect()
    {
        if (enemyData == null || enemyData.deathEffectPrefab == null) return;
        Vector3 deathPos = transform.position + Vector3.up * 0.4f;
        GameObject fx = Instantiate(enemyData.deathEffectPrefab, deathPos, Quaternion.identity);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2.5f;
        Destroy(fx, Mathf.Max(lifetime, 1.5f));
    }

    private void TriggerHitFlash()
    {
        if (!gameObject.activeInHierarchy || enemyRenderers == null || enemyRenderers.Length == 0) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private void CacheOriginalColors()
    {
        if (enemyRenderers == null) return;
        originalBaseColors = new Color[enemyRenderers.Length];
        originalBaseMaps = new Texture[enemyRenderers.Length];
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer r = enemyRenderers[i];
            if (r != null && r.sharedMaterial != null)
            {
                originalBaseColors[i] = r.sharedMaterial.HasProperty(BaseColorId)
                    ? r.sharedMaterial.GetColor(BaseColorId)
                    : Color.white;
                originalBaseMaps[i] = r.sharedMaterial.HasProperty(BaseMapId)
                    ? r.sharedMaterial.GetTexture(BaseMapId)
                    : (r.sharedMaterial.mainTexture != null ? r.sharedMaterial.mainTexture : null);
            }
            else
            {
                originalBaseColors[i] = Color.white;
                originalBaseMaps[i] = null;
            }
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        Color signatureColor = GetEnemySignatureColor();
        Color flashPaintColor = Color.Lerp(signatureColor, Color.white, 0.45f);
        Color flashEmissionColor = signatureColor * 5.0f + Color.white * 2.5f;

        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer r = enemyRenderers[i];
            if (r == null || !r.enabled) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetTexture(BaseMapId, Texture2D.whiteTexture);
            propBlock.SetColor(BaseColorId, flashPaintColor);
            propBlock.SetColor(EmissionColorId, flashEmissionColor);
            r.SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(0.08f);

        float elapsed = 0f;
        float duration = 0.14f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Color currentPaint = Color.Lerp(flashPaintColor, signatureColor, t);
            Color currentEmission = Color.Lerp(flashEmissionColor, Color.black, t);

            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                Renderer r = enemyRenderers[i];
                if (r == null || !r.enabled) continue;
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor(BaseColorId, currentPaint);
                propBlock.SetColor(EmissionColorId, currentEmission);
                r.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        ResetRenderersColor();
        flashCoroutine = null;
    }

    private void ResetRenderersColor()
    {
        if (enemyRenderers == null || propBlock == null) return;
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer r = enemyRenderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            Color original = (originalBaseColors != null && i < originalBaseColors.Length) ? originalBaseColors[i] : Color.white;
            Texture originalTex = (originalBaseMaps != null && i < originalBaseMaps.Length) ? originalBaseMaps[i] : null;

            if (originalTex != null)
                propBlock.SetTexture(BaseMapId, originalTex);
            else
                propBlock.SetTexture(BaseMapId, Texture2D.whiteTexture);

            propBlock.SetColor(BaseColorId, original);
            propBlock.SetColor(EmissionColorId, Color.black);
            r.SetPropertyBlock(propBlock);
        }
    }

    private Color GetEnemySignatureColor()
    {
        if (enemyData == null) return Color.white;
        switch (enemyData.enemyType)
        {
            case EnemyType.Kamikaze:
                return new Color(0.85f, 0.15f, 1.0f, 1.0f);
            case EnemyType.Normal:
                return new Color(1.0f, 0.15f, 0.15f, 1.0f);
            case EnemyType.Shooter:
                return new Color(0.15f, 0.65f, 1.0f, 1.0f);
            default:
                return Color.white;
        }
    }
}
