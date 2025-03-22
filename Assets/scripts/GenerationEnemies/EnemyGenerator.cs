using UnityEngine;
using System.Collections;

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

    private GameObject enemigoActual;
    public GameObject CurrentEnemy { get { return enemigoActual; } }

    void Start()
    {
        GenerarEnemigo();
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaReiniciar))
        {
            Reiniciar();
        }
    }

    public void GenerarEnemigo()
    {
        if (enemyPrefab == null || spawnPoint == null) return;

        // Instanciar enemigo en el punto de spawn
        enemigoActual = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        EnemyBase enemy = enemigoActual.GetComponent<EnemyBase>();

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

            enemy.InicializarStats(hp, atk, rate, speed, movimiento, effect);

            // Calcular la dificultad según la función seleccionada
            float dificultad = CalcularDificultad(enemy);
            enemy.SetDificultad(dificultad);
        }
    }

    float CalcularDificultad(EnemyBase enemy)
    {
        float hp = enemy.maxHP;
        float atk = enemy.attackPower;
        float rate = enemy.attackRate;
        // Si tiene efecto, se añade un extra
        float extra = enemy.uniqueEffect != UniqueEffect.None ? 10f : 0f;

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
            default:
                return hp + atk;
        }
    }

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
