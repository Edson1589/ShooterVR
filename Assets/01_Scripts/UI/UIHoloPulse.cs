using UnityEngine;
using UnityEngine.UI;

public class UIHoloPulse : MonoBehaviour
{
    private static readonly int UnscaledTimeId = Shader.PropertyToID("_UnscaledTime");

    [Header("Material Holográfico")]
    public Graphic[] holoGraphics;

    [Header("Borde Pulsante")]
    public Graphic pulsingBorder;
    public float pulseFrequency = 1.2f;
    [Range(0f, 1f)] public float minAlpha = 0.55f;
    [Range(0f, 1f)] public float maxAlpha = 0.95f;

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
