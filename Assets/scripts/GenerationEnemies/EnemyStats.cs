using UnityEngine;

// Clase para almacenar las estadísticas del enemigo
public class EnemyStats
{
    public float maxHP;
    public float currentHP;
    public float attackDamage;
    public float attackRate;
    public float attackRange;
    public float movementSpeed;
    public float difficultyValue;

    public string PrintStats()
    {
        return $"HP: {maxHP}, Damage: {attackDamage}, Attack Rate: {attackRate}, Attack Range: {attackRange}, Speed: {movementSpeed}, Difficulty: {difficultyValue}";
    }
}
