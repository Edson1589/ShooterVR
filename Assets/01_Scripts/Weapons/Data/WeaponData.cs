using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData_", menuName = "VR Shooter/Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Identificación ")]
    public string weaponName;

    [Header("Prefab")]
    public GameObject weaponPrefab;

    [Header("Proyectil")]
    public BulletData bulletData;

    [Header("Fuego")]
    public float fireCooldown = 0.25f;

    [Header("Munición")]
    public int magazineSize = 12;
    public float reloadDuration = 1.2f;
}
