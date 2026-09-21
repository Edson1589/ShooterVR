using System;
using UnityEngine;

public class ReloadGestureDetector : MonoBehaviour
{
    [Header("Configuración del gesto")]
    [Range(-1f, 1f)]
    public float downThreshold = 0.75f;

    [Range(-1f, 1f)]
    public float resetThreshold = 0.25f;

    public float holdDuration = 0.25f;

    private WeaponView weaponView;

    private float holdTimer;
    private bool gestureArmed;

    public event Action ReloadGesturePerformed;

    public void Configure(WeaponView newWeaponView)
    {
        weaponView = newWeaponView;
        holdTimer = 0f;
        gestureArmed = false;
    }

    private void Update()
    {
        if (weaponView == null) return;

        DetectReloadGesture();
    }

    private void DetectReloadGesture()
    {
        float downAlignment = Vector3.Dot(weaponView.FireDirection.normalized, Vector3.down);

        if (!gestureArmed)
        {
            if (downAlignment <= resetThreshold)
            {
                gestureArmed = true;
            }

            return;
        }

        if (downAlignment < downThreshold)
        {
            holdTimer = 0f;
            return;
        }

        holdTimer += Time.deltaTime;

        if (holdTimer < holdDuration) return;

        holdTimer = 0f;
        gestureArmed = false;

        ReloadGesturePerformed?.Invoke();
    }
}