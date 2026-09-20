using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private BulletData bulletData;
    private bool initialized;
    private GameObject activeTrail;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Configura y lanza el proyectil.
    public void Initialize(BulletData data, Vector3 direction)
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
        initialized = true;

        Vector3 moveDir = direction.normalized;
        if (moveDir != Vector3.zero)
        {
            transform.forward = moveDir;
        }

        // Normalizar para que la velocidad dependa unicamente del valor configurado en BulletData.
        rb.linearVelocity = moveDir * bulletData.speed;

        // Instanciar el efecto de estela / cola si está configurado en el ScriptableObject
        if (bulletData.trailEffectPrefab != null)
        {
            activeTrail = Instantiate(bulletData.trailEffectPrefab, transform.position, transform.rotation, transform);
            // Colocar en la parte posterior de la bala (cola)
            activeTrail.transform.localPosition = new Vector3(0f, 0f, -0.4f);
            activeTrail.transform.localRotation = Quaternion.identity;
        }

        Invoke(nameof(OnLifetimeExpired), bulletData.lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!initialized) return;

        // El collider puede estar en un hijo mientras que el componente que recibe daño se encuentra en el objeto padre.
        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(bulletData.damage);
        }

        SpawnImpactEffect(collision);

        CancelInvoke(nameof(OnLifetimeExpired));
        DestroyProjectile();
    }

    private void SpawnImpactEffect(Collision collision)
    {
        if (bulletData == null || bulletData.impactEffectPrefab == null) return;

        Vector3 hitPoint = transform.position;
        Quaternion hitRotation = transform.forward != Vector3.zero 
            ? Quaternion.LookRotation(-transform.forward) 
            : Quaternion.identity;

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