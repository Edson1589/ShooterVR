using UnityEngine;

public class EnemyLocomotionAnimation : MonoBehaviour
{
    [Header("Ajuste de los pies al suelo")]
    public float footSoleOffset = 0.08f;
    private static readonly int Attack = Animator.StringToHash("Attack");
    private Enemy enemy;
    private EnemyCombat combat;
    private Animator animator;
    private CapsuleCollider body;
    private Transform leftFoot;
    private Transform rightFoot;
    private Vector3 restPosition;
    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        combat = GetComponentInParent<EnemyCombat>();
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        restPosition = transform.localPosition;
        if (enemy != null) body = enemy.GetComponent<CapsuleCollider>();
        if (animator.isHuman && animator.avatar != null && animator.avatar.isValid)
        {
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }
    }

    private void Update()
    {
        if (enemy == null || enemy.enemyData == null || combat == null) return;
        if (enemy.enemyData.enemyType == EnemyType.Kamikaze)
        {
            IsAttacking = IsAttacking ? combat.CanContinueMeleeAttack : combat.IsInMeleeRange;
            animator.SetBool(Attack, IsAttacking);
        }
    }

    public void MeleeHit()
    {
        if (combat != null && animator.GetBool(Attack)) combat.ApplyAnimatedMeleeHit();
    }

    private void LateUpdate()
    {
        if (enemy == null || enemy.IsDead || enemy.IsResolved || body == null || !body.enabled || leftFoot == null || rightFoot == null) return;
        transform.localPosition = restPosition;
        float soleY = Mathf.Min(leftFoot.position.y, rightFoot.position.y) - footSoleOffset * Mathf.Abs(enemy.transform.lossyScale.y);
        transform.position += Vector3.up * (body.bounds.min.y - soleY);
    }
}
