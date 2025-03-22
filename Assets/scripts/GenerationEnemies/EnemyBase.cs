using UnityEngine;
using UnityEngine.AI; // Necesario para usar NavMeshAgent

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
        currentHP = maxHP;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            target = playerObj.transform;
        else
            Debug.LogWarning("No se encontró el objeto con tag 'Player'.");

        if (rend == null)
        {
            // Busca un Renderer en el objeto o sus hijos
            rend = GetComponentInChildren<Renderer>();
            if (rend == null)
                Debug.LogWarning("No se encontró un Renderer en el enemigo.");
        }

        // Intentar obtener el componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = moveSpeed;

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
        maxHP = hp;
        attackPower = atk;
        attackRate = rate;
        moveSpeed = speed;
        tipoMovimiento = movimiento;
        uniqueEffect = effect;
        currentHP = maxHP;

        if (agent != null)
            agent.speed = moveSpeed;
    }

    /// <summary>
    /// Establece la dificultad calculada y cambia el color según su valor.
    /// - Mayor a 100: rojo
    /// - Mayor a 50: amarillo
    /// - 50 o menor: verde
    /// </summary>
    public void SetDificultad(float valor)
    {
        dificultadCalculada = valor;
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
    /// Movimiento del enemigo según su tipo:
    /// - 0: No se mueve.
    /// - 1: Persigue al jugador.
    /// - 2: Huye del jugador usando NavMesh (si está disponible).
    /// Además, activa la animación de correr si el enemigo se está moviendo.
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
                        transform.position += transform.forward * moveSpeed * Time.deltaTime;
                        isMoving = true;
                    }
                }
                break;
            case 2: // Huye del jugador
                {
                    if (agent != null)
                    {
                        // Calcular la dirección de huida (contraria a la posición del jugador)
                        Vector3 fleeDirection = (transform.position - target.position).normalized;
                        // Definir una posición deseada a 10 unidades de distancia
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
                        transform.position += transform.forward * moveSpeed * Time.deltaTime;
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
    /// Además, activa la animación de ataque.
    /// </summary>
    void Atacar()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            totalDamageInflicted += attackPower;

            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Activar animación de ataque
                if (animator != null)
                    animator.SetTrigger("AreaAttackTriggerA");

                playerHealth.TakeDamage((int)attackPower);
                Debug.Log("Enemigo ataca al jugador, causando " + attackPower + " de daño.");

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
        Debug.Log("Enemigo ha recibido " + daño + " puntos de daño. Vida restante: " + currentHP);

        if (currentHP <= 0)
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
