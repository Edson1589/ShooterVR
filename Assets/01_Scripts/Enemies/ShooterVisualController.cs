using UnityEngine;

public class ShooterVisualController : MonoBehaviour
{
    public float footSoleOffset = 0.08f;
    private EnemyCombat combat;
    private Transform shooterVisual;
    private Transform shooterHead;
    private Transform shooterHand;
    private Transform shooterLeftFoot;
    private Transform shooterRightFoot;
    private CapsuleCollider bodyCollider;
    private Vector3 visualRestPosition;
    private Quaternion visualRestRotation;
    private Vector3 targetPosition;

    private void Awake()
    {
        combat = GetComponent<EnemyCombat>();
        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null || !animator.isHuman || animator.avatar == null || !animator.avatar.isValid) return;
        shooterVisual = animator.transform != transform ? animator.transform : null;
        shooterHead = animator.GetBoneTransform(HumanBodyBones.Head);
        shooterHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        shooterLeftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        shooterRightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        bodyCollider = GetComponent<CapsuleCollider>();
        if (shooterVisual != null)
        {
            visualRestRotation = shooterVisual.localRotation;
            visualRestPosition = shooterVisual.localPosition;
        }
    }

    public void UpdatePose(bool hasTarget, Vector3 target)
    {
        GroundShooterVisual();
        if (!hasTarget) return;
        targetPosition = target;
        AlignShooterVisual();
        WeaponView weaponView = combat.Weapon;
        if (weaponView == null || !weaponView.HasFirePoint) return;
        Vector3 direction = targetPosition - weaponView.FirePosition;
        if (direction.sqrMagnitude < 0.001f) return;
        Transform aimPivot = shooterHand != null && weaponView.transform.IsChildOf(shooterHand) ? shooterHand : weaponView.transform;
        aimPivot.rotation = Quaternion.FromToRotation(weaponView.FireDirection, direction) * aimPivot.rotation;
    }

    private void GroundShooterVisual()
    {
        if (shooterVisual == null || bodyCollider == null || !bodyCollider.enabled || shooterLeftFoot == null || shooterRightFoot == null) return;

        shooterVisual.localPosition = visualRestPosition;
        float soleY = Mathf.Min(shooterLeftFoot.position.y, shooterRightFoot.position.y) - footSoleOffset * Mathf.Abs(transform.lossyScale.y);
        shooterVisual.position += Vector3.up * (bodyCollider.bounds.min.y - soleY);
    }

    private void AlignShooterVisual()
    {
        if (shooterHead == null) return;
        if (shooterVisual != null)
        {
            shooterVisual.localRotation = visualRestRotation;
            Vector3 forward = Vector3.ProjectOnPlane(shooterHead.forward, Vector3.up);
            Vector3 target = Vector3.ProjectOnPlane(targetPosition - shooterHead.position, Vector3.up);
            if (forward.sqrMagnitude > 0.001f && target.sqrMagnitude > 0.001f) shooterVisual.rotation = Quaternion.FromToRotation(forward, target) * shooterVisual.rotation;
        }

        Vector3 lookDirection = targetPosition - shooterHead.position;
        if (lookDirection.sqrMagnitude > 0.001f) shooterHead.rotation = Quaternion.FromToRotation(shooterHead.forward, lookDirection) * shooterHead.rotation;
    }
}
