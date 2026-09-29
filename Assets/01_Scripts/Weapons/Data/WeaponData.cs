using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData_", menuName = "Shooter VR/Armas/Datos del arma")]
public class WeaponData : ScriptableObject
{
    [Header("Identificación")]
    public string weaponName;

    [Header("Objeto prefabricado")]
    public GameObject weaponPrefab;

    [Header("Proyectil")]
    public BulletData bulletData;

    [Header("Fuego")]
    public float fireCooldown = 0.25f;

    [Header("Munición")]
    public int magazineSize = 12;
    public float reloadDuration = 1.2f;

    [Header("Audio")]
    public AudioClip fireSound;
    public AudioClip emptySound;
    public AudioClip reloadStartSound;
    public AudioClip reloadEndSound;
    [Range(0f, 1f)] public float soundVolume = 0.65f;
}
