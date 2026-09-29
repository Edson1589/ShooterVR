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
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || IsResolved || amount <= 0f) return;
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        Damaged?.Invoke(amount);
        if (IsDead)
        {
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
