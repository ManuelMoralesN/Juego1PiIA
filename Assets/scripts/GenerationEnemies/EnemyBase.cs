using UnityEngine;
using UnityEngine.AI;

// Enum para definir los efectos
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
    public EnemyStats stats = new EnemyStats(); // Usamos la clase EnemyStats para manejar las estadísticas

    [Header("Evaluación")]
    public int tipoMovimiento; // 0: quieto, 1: persigue, 2: huye

    [Header("Efecto Único")]
    public UniqueEffect uniqueEffect; // None, Slow, Burn, Stun, etc.

    [Header("Visual")]
    public Renderer rend;

    [Header("Ataque")]
    public float attackRange = 2f; // Rango en el que el enemigo puede atacar
    public float totalDamageInflicted = 0f; // Acumula el daño total infligido

    private float nextAttackTime = 0f;
    private Transform target;
    private NavMeshAgent agent; // Componente para la navegación con NavMesh
    private Animator animator;  // Referencia al Animator para animaciones

    void Start()
    {
        stats.currentHP = stats.maxHP; // Inicializamos la salud del enemigo
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
        else
            Debug.LogWarning("No se encontró el objeto con tag 'Player'.");

        if (rend == null)
        {
            // Busca un Renderer 
            rend = GetComponentInChildren<Renderer>();
            if (rend == null)
                Debug.LogWarning("No se encontró un Renderer en el enemigo.");
        }

        //NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = stats.movementSpeed;

        // Obtener el Animator (se espera que esté en el mismo GameObject o en un hijo)
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("No se encontró un Animator en el enemigo.");
    }

    void Update()
    {
        Mover();
        Atacar();
    }

    /// <summary>
    /// Inicializa los stats del enemigo.
    /// </summary>
    public void InicializarStats(float hp, float atk, float rate, float speed, int movimiento, UniqueEffect effect)
    {
        stats.maxHP = hp;
        stats.attackDamage = atk;
        stats.attackRate = rate;
        stats.movementSpeed = speed;
        stats.attackRange = 2f;
        tipoMovimiento = movimiento;
        uniqueEffect = effect;
        stats.currentHP = stats.maxHP;

        if (agent != null)
            agent.speed = stats.movementSpeed;
    }

    /// <summary>
    /// Establece la dificultad calculada y cambia el color según su valor.
    /// </summary>
    public void SetDificultad(float valor)
    {
        stats.difficultyValue = valor;
        Debug.Log("Dificultad calculada: " + valor);
        if (rend != null)
        {
            if (valor > 100f)
                rend.material.color = Color.red;
            else if (valor > 50f)
                rend.material.color = Color.yellow;
            else
                rend.material.color = Color.green;
        }
    }

    /// <summary>
    /// Movimiento del enemigo según su tipo.
    /// </summary>
    void Mover()
    {
        if (target == null) return;

        bool isMoving = false;

        switch (tipoMovimiento)
        {
            case 0: // No se mueve
                isMoving = false;
                break;
            case 1: // Persigue al jugador
                {
                    if (agent != null)
                    {
                        agent.SetDestination(target.position);
                        isMoving = agent.velocity.magnitude > 0.1f;
                    }
                    else
                    {
                        Vector3 direction = (target.position - transform.position).normalized;
                        if (direction != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(direction);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                        }
                        transform.position += transform.forward * stats.movementSpeed * Time.deltaTime;
                        isMoving = true;
                    }
                }
                break;
            case 2: // Huye del jugador
                {
                    if (agent != null)
                    {
                        Vector3 fleeDirection = (transform.position - target.position).normalized;
                        Vector3 desiredPosition = transform.position + fleeDirection * 10f;
                        NavMeshHit hit;
                        if (NavMesh.SamplePosition(desiredPosition, out hit, 10f, NavMesh.AllAreas))
                        {
                            agent.SetDestination(hit.position);
                        }
                        isMoving = agent.velocity.magnitude > 0.1f;
                    }
                    else
                    {
                        Vector3 fleeDirection = (transform.position - target.position).normalized;
                        if (fleeDirection != Vector3.zero)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(fleeDirection);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
                        }
                        transform.position += transform.forward * stats.movementSpeed * Time.deltaTime;
                        isMoving = true;
                    }
                }
                break;
            default:
                break;
        }

        // Activar la animación de correr si el enemigo se está moviendo
        if (animator != null)
            animator.SetBool("IsRunning", isMoving);
    }

    /// <summary>
    /// Ataca al jugador si está en rango, infligiendo daño y aplicando efectos únicos.
    /// </summary>
    void Atacar()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= stats.attackRange && Time.time >= nextAttackTime)
        {
            totalDamageInflicted += stats.attackDamage;

            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (animator != null)
                    animator.SetTrigger("AreaAttackTriggerA");

                playerHealth.TakeDamage((int)stats.attackDamage);
                Debug.Log("Enemigo ataca al jugador, causando " + stats.attackDamage + " de daño.");

                if (uniqueEffect != UniqueEffect.None)
                {
                    playerHealth.ApplyUniqueEffect(uniqueEffect);
                }
            }
            nextAttackTime = Time.time + stats.attackRate;
        }
    }

    /// <summary>
    /// Aplica daño al enemigo y verifica si debe morir.
    /// </summary>
    public void RecibirDaño(float daño)
    {
        stats.currentHP -= daño;
        Debug.Log("Enemigo ha recibido " + daño + " puntos de daño. Vida restante: " + stats.currentHP);

        if (stats.currentHP <= 0)
            Morir();
    }

    /// <summary>
    /// Destruye el objeto enemigo.
    /// </summary>
    void Morir()
    {
        Destroy(gameObject);
    }
}
