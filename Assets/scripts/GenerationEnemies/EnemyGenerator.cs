using UnityEngine;
using System.Collections;

public class EnemyGenerator : MonoBehaviour
{
    [Header("Prefabs y Referencias")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform playerSpawnPoint;

    [Header("Configuración")]
    [Range(0, 4)] public int funcionDeDificultad = 0; // Seleccionable en editor
    public KeyCode teclaReiniciar = KeyCode.I;

    private GameObject enemigoActual;

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

        // Instanciar enemigo
        enemigoActual = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        EnemyBase enemy = enemigoActual.GetComponent<EnemyBase>();

        if (enemy != null)
        {
            // Stats aleatorios (ajusta los rangos como quieras)
            float hp = Random.Range(10f, 100f);
            float atk = Random.Range(5f, 30f);
            float rate = Random.Range(0.2f, 2f);
            float speed = Random.Range(1f, 5f);
            int movimiento = Random.Range(0, 3);
            int efecto = Random.Range(0, 2); // Por ahora 0 = nada, 1 = ralentizar

            enemy.InicializarStats(hp, atk, rate, speed, movimiento, efecto);

            // Calcular dificultad
            float dificultad = CalcularDificultad(enemy);
            enemy.SetDificultad(dificultad);
        }
    }

    float CalcularDificultad(EnemyBase enemy)
    {
        float hp = enemy.maxHP;
        float atk = enemy.attackPower;
        float rate = enemy.attackRate;
        float extra = enemy.efectoUnico == 1 ? 10f : 0f;

        switch (funcionDeDificultad)
        {
            case 0:
                return hp + atk + (1f / rate); // Como la versión 1

            case 1:
                return hp + atk * (1f / rate); // Como la versión 2

            case 2:
                return hp + atk * (1f / rate) + extra; // Como la versión 3

            case 3:
                return hp * atk * (1f / rate); // Como la versión 4

            case 4:
                return Mathf.Sqrt(hp * atk) + (5f / rate) + extra; // NUEVA función diferente

            default:
                return hp + atk; // Simple fallback
        }
    }

    void Reiniciar()
    {
        // Destruir enemigo actual
        if (enemigoActual != null)
        {
            Destroy(enemigoActual);
        }

        // Reposicionar al jugador
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && playerSpawnPoint != null)
        {
            player.transform.position = playerSpawnPoint.position;
        }

        // Generar nuevo enemigo
        GenerarEnemigo();
    }
}
