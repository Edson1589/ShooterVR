using TMPro;
using UnityEngine;

public class WeaponAmmoUI : MonoBehaviour
{
    [Header("Interfaz")]
    public TMP_Text ammoText;

    private WeaponShooter weaponShooter;

    private void Awake()
    {
        weaponShooter = GetComponent<WeaponShooter>();
    }

    private void OnEnable()
    {
        weaponShooter.AmmoChanged += HandleAmmoChanged;

    }

    private void OnDisable()
    {
        weaponShooter.AmmoChanged -= HandleAmmoChanged;

    }

    private void HandleAmmoChanged(int currentAmmo, int magazineSize)
    {
        if (ammoText == null) return;
        ammoText.text = currentAmmo.ToString();
        ammoText.color = currentAmmo <= 0 ? new Color(1f, 0.3f, 0.3f) : currentAmmo <= magazineSize * 0.2f ? new Color(1f, 0.75f, 0.25f) : Color.white;
    }

    private void Start()
    {
        HandleAmmoChanged(weaponShooter.CurrentAmmo, weaponShooter.MagazineSize);
    }
}
