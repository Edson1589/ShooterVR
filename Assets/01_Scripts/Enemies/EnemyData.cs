using UnityEngine;

public enum EnemyType
{
    Normal = 0,
    Shooter = 1,
    Kamikaze = 2
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
    public float contactDamage = 10f;

    [Header("Daño: Shooter")]
    public BulletData bulletData;

    [Header("Puntuación")]
    public int scoreValue = 10;

    [Header("Efectos visuales")]
    public GameObject hitEffectPrefab;
    public GameObject deathEffectPrefab;
    public GameObject spawnEffectPrefab;
}