using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponShooter : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference fireAction;

    private WeaponData weaponData;
    private WeaponView weaponView;

    private ReloadGestureDetector reloadGestureDetector;

    private int currentAmmo;
    private float nextShotTime;
    private bool isReloading;

    private Coroutine reloadCoroutine;

    public int CurrentAmmo => currentAmmo;

    public int MagazineSize => weaponData != null ? weaponData.magazineSize : 0;

    public bool IsReloading => isReloading;

    public event Action<int, int> AmmoChanged;
    public event Action<bool> ReloadStateChanged;

    private void Awake()
    {
        reloadGestureDetector = GetComponent<ReloadGestureDetector>();
    }

    private void OnEnable()
    {
        fireAction?.action.Enable();

        reloadGestureDetector.ReloadGesturePerformed += HandleReloadGesture;
    }

    private void OnDisable()
    {
        fireAction?.action.Disable();

        reloadGestureDetector.ReloadGesturePerformed -= HandleReloadGesture;
    }

    private void Update()
    {
        if (!IsConfigured()) return;

        HandleFireInput();
    }

    private bool IsConfigured()
    {
        return weaponData != null && weaponView != null;
    }

    private void HandleFireInput()
    {
        if (fireAction == null) return;

        if (fireAction.action.WasPressedThisFrame())
        {
            TryFire();
        }
    }

    private void HandleReloadGesture()
    {
        TryReload();
    }

    public void TryFire()
    {
        if (!IsConfigured()) return;

        if (weaponData.bulletData == null) return;

        if (isReloading) return;

        if (Time.time < nextShotTime) return;

        if (currentAmmo <= 0) return;

        if (!FireProjectile()) return;

        currentAmmo--;

        nextShotTime = Time.time + weaponData.fireCooldown;

        NotifyAmmoChanged();
    }

    private bool FireProjectile()
    {
        BulletData bulletData = weaponData.bulletData;

        if (bulletData.projectilePrefab == null) return false;

        GameObject projectileObject = Instantiate(bulletData.projectilePrefab, weaponView.FirePosition, weaponView.FireRotation);

        if (!projectileObject.TryGetComponent(out BulletProjectile projectile))
        {
            Destroy(projectileObject);
            return false;
        }

        projectile.Initialize(bulletData, weaponView.FireDirection);

        if (bulletData.shootEffectPrefab != null)
        {
            GameObject shootVfx = Instantiate(bulletData.shootEffectPrefab, weaponView.FirePosition, weaponView.FireRotation);
            ParticleSystem ps = shootVfx.GetComponent<ParticleSystem>();
            float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 1f;
            Destroy(shootVfx, Mathf.Max(lifetime, 0.5f));
        }

        return true;
    }

    public bool TryReload()
    {
        if (!IsConfigured()) return false;

        if (isReloading) return false;

        if (currentAmmo >= weaponData.magazineSize) return false;

        reloadCoroutine = StartCoroutine(ReloadRoutine());

        return true;
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        NotifyReloadStateChanged();

        yield return new WaitForSeconds(weaponData.reloadDuration);

        currentAmmo = weaponData.magazineSize;

        isReloading = false;
        reloadCoroutine = null;

        NotifyAmmoChanged();
        NotifyReloadStateChanged();
    }

    public void Configure(WeaponData newWeaponData, WeaponView newWeaponView)
    {
        CancelReload();

        weaponData = newWeaponData;
        weaponView = newWeaponView;

        reloadGestureDetector.Configure(newWeaponView);

        currentAmmo = weaponData.magazineSize;

        nextShotTime = 0f;

        NotifyAmmoChanged();
    }

    private void CancelReload()
    {
        if (reloadCoroutine != null)
        {
            StopCoroutine(reloadCoroutine);
            reloadCoroutine = null;
        }

        if (isReloading)
        {
            isReloading = false;
            NotifyReloadStateChanged();
        }
    }

    private void NotifyAmmoChanged()
    {
        AmmoChanged?.Invoke(currentAmmo, weaponData.magazineSize);
    }

    private void NotifyReloadStateChanged()
    {
        ReloadStateChanged?.Invoke(isReloading);
    }
}