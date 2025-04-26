using UnityEngine;
using System.Collections;
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

    [Header("Rangos de Stats")]
    public Vector2 hpRange = new Vector2(10f, 100f);
    public Vector2 attackRange = new Vector2(5f, 30f);
    public Vector2 attackRateRange = new Vector2(0.2f, 2f);
    public Vector2 speedRange = new Vector2(1f, 5f);

    private GameObject enemigoActual;
    public GameObject CurrentEnemy { get { return enemigoActual; } }

    [Header("Horda")]
    public int enemigosPorHorda = 1; // Número de enemigos por ronda (por defecto 1)
    public float tiempoEntreGeneraciones = 2f; // Tiempo entre generación de enemigos
    public int rondaActual = 1; // Número de la ronda actual
    private float tiempoTranscurrido = 0f; // Tiempo que ha pasado

    private List<GameObject> enemigosGenerados = new List<GameObject>(); // Lista para mantener el control de los enemigos generados

    [Header("Pesos")]
    [Range(0f, 0.9f)] public float difficultyWeight = 0.7f;  // Peso de la dificultad
    [Range(0f, 0.9f)] public float balanceWeight = 0.3f;    // Peso del balance

    void Start()
    {
        GenerarHorda();
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaReiniciar))
        {
            Reiniciar();
        }

        // Verificar que la suma de los pesos sea siempre 1
        AjustarPesos();

        tiempoTranscurrido += Time.deltaTime;

        // Generar enemigos en la horda
        if (tiempoTranscurrido >= tiempoEntreGeneraciones && enemigoActual == null)
        {
            if (enemigosPorHorda > 0)
            {
                GenerarEnemigo();
                enemigosPorHorda--;
                tiempoTranscurrido = 0f;
            }
            else
            {
                // Cuando todos los enemigos de la horda se generaron, pasar a la siguiente ronda.
                rondaActual++;
                enemigosPorHorda = Mathf.Min(rondaActual, 5); // Aseguramos que entre 1 y 5 enemigos sean generados
                Debug.Log($"Comienza la ronda {rondaActual}");
                GenerarHorda(); // Regenerar enemigos para la siguiente ronda
            }
        }
    }

    public void GenerarEnemigo()
    {
        if (enemyPrefab == null || spawnPoint == null) return;

        enemigoActual = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

        // Guardamos el enemigo generado en la lista para destruirlo más tarde
        enemigosGenerados.Add(enemigoActual);

        EnemyBase enemy = enemigoActual.GetComponent<EnemyBase>();

        if (enemy != null)
        {
            // Aumentamos las estadísticas del enemigo en cada ronda
            float hp = Random.Range(hpRange.x, hpRange.y) + (rondaActual - 1) * 10f;  // Aumenta HP por ronda
            float atk = Random.Range(attackRange.x, attackRange.y) + (rondaActual - 1) * 2f; // Aumenta daño por ronda
            float rate = Random.Range(attackRateRange.x, attackRateRange.y) - (rondaActual - 1) * 0.1f; // Aumenta la tasa de ataque por ronda (pero disminuye un poco)
            float speed = Random.Range(speedRange.x, speedRange.y) + (rondaActual - 1) * 0.5f;  // Aumenta velocidad por ronda
            int movimiento = Random.Range(0, 3); // 0: quieto, 1: sigue, 2: huye
            UniqueEffect effect = (UniqueEffect)Random.Range(0, 4);

            enemy.InicializarStats(hp, atk, rate, speed, movimiento, effect);

            // Calculamos la dificultad en base a las estadísticas aumentadas
            float dificultad = CalcularDificultad(enemy);
            enemy.SetDificultad(dificultad);
        }
    }

    float CalcularDificultad(EnemyBase enemy)
    {
        float hp = enemy.maxHP;
        float atk = enemy.attackPower;
        float rate = enemy.attackRate;

        float extra = enemy.uniqueEffect != UniqueEffect.None ? 10f : 0f;

        float normalizedHp = (hp - hpRange.x) / (hpRange.y - hpRange.x);
        float normalizedAtk = (atk - attackRange.x) / (attackRange.y - attackRange.x);

        switch (funcionDeDificultad)
        {
            case 0:
                return normalizedHp + normalizedAtk + (1f / rate);
            case 1:
                return normalizedHp + (normalizedAtk * (1f / rate));
            case 2:
                return normalizedHp + (normalizedAtk * (1f / rate)) + extra;
            case 3:
                return normalizedHp * normalizedAtk * (1f / rate);
            case 4:
                return Mathf.Sqrt(normalizedHp * normalizedAtk) + (5f / rate) + extra;
            default:
                return normalizedHp + normalizedAtk;
        }
    }

    void GenerarHorda()
    {
        // Limitamos el número de enemigos a un máximo de 5, y mínimo 1 por ronda
        enemigosPorHorda = Mathf.Min(rondaActual, 5);  // Esto garantiza entre 1 y 5 enemigos sean generados
        for (int i = 0; i < enemigosPorHorda; i++)
        {
            GenerarEnemigo();
        }
    }

    void Reiniciar()
    {
        // Destruir los enemigos existentes
        foreach (var enemigo in enemigosGenerados)
        {
            if (enemigo != null)
            {
                Destroy(enemigo);
            }
        }

        // Limpiar la lista de enemigos generados
        enemigosGenerados.Clear();

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

        // Generar la primera horda de la ronda
        GenerarHorda();
    }

    // Método para ajustar los pesos y asegurar que siempre sumen 1
    private void AjustarPesos()
    {
        float totalWeight = difficultyWeight + balanceWeight;

        if (totalWeight != 1f)
        {
            // Ajustamos los pesos para que siempre sumen 1
            balanceWeight = 1f - difficultyWeight;

            // Limitar los pesos para que no superen el valor máximo
            if (difficultyWeight > 0.9f)
            {
                difficultyWeight = 0.9f;
                balanceWeight = 1f - difficultyWeight;
            }
            else if (balanceWeight > 0.9f)
            {
                balanceWeight = 0.9f;
                difficultyWeight = 1f - balanceWeight;
            }
        }
    }
}
