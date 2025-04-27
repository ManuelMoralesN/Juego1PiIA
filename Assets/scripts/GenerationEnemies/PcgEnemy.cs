using UnityEngine;

public class PcgEnemy : EnemyBase
{
    public bool isDead = false;
    private static float maxDifficulty = 100f; // Valor de dificultad m�xima (definir un valor alto)

    [Header("Configuración de Pesos")]
    [Range(0f, 1f)]
    public float DifficultyWeight = 0.5f;  // Peso de la dificultad
    [Range(0f, 1f)]
    public float BalanceWeight = 0.5f;    // Peso del balance

    // Método para generar un enemigo con estadísticas aleatorias y normalizadas
    public void GeneratePcgEnemy()
    {
        // Generar estadísticas aleatorias dentro de los rangos definidos en el generador
        stats.maxHP = Random.Range(10f, 100f);  // Rango de vida aleatorio
        stats.attackDamage = Random.Range(5f, 30f);  // Rango de daño aleatorio
        stats.attackRate = Random.Range(0.2f, 2f);  // Rango de velocidad de ataque aleatorio
        stats.attackRange = Random.Range(1f, 10f);  // Rango de distancia de ataque aleatorio
        stats.movementSpeed = Random.Range(1f, 5f);  // Rango de velocidad de movimiento aleatorio

        // Normalizar las estadísticas a un rango de 0 a 1
        NormalizeStats();

        // Asignar un tipo de movimiento aleatorio (quieto, persigue, huye)
        tipoMovimiento = Random.Range(0, 3); // 0: quieto, 1: persigue, 2: huye

        // Asignar un efecto único aleatorio (None, Slow, Burn, Stun)
        uniqueEffect = (UniqueEffect)Random.Range(0, 4);

        // Calcular el valor de dificultad basado en las estadísticas generadas
        ComputeDifficultyValue();
    }

    // Método para normalizar las estadísticas del enemigo
    private void NormalizeStats()
    {
        // Normalizar HP entre 0 y 1
        stats.maxHP = Mathf.InverseLerp(10f, 100f, stats.maxHP); // Rango entre 10 y 100

        // Normalizar daño entre 0 y 1
        stats.attackDamage = Mathf.InverseLerp(5f, 30f, stats.attackDamage); // Rango entre 5 y 30

        // Normalizar la tasa de ataque entre 0 y 1
        stats.attackRate = Mathf.InverseLerp(0.2f, 2f, stats.attackRate); // Rango entre 0.2 y 2

        // Normalizar el rango de ataque entre 0 y 1
        stats.attackRange = Mathf.InverseLerp(1f, 10f, stats.attackRange); // Rango entre 1 y 10

        // Normalizar velocidad de movimiento entre 0 y 1
        stats.movementSpeed = Mathf.InverseLerp(1f, 5f, stats.movementSpeed); // Rango entre 1 y 5
    }

    // Método para calcular la dificultad del enemigo basado en sus estadísticas normalizadas
    public void ComputeDifficultyValue()
    {
        // Normalizamos las estadísticas de comportamiento
        float normalizedMovement = GetMovementWeight(); // Normalización del tipo de movimiento
        float normalizedEffect = uniqueEffect != UniqueEffect.None ? 1f : 0f; // Si tiene un efecto único, se asigna 1, sino 0

        // Cálculo de la dificultad basado en las estadísticas
        stats.difficultyValue = DifficultyFunction(stats.maxHP, stats.attackDamage, stats.attackRate,
            stats.attackRange, stats.movementSpeed, normalizedMovement, normalizedEffect);

        // Normalizar la dificultad (ahora la dificultad estará entre 0 y 1)
        stats.difficultyValue = Mathf.InverseLerp(0f, maxDifficulty, stats.difficultyValue);
    }

    // Método para calcular la dificultad utilizando las estadísticas normalizadas
    float DifficultyFunction(float hp, float attackDamage, float attackRate, float attackRange, float movementSpeed, float movementWeight, float effectWeight)
    {
        // La dificultad se calcula ponderando las estadísticas
        return (hp + attackDamage + (1f / attackRate) + attackRange + movementSpeed + movementWeight + effectWeight);
    }

    // Asignar un peso al tipo de movimiento
    private float GetMovementWeight()
    {
        switch (tipoMovimiento)
        {
            case 0: return 0f;  // Quieto
            case 1: return 1f;  // Persigue
            case 2: return 0.4f;  // Huye
            default: return 0f;
        }
    }

    // Método para calcular el BalanceScore, que mide la "completitud" del enemigo
    public float CalculateBalanceScore()
    {
        // Ponderar las estadísticas en cuanto a balance
        float balance = Mathf.Abs(stats.attackDamage - stats.attackRate);  // Un balance más bajo sería mejor
        float movementBalance = Mathf.Abs(stats.movementSpeed - 0.5f);  // Movimiento balanceado cerca de 0.5 (ni muy rápido ni muy lento)

        return 1f - (balance + movementBalance) / 2f;  // Balance entre 0 y 1, con 1 siendo el más balanceado
    }

    // Método para calcular el TotalScore, combinando Difficulty y Balance con sus respectivos weights
    public float CalculateTotalScore()
    {
        // Calcular Difficulty y Balance
        float difficultyScore = stats.difficultyValue;
        float balanceScore = CalculateBalanceScore();

        // Calcular el TotalScore basado en los weights
        return difficultyScore * DifficultyWeight + balanceScore * BalanceWeight;
    }
}
