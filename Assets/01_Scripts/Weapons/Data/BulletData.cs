using UnityEngine;

[CreateAssetMenu(fileName = "BulletData_", menuName = "VR Shooter/Bullets/Bullet Data")]
public class BulletData : ScriptableObject
{
    [Header("Identificación")]
    public string bulletName;

    [Header("Proyectil")]
    public float speed = 25f;
    public float lifeTime = 3f;

    [Header("Gameplay")]
    public float damage = 10f;

    [Header("Prefab")]
    public GameObject projectilePrefab;
}
