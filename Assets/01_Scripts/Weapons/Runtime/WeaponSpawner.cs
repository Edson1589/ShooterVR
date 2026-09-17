using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [Header("Arma inicial")]
    public WeaponData startingWeapon;

    [Header("Referencias")]
    public WeaponShooter weaponShooter;

    private GameObject currentWeaponObject;

    private void Start()
    {
        EquipWeapon(startingWeapon);
    }

    public void EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null) return;

        if (weaponData.weaponPrefab == null) return;

        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
        }

        currentWeaponObject = Instantiate(weaponData.weaponPrefab, transform);

        currentWeaponObject.transform.localPosition = Vector3.zero;
        currentWeaponObject.transform.localRotation = Quaternion.identity;

        WeaponView weaponView = currentWeaponObject.GetComponent<WeaponView>();

        if (weaponView == null) return;

        weaponShooter.Configure(weaponData, weaponView.FirePoint);
    }
}