using UnityEngine;
using System.Collections;
using TMPro;

public class EnemyBase : MonoBehaviour
{
    [Header("Stats del Enemigo")]
    public float maxHP;
    public float currentHP;
    public float attackPower;
    public float attackRate;
    public float moveSpeed;

    [Header("Evaluación")]
    public float dificultadCalculada;
    public int tipoMovimiento; // 0: quieto, 1: sigue al jugador, 2: se aleja
    public int efectoUnico;    // 0: sin efecto, 1: con efecto (ej. ralentiza)

    [Header("Visual")]
    public Renderer rend;

    private Transform target;

    void Start()
    {
        currentHP = maxHP;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto con tag 'Player'.");
        }

        if (rend == null)
        {
            rend = GetComponentInChildren<Renderer>();
            if (rend == null)
            {
                Debug.LogWarning("No se encontró un Renderer en el enemigo.");
            }
        }
    }

    void Update()
    {
        Mover();
    }

    /// <summary>
    /// Inicializa los stats del enemigo con los valores proporcionados.
    /// </summary>
    public void InicializarStats(float hp, float atk, float rate, float speed, int movimiento, int efecto)
    {
        maxHP = hp;
        attackPower = atk;
        attackRate = rate;
        moveSpeed = speed;
        tipoMovimiento = movimiento;
        efectoUnico = efecto;
        currentHP = maxHP;
    }

    /// <summary>
    /// Establece la dificultad calculada y cambia el color según su valor.
    /// </summary>
    public void SetDificultad(float valor)
    {
        dificultadCalculada = valor;

        if (rend != null)
        {
            if (valor > 50f)
                rend.material.color = Color.red;
            else if (valor > 25f)
                rend.material.color = Color.yellow;
            else
                rend.material.color = Color.green;
        }
    }

    /// <summary>
    /// Movimiento simple basado en el tipo asignado.
    /// </summary>
    void Mover()
    {
        if (target == null) return;

        switch (tipoMovimiento)
        {
            case 0: // No se mueve
                break;

            case 1: // Se acerca al jugador
                transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
                break;

            case 2: // Se aleja del jugador
                Vector3 dir = transform.position - target.position;
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Aplica daño al enemigo y verifica si debe morir.
    /// </summary>
    public void RecibirDaño(float daño)
    {
        currentHP -= daño;
        if (currentHP <= 0)
        {
            Morir();
        }
    }

    /// <summary>
    /// Método que destruye el objeto enemigo.
    /// </summary>
    void Morir()
    {
        Destroy(gameObject);
    }
}
