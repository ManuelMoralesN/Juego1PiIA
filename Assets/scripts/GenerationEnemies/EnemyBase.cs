using System.Collections;
using UnityEngine;

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
    public int tipoMovimiento; // 0: quieto, 1: sigue al jugador, 2: escapa, etc.
    public int efectoUnico; // 0: ninguno, 1: ralentiza, etc.

    [Header("Visual")]
    public Renderer rend;

    private Transform target;

    void Start()
    {
        currentHP = maxHP;
        target = GameObject.FindWithTag("Player").transform;

        if (rend == null)
            rend = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        Mover();
    }

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

    public void RecibirDaño(float daño)
    {
        currentHP -= daño;
        if (currentHP <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Destroy(gameObject);
    }
}
