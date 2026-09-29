using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("Vibración de Controles")]
    public float hapticAmplitude = 0.5f;
    public float hapticDuration = 0.15f;

    [Header("Efecto Visual de Daño")]
    public Renderer damageOverlayRenderer;
    public Color damageFlashColor = new Color(1f, 0.08f, 0.08f, 1f);
    public float flashDuration = 0.35f;

    private PlayerHealth playerHealth;
    private Coroutine flashCoroutine;
    private MaterialPropertyBlock propertyBlock;

    private static readonly int DamageColorId = Shader.PropertyToID("_DamageColor");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
    private static readonly int VignetteAmountId = Shader.PropertyToID("_VignetteAmount");

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        propertyBlock = new MaterialPropertyBlock();

        FindDamageOverlay();

        if (damageOverlayRenderer != null) damageOverlayRenderer.enabled = false;
    }

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnDamageTaken.AddListener(PlayDamageFeedback);
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnDamageTaken.RemoveListener(PlayDamageFeedback);
    }

    private void PlayDamageFeedback(float damage)
    {
        TriggerHaptics();
        TriggerVisualEffect();
    }

    private void TriggerVisualEffect()
    {
        if (damageOverlayRenderer == null) return;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(DamageFlashRoutine());
    }

    private IEnumerator DamageFlashRoutine()
    {
        damageOverlayRenderer.enabled = true;

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;

            float progress = elapsed / flashDuration;
            float intensity = 1f - Mathf.Clamp01(progress);
            float flash = Mathf.SmoothStep(0f, 1f, intensity);

            UpdateOverlay(flash);

            yield return null;
        }

        damageOverlayRenderer.enabled = false;
        flashCoroutine = null;
    }

    private void UpdateOverlay(float intensity)
    {
        damageOverlayRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetColor(DamageColorId, damageFlashColor);
        propertyBlock.SetFloat(FlashAmountId, intensity * 0.75f);
        propertyBlock.SetFloat(VignetteAmountId, intensity);

        damageOverlayRenderer.SetPropertyBlock(propertyBlock);
    }

    private void FindDamageOverlay()
    {
        if (damageOverlayRenderer != null) return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null) return;

        Transform overlay = mainCamera.transform.Find("DamageVisorOverlay");

        if (overlay != null) damageOverlayRenderer = overlay.GetComponent<Renderer>();
    }

    private void TriggerHaptics()
    {
        SendHapticImpulse(XRNode.LeftHand);
        SendHapticImpulse(XRNode.RightHand);
    }

    private void SendHapticImpulse(XRNode node)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);

        if (!device.TryGetHapticCapabilities(out HapticCapabilities capabilities)) return;

        if (!capabilities.supportsImpulse) return;

        device.SendHapticImpulse(0, hapticAmplitude, hapticDuration);
    }
}