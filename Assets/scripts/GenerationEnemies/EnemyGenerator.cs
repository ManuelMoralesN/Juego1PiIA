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

    // Rangos para stats aleatorios (ajústalos según necesites)
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
            int movimiento = Random.Range(0, 3);      // 0: quieto, 1: sigue al jugador, 2: se aleja
            int efecto = Random.Range(0, 2);            // 0: sin efecto, 1: con efecto único

            enemy.InicializarStats(hp, atk, rate, speed, movimiento, efecto);

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
        float extra = enemy.efectoUnico == 1 ? 10f : 0f; // Se añade si tiene efecto único

        switch (funcionDeDificultad)
        {
            case 0:
                // Versión 1: Dificultad = HP + AttackPower + 1.0f/AttackRate
                return hp + atk + (1f / rate);

            case 1:
                // Versión 2: Dificultad = HP + (AttackPower * 1.0f/AttackRate)
                return hp + (atk * (1f / rate));

            case 2:
                // Versión 3: Dificultad = HP + AttackPower*(1.0f/AttackRate) + (EfectoÚnico)
                return hp + (atk * (1f / rate)) + extra;

            case 3:
                // Versión 4: Dificultad = HP * AttackPower * (1.0f/AttackRate)
                return hp * atk * (1f / rate);

            case 4:
                // Función adicional: combinando raíces y extra
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

        // Reposicionar al jugador (si se encuentra)
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
        }

        // Generar un nuevo enemigo
        GenerarEnemigo();
    }
}
