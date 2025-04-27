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
    public int funcionDeDificultad = 0;
    public KeyCode teclaReiniciar = KeyCode.I;

    [Header("Rangos de Stats")]
    public Vector2 hpRange = new Vector2(10f, 100f);
    public Vector2 attackRange = new Vector2(5f, 30f);
    public Vector2 attackRateRange = new Vector2(0.2f, 2f);
    public Vector2 speedRange = new Vector2(1f, 5f);

    private GameObject enemigoActual;
    public GameObject CurrentEnemy { get { return enemigoActual; } }

    [Header("Horda")]
    public int enemigosPorHorda = 1;
    public float tiempoEntreGeneraciones = 2f;
    public int rondaActual = 1;
    private float tiempoTranscurrido = 0f;

    private List<GameObject> enemigosGenerados = new List<GameObject>();

    [Header("Pesos")]
    [Range(0f, 0.9f)] public float difficultyWeight = 0.7f;
    [Range(0f, 0.9f)] public float balanceWeight = 0.3f;

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

        AjustarPesos();

        tiempoTranscurrido += Time.deltaTime;

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
                // Cuando se terminen de generar enemigos, pasar a la siguiente ronda
                rondaActual++;

                // === NUEVO ===
                // Si la ronda supera el límite deseado, mostrar pantalla de victoria
                if (rondaActual > 5) //Rondas
                {
                    UIManager uiManager = FindObjectOfType<UIManager>();
                    if (uiManager != null)
                    {
                        uiManager.ShowVictory();
                    }
                    enabled = false; // Detiene la generación de enemigos
                    return;
                }
                // =========================

                enemigosPorHorda = Mathf.Min(rondaActual, 5);
                Debug.Log($"Comienza la ronda {rondaActual}");
                GenerarHorda();
            }
        }
    }

    public void GenerarEnemigo()
    {
        if (enemyPrefab == null || spawnPoint == null) return;

        enemigoActual = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemigosGenerados.Add(enemigoActual);

        EnemyBase enemy = enemigoActual.GetComponent<EnemyBase>();

        if (enemy != null)
        {
            float hp = Random.Range(hpRange.x, hpRange.y) + (rondaActual - 1) * 10f;
            float atk = Random.Range(attackRange.x, attackRange.y) + (rondaActual - 1) * 2f;
            float rate = Random.Range(attackRateRange.x, attackRateRange.y) - (rondaActual - 1) * 0.1f;
            float speed = Random.Range(speedRange.x, speedRange.y) + (rondaActual - 1) * 0.5f;
            int movimiento = Random.Range(0, 3);
            UniqueEffect effect = (UniqueEffect)Random.Range(0, 4);

            enemy.InicializarStats(hp, atk, rate, speed, movimiento, effect);

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
        enemigosPorHorda = Mathf.Min(rondaActual, 5);
        for (int i = 0; i < enemigosPorHorda; i++)
        {
            GenerarEnemigo();
        }
    }

    void Reiniciar()
    {
        foreach (var enemigo in enemigosGenerados)
        {
            if (enemigo != null)
            {
                Destroy(enemigo);
            }
        }

        enemigosGenerados.Clear();

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

        GenerarHorda();
    }

    private void AjustarPesos()
    {
        float totalWeight = difficultyWeight + balanceWeight;

        if (totalWeight != 1f)
        {
            balanceWeight = 1f - difficultyWeight;

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
