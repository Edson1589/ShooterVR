using UnityEngine;

public class WeaponView : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform firePoint;

    public bool HasFirePoint => firePoint != null;

    public Vector3 FirePosition => firePoint.position;

    public Quaternion FireRotation => firePoint.rotation;

    public Vector3 FireDirection => firePoint.forward;
}
