using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR;

/// <summary>
/// Tracks the player's health and exposes a generic damage/heal API for other
/// systems (enemies, traps, pickups) to call. Does not handle death behavior
/// itself (Game Over, respawn, etc.) - that is left to whatever listens to
/// OnPlayerDied.
/// </summary>
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Serializable]
    public class HealthChangedEvent : UnityEvent<float> { }

    [Header("Health Settings")]
    [Tooltip("Maximum health value.")]
    [SerializeField] private float maxHealth = 100f;

    [Tooltip("After taking damage, incoming damage is ignored for this many seconds. Prevents a single hit (e.g. overlapping colliders) from being counted multiple times.")]
    [SerializeField] private float invulnerabilityDuration = 0.5f;

    [Header("Damage Feedback")]
    [Tooltip("Controller haptic pulse strength (0-1) when taking damage.")]
    [SerializeField] private float hapticAmplitude = 0.5f;

    [Tooltip("Controller haptic pulse duration in seconds when taking damage.")]
    [SerializeField] private float hapticDuration = 0.15f;

    [Header("Events")]
    [Tooltip("Invoked whenever health changes, passing the new current health.")]
    [SerializeField] private HealthChangedEvent onHealthChanged;

    [Tooltip("Invoked once when health reaches 0.")]
    [SerializeField] private UnityEvent onPlayerDied;

    private float currentHealth;
    private float invulnerableUntilTime;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Start()
    {
        onHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(float amount)
    {
        if (IsDead || amount <= 0f || Time.time < invulnerableUntilTime)
        {
            return;
        }

        invulnerableUntilTime = Time.time + invulnerabilityDuration;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        onHealthChanged?.Invoke(currentHealth);
        TriggerDamageHaptics();

        if (IsDead)
        {
            onPlayerDied?.Invoke();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        onHealthChanged?.Invoke(currentHealth);
    }

    private void TriggerDamageHaptics()
    {
        SendHapticImpulse(XRNode.LeftHand);
        SendHapticImpulse(XRNode.RightHand);
    }

    private void SendHapticImpulse(XRNode node)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (device.TryGetHapticCapabilities(out HapticCapabilities capabilities) && capabilities.supportsImpulse)
        {
            device.SendHapticImpulse(0u, hapticAmplitude, hapticDuration);
        }
    }

    [ContextMenu("Test: Take 10 Damage")]
    private void TestTakeDamage()
    {
        TakeDamage(10f);
    }

    [ContextMenu("Test: Heal 10")]
    private void TestHeal()
    {
        Heal(10f);
    }
}
