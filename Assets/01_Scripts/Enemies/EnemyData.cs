using UnityEngine;

public enum EnemyType
{
    [InspectorName("Normal")] Normal = 0,
    [InspectorName("Tirador")] Shooter = 1,
    [InspectorName("Kamikaze")] Kamikaze = 2
}

[CreateAssetMenu(fileName = "EnemyData_", menuName = "VR Shooter/Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Tipo y vida")]
    public EnemyType enemyType;
    public float maxHealth = 30f;

    [Header("Ataques")]
    public float attackCooldown = 1f;

    [Header("Movimiento: normal y kamikaze")]
    public float moveSpeed = 1.5f;

    [Header("Enemigo kamikaze")]
    public float stoppingDistance = 0.65f;

    [Header("Contacto: normal y kamikaze")]
    [InspectorName("Daño por contacto")]
    public float contactDamage = 10f;

    [Header("Enemigo tirador")]
    public BulletData bulletData;
}
