using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShooter : MonoBehaviour
{
    [Header("Configuración del arma")]
    public WeaponData weaponData;

    [Header("Referencias")]
    public Transform firePoint;

    [Header("Input")]
    public InputActionReference fireAction;
    public InputActionReference reloadAction;

    private int currentAmmo;
    private float nextShotTime;
    private bool isReloading;

    public int CurrentAmmo => currentAmmo;
    public bool IsReloading => isReloading;

    private void Awake()
    {
        if (weaponData != null)
        {
            currentAmmo = weaponData.magazineSize;
        }
    }

    private void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.action.Enable();
        }

        if (reloadAction != null)
        {
            reloadAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.action.Disable();
        }

        if (reloadAction != null)
        {
            reloadAction.action.Disable();
        }
    }

    private void Update()
    {
        if (weaponData == null) return;

        HandleFireInput();
        HandleReloadInput();
    }

    private void HandleFireInput()
    {
        if (fireAction == null) return;

        if (fireAction.action.WasPressedThisFrame())
        {
            TryFire();
        }
    }

    private void HandleReloadInput()
    {
        if (reloadAction == null) return;

        if (reloadAction.action.WasPressedThisFrame())
        {
            TryReload();
        }
    }

    public void TryFire()
    {
        if (weaponData == null) return;

        if (weaponData.bulletData == null) return;

        if (firePoint == null) return;

        if (isReloading) return;

        if (Time.time < nextShotTime) return;

        if (currentAmmo <= 0)
        {
            TryReload();
            return;
        }

        FireProjectile();

        currentAmmo--;

        nextShotTime = Time.time + weaponData.fireCooldown;
    }

    private void FireProjectile()
    {
        BulletData bulletData = weaponData.bulletData;

        if (bulletData.projectilePrefab == null) return;

        GameObject projectileObject = Instantiate(bulletData.projectilePrefab, firePoint.position, firePoint.rotation);

        if (projectileObject.TryGetComponent(out BulletProjectile projectile))
        {
            projectile.Initialize(bulletData, firePoint.forward);
        }
        else
        {
            Destroy(projectileObject);
        }
    }

    public void TryReload()
    {
        if (isReloading) return;

        if (currentAmmo >= weaponData.magazineSize) return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(weaponData.reloadDuration);

        currentAmmo = weaponData.magazineSize;

        isReloading = false;
    }

    public void Configure(WeaponData newWeaponData, Transform newFirePoint)
    {
        weaponData = newWeaponData;
        firePoint = newFirePoint;

        currentAmmo = weaponData.magazineSize;
    }
}