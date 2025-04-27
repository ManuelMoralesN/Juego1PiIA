using UnityEngine;
using System.Collections.Generic;

public class EnemyGenerator : MonoBehaviour
{
    [Header("Prefabs y Referencias")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform playerSpawnPoint;

    [Header("Configuración de Dificultad")]
    [Range(0, 4)]
    public int funcionDeDificultad = 0; // Seleccionable en el editor: 0 a 4
    public KeyCode teclaReiniciar = KeyCode.I;

    // Rangos para stats aleatorios
    [Header("Rangos de Stats")]
    public Vector2 hpRange = new Vector2(10f, 100f);
    public Vector2 attackRange = new Vector2(5f, 30f);
    public Vector2 attackRateRange = new Vector2(0.2f, 2f);
    public Vector2 speedRange = new Vector2(1f, 5f);

    private List<PcgEnemy> enemigos = new List<PcgEnemy>(); // Lista de enemigos generados
    private GameObject enemigoActual;

    public PcgEnemy CurrentEnemy
    {
        get { return enemigoActual?.GetComponent<PcgEnemy>(); } // Retorna el último enemigo generado
    }

    void Start()
    {
        // Generar enemigos iniciales
        GenerarEnemigos(5); // Generar 5 enemigos de prueba
        // Ejecutar GreedySearch
        GreedySearch();
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaReiniciar))
        {
            Reiniciar();
        }
    }

    // Función para generar varios enemigos
    public void GenerarEnemigos(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            GenerarEnemigo();
        }
    }

    // Generar un solo enemigo
    public void GenerarEnemigo()
    {
        if (enemyPrefab == null || spawnPoint == null) return;

        // Instanciar enemigo en el punto de spawn
        enemigoActual = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        PcgEnemy enemy = enemigoActual.GetComponent<PcgEnemy>();

        if (enemy != null)
        {
            // Generar stats aleatorios
            float hp = Random.Range(hpRange.x, hpRange.y);
            float atk = Random.Range(attackRange.x, attackRange.y);
            float rate = Random.Range(attackRateRange.x, attackRateRange.y);
            float speed = Random.Range(speedRange.x, speedRange.y);
            int movimiento = Random.Range(0, 3); // 0: quieto, 1: sigue, 2: huye

            // Generar efecto aleatorio: se elige entre None (0), Slow (1), Burn (2) y Stun (3)
            UniqueEffect effect = (UniqueEffect)Random.Range(0, 4);

            // Llamar a la inicialización de estadísticas del enemigo
            enemy.InicializarStats(hp, atk, rate, speed, movimiento, effect);

            // Calcular la dificultad según la función seleccionada
            float dificultad = CalcularDificultad(enemy);
            enemy.SetDificultad(dificultad);

            // Calcular el TotalScore
            float totalScore = enemy.CalculateTotalScore();
            Debug.Log("Total Score del enemigo: " + totalScore);

            // Agregar enemigo a la lista
            enemigos.Add(enemy);
        }
    }

    // Método para calcular la dificultad
    float CalcularDificultad(PcgEnemy enemy)
    {
        float hp = enemy.stats.maxHP;
        float atk = enemy.stats.attackDamage;
        float rate = enemy.stats.attackRate;
        float extra = enemy.uniqueEffect != UniqueEffect.None ? 10f : 0f;  // Se accede a uniqueEffect desde PcgEnemy

        switch (funcionDeDificultad)
        {
            case 0:
                return hp + atk + (1f / rate);
            case 1:
                return hp + (atk * (1f / rate));
            case 2:
                return hp + (atk * (1f / rate)) + extra;
            case 3:
                return hp * atk * (1f / rate);
            case 4:
                return Mathf.Sqrt(hp * atk) + (5f / rate) + extra;
            case 5:
                return (hp / rate) + atk + extra;
            case 6:
                return atk * (10f / rate) + extra;
            case 7:
                return (hp * 0.75f) + (atk * 0.5f) + (1f / rate) * 2 + extra;
            case 8:
                return Mathf.Pow(atk, 1.5f) * (1f / rate) + extra;
            case 9:
                return (atk + extra) * rate + hp * 0.3f;
            default:
                return hp + atk;
        }
    }

    // Implementación de GreedySearch
    public void GreedySearch()
    {
        PcgEnemy bestEnemy = null;
        float bestScore = float.MinValue; // Inicializamos con el valor más bajo posible

        // Recorremos la lista de enemigos y evaluamos el TotalScore
        foreach (PcgEnemy enemigo in enemigos)
        {
            float totalScore = enemigo.CalculateTotalScore();  // Obtenemos el TotalScore del enemigo

            // Si el TotalScore es el mejor hasta ahora, lo guardamos
            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestEnemy = enemigo;
            }
        }

        // Mostrar el mejor enemigo
        if (bestEnemy != null)
        {
            Debug.Log("El mejor enemigo es: " + bestEnemy.name + " con un TotalScore de: " + bestScore);
        }
        else
        {
            Debug.Log("No se encontraron enemigos.");
        }
    }

    // Método para reiniciar el juego
    void Reiniciar()
    {
        // Destruir el enemigo actual
        if (enemigoActual != null)
        {
            Destroy(enemigoActual);
        }

        // Reposicionar al jugador y reiniciar su estado
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.ResetPlayer();
            }
        }

        // Generar un nuevo enemigo
        GenerarEnemigo();
    }
}
