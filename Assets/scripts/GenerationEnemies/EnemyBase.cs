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
            // Busca un Renderer 
            rend = GetComponentInChildren<Renderer>();
            if (rend == null)
                Debug.LogWarning("No se encontró un Renderer en el enemigo.");
        }

        //NavMeshAgent
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
                        transform.position += transform.forward * moveSpeed * Time.deltaTime;
                        isMoving = true;
                    }
                }
                break;
            default:
                break;
        }

        if (animator != null)
            animator.SetBool("IsRunning", isMoving);
    }

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

    public void RecibirDaño(float daño)
    {
        currentHP -= daño;
        Debug.Log("Enemigo ha recibido " + daño + " puntos de daño. Vida restante: " + currentHP);

        if (currentHP <= 0)
            Morir();
    }

    void Morir()
    {
        Destroy(gameObject);
    }

    public float BalanceScore()
    {
        float normalizedSpeed = (moveSpeed - 1f) / (5f - 1f); // Asumiendo un rango de velocidad de 1 a 5
        float normalizedDamage = (attackPower - 5f) / (30f - 5f); // Asumiendo un rango de daño de 5 a 30

        return (normalizedSpeed + normalizedDamage) / 2;
    }

    public float TotalScore()
    {
        return dificultadCalculada * 0.7f + BalanceScore() * 0.3f;
    }
}
