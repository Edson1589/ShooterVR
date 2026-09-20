using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Arma inicial")]
    public WeaponData startingWeapon;

    private WeaponShooter weaponShooter;
    private GameObject currentWeaponObject;

    private void Awake()
    {
        weaponShooter = GetComponent<WeaponShooter>();
    }

    private void Start()
    {
        EquipWeapon(startingWeapon);
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return;

        if (weaponData.weaponPrefab == null) return;

        RemoveCurrentWeapon();

        currentWeaponObject = Instantiate(weaponData.weaponPrefab, transform);

        currentWeaponObject.transform.localPosition = Vector3.zero;

        currentWeaponObject.transform.localRotation = Quaternion.identity;

        WeaponView weaponView = currentWeaponObject.GetComponent<WeaponView>();

        if (weaponView == null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;

            return;
        }

        weaponShooter.Configure(weaponData, weaponView);
    }

    private void RemoveCurrentWeapon()
    {
        if (currentWeaponObject == null) return;

        Destroy(currentWeaponObject);
        currentWeaponObject = null;
    }
}