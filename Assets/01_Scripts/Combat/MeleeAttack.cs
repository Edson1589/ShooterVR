using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    [Header("Movimiento")]
    public Transform movementReference;
    public float minimumSpeed = 1.2f;
    public float resetSpeed = 0.4f;

    [Header("Daño")]
    public float damage = 15f;
    public float attackCooldown = 0.3f;
    
    [Header("Audio")]
    public AudioClip hitSound;

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

        if (movementReference == null) movementReference = player != null ? player.transform : transform.root;

        SphereCollider hitbox = GetComponent<SphereCollider>();
        hitbox.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnEnable()
    {
        ResetAttack();
        previousPosition = RelativePosition;
        CurrentSpeed = 0f;
        nextAttackTime = 0f;
    }

    private void FixedUpdate()
    {
        UpdateSpeed();

        if (CurrentSpeed <= resetSpeed)
        {
            ResetAttack();
            return;
        }

        if (!swingActive && CurrentSpeed >= minimumSpeed && Time.time >= nextAttackTime) swingActive = true;
    }

    private void UpdateSpeed()
    {
        Vector3 currentPosition = RelativePosition;
        CurrentSpeed = Vector3.Distance(currentPosition, previousPosition) / Time.fixedDeltaTime;
        previousPosition = currentPosition;
    }

    private void ResetAttack()
    {
        swingActive = false;
        hitEnemies.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    private void TryHit(Collider other)
    {
        if (!CanAttack() || other.isTrigger) return;

        Enemy enemy = other.GetComponentInParent<Enemy>();

        if (enemy == null || enemy.IsDead || !hitEnemies.Add(enemy)) return;

        enemy.TakeDamage(damage);
        OneShotAudio.Play(hitSound, transform.position, 0.65f);

        nextAttackTime = Time.time + attackCooldown;
    }

    private bool CanAttack()
    {
        if (!isActiveAndEnabled) return false;
        if (!swingActive || CurrentSpeed < minimumSpeed) return false;
        if (player != null && player.IsDead) return false;

        return true;
    }

    private void OnValidate()
    {
        minimumSpeed = Mathf.Max(0.1f, minimumSpeed);
        resetSpeed = Mathf.Clamp(resetSpeed, 0f, minimumSpeed * 0.9f);
        damage = Mathf.Max(0f, damage);
        attackCooldown = Mathf.Max(0f, attackCooldown);
    }
}