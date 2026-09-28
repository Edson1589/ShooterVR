using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIHoloPulse : MonoBehaviour
{
    private static readonly int UnscaledTimeId = Shader.PropertyToID("_UnscaledTime");

    [Header("Material Holográfico")]
    [Tooltip("Imágenes con material URP_HoloCanvasUI para actualizar _UnscaledTime.")]
    [SerializeField] private Graphic[] holoGraphics;

    [Header("Borde Pulsante")]
    [Tooltip("Borde glowing opcional que pulsa suavemente.")]
    [SerializeField] private Graphic pulsingBorder;
    [SerializeField] private float pulseFrequency = 1.2f;
    [Range(0f, 1f)] [SerializeField] private float minAlpha = 0.55f;
    [Range(0f, 1f)] [SerializeField] private float maxAlpha = 0.95f;

    private MaterialPropertyBlock propBlock;
    private Color borderBaseColor;
    private Material[] instancedMaterials;

    private void Awake()
    {
        if (pulsingBorder != null)
        {
            borderBaseColor = pulsingBorder.color;
        }

        if (holoGraphics != null && holoGraphics.Length > 0)
        {
            instancedMaterials = new Material[holoGraphics.Length];
            for (int i = 0; i < holoGraphics.Length; i++)
            {
                if (holoGraphics[i] != null && holoGraphics[i].material != null)
                {
                    instancedMaterials[i] = new Material(holoGraphics[i].material);
                    holoGraphics[i].material = instancedMaterials[i];
                }
            }
        }
    }

    private void Update()
    {
        float time = Time.unscaledTime;

        if (instancedMaterials != null)
        {
            for (int i = 0; i < instancedMaterials.Length; i++)
            {
                if (instancedMaterials[i] != null)
                {
                    instancedMaterials[i].SetFloat(UnscaledTimeId, time);
                }
            }
        }

        if (pulsingBorder != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
            Color c = borderBaseColor;
            c.a = borderBaseColor.a * alpha;
            pulsingBorder.color = c;
        }
    }

    private void OnDestroy()
    {
        if (instancedMaterials != null)
        {
            for (int i = 0; i < instancedMaterials.Length; i++)
            {
                if (instancedMaterials[i] != null)
                {
                    Destroy(instancedMaterials[i]);
                }
            }
        }
    }
}
