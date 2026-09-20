using UnityEngine;

[CreateAssetMenu(fileName = "BulletData_", menuName = "VR Shooter/Bullets/Bullet Data")]
public class BulletData : ScriptableObject
{
    [Header("Identificación")]
    public string bulletName;

    [Header("Proyectil")]
    public float speed = 25f;
    public float lifetime = 3f;

    [Header("Gameplay")]
    public float damage = 10f;

    [Header("Prefab")]
    public GameObject projectilePrefab;

    [Header("Efectos")]

    [Tooltip("Efecto que acompaña a la bala durante su trayectoria.")]
    public GameObject trailEffectPrefab;

    [Tooltip("Efecto que aparece en el arma al realizar el disparo.")]
    public GameObject shootEffectPrefab;

    [Tooltip("Efecto que aparece cuando la bala impacta contra una superficie u objetivo.")]
    public GameObject impactEffectPrefab;
}
