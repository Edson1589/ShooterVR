using UnityEngine;

[DisallowMultipleComponent]
public class PlayerDamageFeedback : MonoBehaviour
{
    private static readonly int DamageColorId = Shader.PropertyToID("_DamageColor");
    private static readonly int VignetteAmountId = Shader.PropertyToID("_VignetteAmount");
    private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");

    [Header("Referencias")]
    [Tooltip("Componente de salud del jugador. Si no se asigna, se busca automáticamente.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Tooltip("Cámara del casco VR. Si no se asigna, se busca Camera.main.")]
    [SerializeField] private Transform playerCamera;

    [Tooltip("Renderer del visor de daño en la cámara. Si no existe, se genera automáticamente.")]
    [SerializeField] private Renderer overlayRenderer;

    [Tooltip("Material con el shader Custom/VR/URP_VRDamageVignette.")]
    [SerializeField] private Material damageMaterial;

    [Header("Color e Intensidad")]
    [SerializeField] private Color damageColor = new Color(1.0f, 0.10f, 0.10f, 1.0f);
    [Range(0f, 1f)] [SerializeField] private float maxFlashAmount = 0.70f;
    [Range(0f, 1f)] [SerializeField] private float maxVignetteAmount = 1.0f;

    [Header("Velocidad de Recuperación")]
    [Tooltip("Velocidad con la que se desvanece el destello rojo total.")]
    [SerializeField] private float flashDecaySpeed = 3.2f;

    [Tooltip("Velocidad con la que se desvanece la viñeta perimetral.")]
    [SerializeField] private float vignetteDecaySpeed = 1.7f;

    [Header("Alerta de Salud Crítica")]
    [Tooltip("Porcentaje de vida (0-1) por debajo del cual pulsa la alerta continua.")]
    [Range(0.1f, 0.5f)] [SerializeField] private float lowHealthThreshold = 0.35f;

    [Tooltip("Frecuencia del pulso de latido en hercios.")]
    [SerializeField] private float heartbeatFrequency = 1.4f;

    [Range(0f, 1f)] [SerializeField] private float lowHealthMinVignette = 0.28f;
    [Range(0f, 1f)] [SerializeField] private float lowHealthMaxVignette = 0.75f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip damageSound;

    private MaterialPropertyBlock propBlock;
    private float currentFlash;
    private float currentVignette;

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();

        if (playerHealth == null)
        {
            playerHealth = GetComponentInParent<PlayerHealth>();
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }
        }

        if (playerCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                playerCamera = cam.transform;
            }
            else
            {
                Camera foundCam = GetComponentInChildren<Camera>();
                if (foundCam != null) playerCamera = foundCam.transform;
            }
        }

        EnsureOverlayObject();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken.AddListener(HandleDamageTaken);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnDamageTaken.RemoveListener(HandleDamageTaken);
        }
        ResetOverlay();
    }

    private void Start()
    {
        EnsureOverlayObject();
        UpdateShaderProperties(0f, 0f);
    }

    /// <summary>
    /// Crea o localiza el Quad del visor en la cámara VR si aún no está enlazado.
    /// </summary>
    public void EnsureOverlayObject()
    {
        if (overlayRenderer != null) return;

        if (playerCamera == null)
        {
            Camera cam = Camera.main;
            if (cam != null) playerCamera = cam.transform;
        }

        if (playerCamera == null) return;

        Transform existing = playerCamera.Find("DamageVisorOverlay");
        if (existing != null)
        {
            overlayRenderer = existing.GetComponent<Renderer>();
            return;
        }

        // Crear Quad visor a 0.22m frente a la cámara (cubre ampliamente el campo de visión de ambos ojos)
        GameObject overlayObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        overlayObj.name = "DamageVisorOverlay";
        overlayObj.transform.SetParent(playerCamera, false);
        overlayObj.transform.localPosition = new Vector3(0f, 0f, 0.22f);
        overlayObj.transform.localRotation = Quaternion.identity;
        overlayObj.transform.localScale = new Vector3(1.35f, 1.35f, 1.0f);

        // Eliminar collider para evitar cualquier interferencia física o raycast
        Collider col = overlayObj.GetComponent<Collider>();
        if (col != null) DestroyImmediate(col);

        overlayRenderer = overlayObj.GetComponent<Renderer>();
        if (damageMaterial != null)
        {
            overlayRenderer.sharedMaterial = damageMaterial;
        }
    }

    private void HandleDamageTaken(float damageAmount)
    {
        // Escala la intensidad según el daño recibido (mínimo 50% de intensidad)
        float severity = Mathf.Clamp01(damageAmount / 25f);
        float flashPeak = maxFlashAmount * Mathf.Lerp(0.55f, 1.0f, severity);
        float vignettePeak = maxVignetteAmount;

        currentFlash = Mathf.Max(currentFlash, flashPeak);
        currentVignette = Mathf.Max(currentVignette, vignettePeak);

        if (audioSource != null && damageSound != null)
        {
            audioSource.PlayOneShot(damageSound);
        }
    }

    private void Update()
    {
        // 1. Desvanecimiento suave del destello de impacto
        if (currentFlash > 0.001f)
        {
            currentFlash = Mathf.MoveTowards(currentFlash, 0f, flashDecaySpeed * Time.deltaTime);
        }
        else
        {
            currentFlash = 0f;
        }

        // 2. Cálculo del nivel objetivo de viñeta (normal vs salud crítica vs muerte)
        float targetVignette = 0f;

        if (playerHealth != null)
        {
            if (playerHealth.IsDead)
            {
                targetVignette = 1.0f;
            }
            else
            {
                float healthRatio = playerHealth.MaxHealth > 0f ? (playerHealth.CurrentHealth / playerHealth.MaxHealth) : 1f;
                if (healthRatio <= lowHealthThreshold)
                {
                    // Pulso rítmico simulando el latido cardíaco de peligro
                    float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * heartbeatFrequency * Mathf.PI * 2f);
                    targetVignette = Mathf.Lerp(lowHealthMinVignette, lowHealthMaxVignette, pulse);
                }
            }
        }

        // 3. Interpolar viñeta hacia su objetivo
        if (currentVignette > targetVignette)
        {
            currentVignette = Mathf.MoveTowards(currentVignette, targetVignette, vignetteDecaySpeed * Time.deltaTime);
        }
        else
        {
            currentVignette = Mathf.MoveTowards(currentVignette, targetVignette, 2.5f * Time.deltaTime);
        }

        UpdateShaderProperties(currentVignette, currentFlash);
    }

    private void UpdateShaderProperties(float vignette, float flash)
    {
        if (overlayRenderer == null) return;

        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        overlayRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(DamageColorId, damageColor);
        propBlock.SetFloat(VignetteAmountId, vignette);
        propBlock.SetFloat(FlashAmountId, flash);
        overlayRenderer.SetPropertyBlock(propBlock);

        bool shouldBeVisible = (vignette > 0.002f || flash > 0.002f);
        if (overlayRenderer.enabled != shouldBeVisible)
        {
            overlayRenderer.enabled = shouldBeVisible;
        }
    }

    private void ResetOverlay()
    {
        currentFlash = 0f;
        currentVignette = 0f;
        UpdateShaderProperties(0f, 0f);
    }
}
