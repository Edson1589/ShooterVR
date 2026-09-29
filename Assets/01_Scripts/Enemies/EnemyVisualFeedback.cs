using System.Collections;
using UnityEngine;

public class EnemyVisualFeedback : MonoBehaviour
{
    private Enemy enemy;
    private EnemyData enemyData => enemy.enemyData;
    private Renderer[] enemyRenderers;
    private MaterialPropertyBlock propBlock;
    private Coroutine flashCoroutine;
    private Color[] originalBaseColors;
    private Texture[] originalBaseMaps;
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyRenderers = GetComponentsInChildren<Renderer>(true);
        propBlock = new MaterialPropertyBlock();
        CacheOriginalColors();
    }

    private void OnEnable()
    {
        enemy.Damaged += HandleDamaged;
        enemy.Resolving += PlayDeathEffect;
    }

    private void Start()
    {
        if (enemy.isActiveAndEnabled && !enemy.IsResolved) PlaySpawnEffect();
    }

    private void OnDisable()
    {
        enemy.Damaged -= HandleDamaged;
        enemy.Resolving -= PlayDeathEffect;
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        ResetRenderersColor();
    }

    private void HandleDamaged(float amount)
    {
        PlayHitEffect();
        TriggerHitFlash();
    }

    private void PlaySpawnEffect()
    {
        if (enemyData == null || enemyData.spawnEffectPrefab == null) return;
        GameObject fx = Instantiate(enemyData.spawnEffectPrefab, transform.position, transform.rotation);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2.5f;
        Destroy(fx, Mathf.Max(lifetime, 1f));
    }

    private void PlayHitEffect()
    {
        if (enemyData == null || enemyData.hitEffectPrefab == null) return;
        Vector3 hitPosition = transform.position + Vector3.up * 0.9f;
        GameObject fx = Instantiate(enemyData.hitEffectPrefab, hitPosition, Quaternion.identity);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 1f;
        Destroy(fx, Mathf.Max(lifetime, 0.5f));
    }

    private void PlayDeathEffect()
    {
        if (enemyData == null || enemyData.deathEffectPrefab == null) return;
        Vector3 deathPos = transform.position + Vector3.up * 0.4f;
        GameObject fx = Instantiate(enemyData.deathEffectPrefab, deathPos, Quaternion.identity);
        ParticleSystem ps = fx.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 2.5f;
        Destroy(fx, Mathf.Max(lifetime, 1.5f));
    }

    private void TriggerHitFlash()
    {
        if (!gameObject.activeInHierarchy || enemyRenderers == null || enemyRenderers.Length == 0) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private void CacheOriginalColors()
    {
        if (enemyRenderers == null) return;
        originalBaseColors = new Color[enemyRenderers.Length];
        originalBaseMaps = new Texture[enemyRenderers.Length];
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer r = enemyRenderers[i];
            if (r != null && r.sharedMaterial != null)
            {
                originalBaseColors[i] = r.sharedMaterial.HasProperty(BaseColorId) ? r.sharedMaterial.GetColor(BaseColorId) : Color.white;
                originalBaseMaps[i] = r.sharedMaterial.HasProperty(BaseMapId) ? r.sharedMaterial.GetTexture(BaseMapId) : (r.sharedMaterial.mainTexture != null ? r.sharedMaterial.mainTexture : null);
            }
            else
            {
                originalBaseColors[i] = Color.white;
                originalBaseMaps[i] = null;
            }
        }
    }

    private IEnumerator HitFlashRoutine()
    {
        Color signatureColor = GetEnemySignatureColor();
        Color flashPaintColor = Color.Lerp(signatureColor, Color.white, 0.45f);
        Color flashEmissionColor = signatureColor * 5.0f + Color.white * 2.5f;

        if (propBlock == null) propBlock = new MaterialPropertyBlock();

        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer r = enemyRenderers[i];
            if (r == null || !r.enabled) continue;
            r.GetPropertyBlock(propBlock);
            propBlock.SetTexture(BaseMapId, Texture2D.whiteTexture);
            propBlock.SetColor(BaseColorId, flashPaintColor);
            propBlock.SetColor(EmissionColorId, flashEmissionColor);
            r.SetPropertyBlock(propBlock);
        }

        yield return new WaitForSeconds(0.08f);

        float elapsed = 0f;
        float duration = 0.14f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Color currentPaint = Color.Lerp(flashPaintColor, signatureColor, t);
            Color currentEmission = Color.Lerp(flashEmissionColor, Color.black, t);

            for (int i = 0; i < enemyRenderers.Length; i++)
            {
                Renderer r = enemyRenderers[i];
                if (r == null || !r.enabled) continue;
                r.GetPropertyBlock(propBlock);
                propBlock.SetColor(BaseColorId, currentPaint);
                propBlock.SetColor(EmissionColorId, currentEmission);
                r.SetPropertyBlock(propBlock);
            }

            yield return null;
        }

        ResetRenderersColor();
        flashCoroutine = null;
    }

    private void ResetRenderersColor()
    {
        if (enemyRenderers == null || propBlock == null) return;
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            Renderer r = enemyRenderers[i];
            if (r == null) continue;
            r.GetPropertyBlock(propBlock);
            Color original = (originalBaseColors != null && i < originalBaseColors.Length) ? originalBaseColors[i] : Color.white;
            Texture originalTex = (originalBaseMaps != null && i < originalBaseMaps.Length) ? originalBaseMaps[i] : null;

            if (originalTex != null)
                propBlock.SetTexture(BaseMapId, originalTex);
            else
                propBlock.SetTexture(BaseMapId, Texture2D.whiteTexture);

            propBlock.SetColor(BaseColorId, original);
            propBlock.SetColor(EmissionColorId, Color.black);
            r.SetPropertyBlock(propBlock);
        }
    }

    private Color GetEnemySignatureColor()
    {
        if (enemyData == null) return Color.white;
        switch (enemyData.enemyType)
        {
            case EnemyType.Kamikaze:
                return new Color(0.85f, 0.15f, 1.0f, 1.0f);
            case EnemyType.Normal:
                return new Color(1.0f, 0.15f, 0.15f, 1.0f);
            case EnemyType.Shooter:
                return new Color(0.15f, 0.65f, 1.0f, 1.0f);
            default:
                return Color.white;
        }
    }
}
