using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class MeleeAttack : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Raíz del rig: su desplazamiento y giro no cuentan como un golpe.")]
    [SerializeField] private Transform movementReference;
    [Min(0.1f)] [SerializeField] private float minimumSpeed = 1.2f;
    [Min(0f)] [SerializeField] private float resetSpeed = 0.4f;

    [Header("Daño")]
    [Min(0f)] [SerializeField] private float damage = 15f;
    [Min(0f)] [SerializeField] private float attackCooldown = 0.3f;

    private readonly HashSet<Enemy> hitEnemies = new HashSet<Enemy>();
    private PlayerHealth player;
    private Vector3 previousPosition;
    private bool swingActive;
    private float nextAttackTime;

    public float CurrentSpeed { get; private set; }

    private Vector3 RelativePosition => movementReference.InverseTransformPoint(transform.position);

    private void Awake()
    {
        player = GetComponentInParent<PlayerHealth>();
        if (movementReference == null)
        {
            movementReference = player != null ? player.transform : transform.root;
        }

        GetComponent<SphereCollider>().isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnEnable()
    {
        previousPosition = RelativePosition;
        CurrentSpeed = 0f;
        swingActive = false;
        nextAttackTime = 0f;
        hitEnemies.Clear();
    }

    private void FixedUpdate()
    {
        Vector3 position = RelativePosition;
        CurrentSpeed = (position - previousPosition).magnitude / Time.fixedDeltaTime;
        previousPosition = position;

        // Frenar la mano termina el gesto y permite volver a golpear al enemigo.
        if (CurrentSpeed <= resetSpeed)
        {
            swingActive = false;
            hitEnemies.Clear();
        }
        else if (!swingActive && CurrentSpeed >= minimumSpeed && Time.time >= nextAttackTime)
        {
            swingActive = true;
        }
    }

    private void OnTriggerEnter(Collider other) => TryHit(other);

    private void OnTriggerStay(Collider other) => TryHit(other);

    private void TryHit(Collider other)
    {
        if (!isActiveAndEnabled || !swingActive || CurrentSpeed < minimumSpeed) return;
        if (player != null && player.IsDead) return;
        if (other.isTrigger) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null || enemy.IsDead || !hitEnemies.Add(enemy)) return;

        enemy.TakeDamage(damage);
        nextAttackTime = Time.time + attackCooldown;
    }

    private void OnValidate()
    {
        minimumSpeed = Mathf.Max(0.1f, minimumSpeed);
        resetSpeed = Mathf.Clamp(resetSpeed, 0f, minimumSpeed * 0.9f);
        damage = Mathf.Max(0f, damage);
        attackCooldown = Mathf.Max(0f, attackCooldown);
    }
}
