using System;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Serializable]
    public class HealthChangedEvent : UnityEvent<float> { }

    [Serializable]
    public class DamageTakenEvent : UnityEvent<float> { }

    [Header("Configuración de Vida")]
    public float maxHealth = 100f;

    public float invulnerabilityDuration = 0.5f;

    [Header("Audio")]
    public AudioClip lowHealthSound;
    [Range(0f, 1f)] public float lowHealthThreshold = 0.25f;

    [Header("Eventos")]
    public HealthChangedEvent onHealthChanged = new HealthChangedEvent();
    public DamageTakenEvent onDamageTaken = new DamageTakenEvent();
    public UnityEvent onPlayerDied = new UnityEvent();

    private float currentHealth;
    private float invulnerableUntilTime;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    public HealthChangedEvent OnHealthChanged => onHealthChanged;
    public DamageTakenEvent OnDamageTaken => onDamageTaken;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        onHealthChanged.Invoke(currentHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f || IsInvulnerable()) return;

        invulnerableUntilTime = Time.time + invulnerabilityDuration;

        bool wasLow = currentHealth <= maxHealth * lowHealthThreshold;
        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        if (!IsDead)
        {
            if (!wasLow && currentHealth <= maxHealth * lowHealthThreshold) OneShotAudio.Play(lowHealthSound, transform.position, 0.5f, false);
        }

        onHealthChanged.Invoke(currentHealth);
        onDamageTaken.Invoke(amount);

        if (IsDead) onPlayerDied.Invoke();
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        onHealthChanged.Invoke(currentHealth);
    }

    private bool IsInvulnerable()
    {
        return Time.time < invulnerableUntilTime;
    }
}
