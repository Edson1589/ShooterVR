using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody rb;
    private BulletData bulletData;
    private bool initialized;

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

        bulletData = data;
        initialized = true;

        // Normalizar para que la velocidad dependa unicamente del valor configurado en BulletData.
        rb.linearVelocity = direction.normalized * bulletData.speed;

        Destroy(gameObject, bulletData.lifetime);
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

        Destroy(gameObject);
    }
}