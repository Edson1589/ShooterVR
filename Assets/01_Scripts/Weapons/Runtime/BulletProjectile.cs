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

    //Configura el proyectil antes de ser disparado.
    public void Initialize(BulletData data, Vector3 direction)
    {
        bulletData = data;
        initialized = true;

        // Velocidad normalizada para que BulletData controle completamente la velocidad del proyectil.
        rb.linearVelocity = direction.normalized * bulletData.speed;

        Destroy(gameObject, bulletData.lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!initialized) return;

        // Buscar tambien en los padres porque el collider golpeado puede pertenecer a un hijo del objeto que recibe daño.
        IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(bulletData.damage);
        }

        Destroy(gameObject);
    }
}
