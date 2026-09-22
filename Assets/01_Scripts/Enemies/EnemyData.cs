using UnityEngine;

public enum EnemyType
{
    [InspectorName("Normal")] Normal,
    [InspectorName("Tirador")] Shooter
}

[CreateAssetMenu(fileName = "EnemyData_", menuName = "VR Shooter/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Tipo y vida")]
    public EnemyType enemyType;
    public float maxHealth = 30f;

    [Header("Ataques")]
    public float attackCooldown = 1f;

    [Header("Enemigo normal")]
    public float moveSpeed = 1.5f;
    public float stoppingDistance = 0.65f;

    [InspectorName("Daño por contacto")]
    public float contactDamage = 10f;

    [Header("Enemigo tirador")]
    public BulletData bulletData;
}
