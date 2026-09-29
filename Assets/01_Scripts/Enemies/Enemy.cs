using System;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    public static event Action<EnemyData> OnEnemyDied;
    public event Action<Enemy> Resolved;
    public event Action<float> Damaged;
    public event Action Resolving;

    [Header("Configuración")]
    public EnemyData enemyData;

    private float currentHealth;
    private Vector3 lastStepPosition;
    private float stepTravel;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsResolved { get; private set; }

    private void Awake()
    {
        if (enemyData == null)
        {
            enabled = false;
            return;
        }
        currentHealth = enemyData.maxHealth;
        lastStepPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 movement = Vector3.ProjectOnPlane(transform.position - lastStepPosition, Vector3.up);
        lastStepPosition = transform.position;
        if (Time.timeScale == 0f || IsDead || IsResolved || enemyData == null) return;
        if (movement.magnitude > 1f) { stepTravel = 0f; return; }
        stepTravel += movement.magnitude;
        if (stepTravel < Mathf.Max(0.1f, enemyData.stepDistance)) return;
        stepTravel = 0f;
        OneShotAudio.Play(enemyData.footstepSound, transform.position, enemyData.soundVolume * 0.45f);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || IsResolved || amount <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Damaged?.Invoke(amount);
        if (IsDead)
        {
            OneShotAudio.Play(enemyData.deathSound, transform.position, enemyData.soundVolume);
            Resolve();
            OnEnemyDied?.Invoke(enemyData);
        }
    }

    public void Resolve()
    {
        if (IsResolved) return;
        IsResolved = true;
        Resolving?.Invoke();
        Resolved?.Invoke(this);
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        if (IsResolved) return;
        IsResolved = true;
        Resolved?.Invoke(this);
    }
}
