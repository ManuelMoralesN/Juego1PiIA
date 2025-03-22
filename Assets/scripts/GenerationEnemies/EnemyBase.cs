using UnityEngine;

// Enum para definir los efectos únicos disponibles
public enum UniqueEffect
{
    None,
    Slow,
    Burn,
    Stun
}

public class EnemyBase : MonoBehaviour
{
    [Header("Stats del Enemigo")]
    public float maxHP;
    public float currentHP;
    public float attackPower;
    public float attackRate; // Intervalo de ataque en segundos
    public float moveSpeed;

    [Header("Evaluación")]
    public float dificultadCalculada;
    public int tipoMovimiento; // 0: quieto, 1: sigue al jugador, 2: se aleja

    // Ahora usamos un enum para definir el efecto único
    [Header("Efecto Único")]
    public UniqueEffect uniqueEffect; // None, Slow, Burn, Stun, etc.

    [Header("Visual")]
    public Renderer rend;

    [Header("Ataque")]
    public float attackRange = 2f; // Rango en el que el enemigo puede atacar
    public float totalDamageInflicted = 0f;

    private float nextAttackTime = 0f;

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
        Atacar();
    }

    /// <summary>
    /// Inicializa los stats del enemigo con los valores proporcionados.
    /// </summary>
    public void InicializarStats(float hp, float atk, float rate, float speed, int movimiento, UniqueEffect effect)
    {
        maxHP = hp;
        attackPower = atk;
        attackRate = rate;
        moveSpeed = speed;
        tipoMovimiento = movimiento;
        uniqueEffect = effect;
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
                {
                    Vector3 direction = (target.position - transform.position).normalized;
                    if (direction != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                    }
                    transform.position += transform.forward * moveSpeed * Time.deltaTime;
                }
                break;
            case 2: // Se aleja del jugador
                {
                    Vector3 fleeDirection = (transform.position - target.position).normalized;
                    if (fleeDirection != Vector3.zero)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                    }
                    transform.position += transform.forward * moveSpeed * Time.deltaTime;
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Si el jugador está en rango, ataca usando su AttackRate como cooldown.
    /// </summary>
    void Atacar()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            // Registrar el daño infligido, sin importar la invulnerabilidad del jugador
            totalDamageInflicted += attackPower;

            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage((int)attackPower);
                Debug.Log("Enemigo ataca al jugador, causando " + attackPower + " de daño.");

                // Aplicar efecto único si corresponde
                if (uniqueEffect != UniqueEffect.None)
                {
                    playerHealth.ApplyUniqueEffect(uniqueEffect);
                }
            }
            nextAttackTime = Time.time + attackRate;
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
    /// Destruye el objeto enemigo.
    /// </summary>
    void Morir()
    {
        Destroy(gameObject);
    }
}
