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

    [Header("Configuración de Hordas")]
    public int rondaActual = 1; // Ronda inicial
    public int enemigosPorHorda = 1; // Número de enemigos por horda, comienza en 1
    public int maxEnemigosPorHorda = 5; // Límite máximo de enemigos en una horda

    private bool todosMuertos = true; // Estado que indica si todos los enemigos fueron eliminados
    private List<PcgEnemy> enemigosEnHorda = new List<PcgEnemy>(); // Lista de enemigos de la horda actual

    void Update()
    {
        // Si todos los enemigos han muerto, iniciamos la siguiente horda
        if (todosMuertos)
        {
            // Generar enemigos para la nueva horda
            GenerarHorda(rondaActual);
        }
    }

    // Función para generar los enemigos de la horda
    public void GenerarHorda(int ronda)
    {
        enemigosEnHorda.Clear(); // Limpiamos la lista de enemigos de la horda actual

        // Determinamos el número de enemigos según la ronda
        enemigosPorHorda = Mathf.Min(maxEnemigosPorHorda, ronda); // La cantidad de enemigos aumenta pero no supera el límite

        // Generar enemigos para esta horda
        for (int i = 0; i < enemigosPorHorda; i++)
        {
            GenerarEnemigo(ronda); // Generar un enemigo por vez
        }

        todosMuertos = false; // Aún no han muerto todos los enemigos
    }

    // Función para generar un solo enemigo con dificultad ajustada según la ronda
    public void GenerarEnemigo(int ronda)
    {
        if (enemyPrefab == null || spawnPoint == null) return;

        // Instanciar enemigo en el punto de spawn
        enemigoActual = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        PcgEnemy enemy = enemigoActual.GetComponent<PcgEnemy>();

        if (enemy != null)
        {
            // Ajustamos las estadísticas del enemigo para la ronda actual
            enemy.DifficultyWeight = Mathf.Clamp01(ronda / 10f); // Aumento gradual de la dificultad con las rondas
            enemy.GeneratePcgEnemy(); // Generar el enemigo con nuevas estadísticas

            // Agregamos el enemigo a la lista
            enemigosEnHorda.Add(enemy);
        }
    }

    // Función para verificar si todos los enemigos fueron eliminados
    public void VerificarMuerteEnemigos()
    {
        todosMuertos = enemigosEnHorda.TrueForAll(e => e.isDead); // Verificar si todos los enemigos están muertos
    }

    // Llamar esta función cuando todos los enemigos sean derrotados para avanzar a la siguiente ronda
    public void EnemigosEliminados()
    {
        VerificarMuerteEnemigos();
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
        GenerarEnemigo(rondaActual);
    }
}
