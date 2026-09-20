using TMPro;
using UnityEngine;

public class WeaponAmmoUI : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text ammoText;
    public TMP_Text reloadText;

    private WeaponShooter weaponShooter;

    private void Awake()
    {
        weaponShooter = GetComponent<WeaponShooter>();
    }

    private void OnEnable()
    {
        weaponShooter.AmmoChanged += HandleAmmoChanged;

        weaponShooter.ReloadStateChanged += HandleReloadStateChanged;
    }

    private void OnDisable()
    {
        weaponShooter.AmmoChanged -= HandleAmmoChanged;

        weaponShooter.ReloadStateChanged -= HandleReloadStateChanged;
    }

    private void HandleAmmoChanged(int currentAmmo, int magazineSize)
    {
        ammoText.text = $"{currentAmmo} / {magazineSize}";
    }

    private void HandleReloadStateChanged(bool isReloading)
    {
        reloadText.gameObject.SetActive(isReloading);

        if (isReloading)
        {
            reloadText.text = "RECARGANDO...";
        }
    }
}