using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private BulletData bulletData;
    private bool initialized;
    private bool firedByEnemy;
    private GameObject activeTrail;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ConfigureEnemyProjectileCollisions()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int projectileLayer = LayerMask.NameToLayer("EnemyProjectile");
        if (enemyLayer >= 0 && projectileLayer >= 0) Physics.IgnoreLayerCollision(enemyLayer, projectileLayer, true);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public void Initialize(BulletData data, Vector3 direction, bool firedByEnemy = false)
    {
        if (data == null)
        {
            Destroy(gameObject);
            return;
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        bulletData = data;
        this.firedByEnemy = firedByEnemy;
        if (firedByEnemy)
        {
            int layer = LayerMask.NameToLayer("EnemyProjectile");
            if (layer >= 0)
            {
                gameObject.layer = layer;
                foreach (Collider projectileCollider in GetComponentsInChildren<Collider>(true))
                    projectileCollider.gameObject.layer = layer;
            }
        }
        initialized = true;

        Vector3 moveDir = direction.normalized;
        if (moveDir != Vector3.zero)
        {
            transform.forward = moveDir;
        }
        rb.linearVelocity = moveDir * bulletData.speed;
        if (bulletData.trailEffectPrefab != null)
        {
            activeTrail = Instantiate(bulletData.trailEffectPrefab, transform.position, transform.rotation, transform);
            activeTrail.transform.localPosition = new Vector3(0f, 0f, -0.4f);
            activeTrail.transform.localRotation = Quaternion.identity;
        }

        Invoke(nameof(OnLifetimeExpired), bulletData.lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!initialized) return;
        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();
        if (firedByEnemy)
        {
            damageable = collision.collider.GetComponentInParent<PlayerHealth>();
        }

        if (damageable != null)
        {
            damageable.TakeDamage(bulletData.damage);
        }

        SpawnImpactEffect(collision);
        AudioClip impact = collision.collider.GetComponentInParent<Enemy>() != null ? bulletData.enemyImpactSound : collision.collider.GetComponentInParent<PlayerHealth>() == null ? bulletData.surfaceImpactSound : null;
        OneShotAudio.Play(impact, collision.contactCount > 0 ? collision.GetContact(0).point : transform.position, bulletData.impactVolume);

        CancelInvoke(nameof(OnLifetimeExpired));
        DestroyProjectile();
    }

    private void SpawnImpactEffect(Collision collision)
    {
        if (bulletData == null || bulletData.impactEffectPrefab == null) return;

        Vector3 hitPoint = transform.position;
        Quaternion hitRotation = transform.forward != Vector3.zero  ? Quaternion.LookRotation(-transform.forward)  : Quaternion.identity;

        if (collision.contactCount > 0)
        {
            ContactPoint contact = collision.GetContact(0);
            hitPoint = contact.point;
            if (contact.normal != Vector3.zero)
            {
                hitRotation = Quaternion.LookRotation(contact.normal);
            }
        }

        GameObject impactVfx = Instantiate(bulletData.impactEffectPrefab, hitPoint, hitRotation);
        ParticleSystem ps = impactVfx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 1.5f;
        Destroy(impactVfx, Mathf.Max(lifetime, 0.5f));
    }

    private void OnLifetimeExpired()
    {
        DestroyProjectile();
    }

    private void DestroyProjectile()
    {
        DetachTrail();
        Destroy(gameObject);
    }

    private void DetachTrail()
    {
        if (activeTrail != null)
        {
            activeTrail.transform.SetParent(null, true);

            if (activeTrail.TryGetComponent(out TrailRenderer tr))
            {
                tr.emitting = false;
                Destroy(activeTrail, tr.time);
            }
            else if (activeTrail.TryGetComponent(out ParticleSystem ps))
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                Destroy(activeTrail, ps.main.startLifetime.constantMax);
            }
            else
            {
                Destroy(activeTrail, 1f);
            }

            activeTrail = null;
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(OnLifetimeExpired));
        DetachTrail();
    }
}
