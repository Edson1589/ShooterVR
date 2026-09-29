using UnityEngine;

[CreateAssetMenu(fileName = "BulletData_", menuName = "Shooter VR/Proyectiles/Datos del proyectil")]
public class BulletData : ScriptableObject
{
    [Header("Identificación")]
    public string bulletName;

    [Header("Proyectil")]
    public float speed = 25f;
    public float lifetime = 3f;

    [Header("Jugabilidad")]
    public float damage = 10f;

    [Header("Objeto prefabricado")]
    public GameObject projectilePrefab;

    [Header("Efectos")]
    public GameObject trailEffectPrefab;
    public GameObject shootEffectPrefab;
    public GameObject impactEffectPrefab;

    [Header("Audio")]
    public AudioClip enemyImpactSound;
    public AudioClip surfaceImpactSound;
    [Range(0f, 1f)] public float impactVolume = 0.45f;
}
