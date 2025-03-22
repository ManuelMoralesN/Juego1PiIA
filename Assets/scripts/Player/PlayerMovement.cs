using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;  // Velocidad de caminar
    private float originalSpeed;  // Para restaurar la velocidad
    private Vector3 movement;
    private Animator animator;  // Referencia al Animator
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        originalSpeed = speed; // Guardamos la velocidad original
    }

    void Update()
    {
        // Capturar los inputs de movimiento
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Crear un vector de movimiento
        movement = new Vector3(horizontalInput, 0f, verticalInput);

        // Normalizar para mantener la velocidad consistente en las diagonales
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Determinar si el personaje está caminando
        bool Running = movement.magnitude > 0;
        animator.SetBool("Running", Running);

        // Rotar el personaje hacia la dirección del movimiento si hay input
        if (movement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // Activar la animación de disparo cuando se presiona la tecla F
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Shoot");
        }

        // Activar la animación de bailar cuando se presiona la tecla B
        if (Input.GetKeyDown(KeyCode.B))
        {
            animator.SetTrigger("Dance");
        }
    }

    void FixedUpdate()
    {
        // Mover el personaje usando el Rigidbody
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Aplica un efecto slow que reduce la velocidad del jugador.
    /// </summary>
    /// <param name="slowMultiplier">Factor de reducción (por ejemplo, 0.5 para la mitad de velocidad)</param>
    /// <param name="duration">Duración del efecto en segundos</param>
    public void ApplySlow(float slowMultiplier, float duration)
    {
        StartCoroutine(SlowCoroutine(slowMultiplier, duration));
    }

    private IEnumerator SlowCoroutine(float slowMultiplier, float duration)
    {
        // Reducir la velocidad a la velocidad original multiplicada por el slowMultiplier
        speed = originalSpeed * slowMultiplier;
        Debug.Log("Slow aplicado: velocidad reducida a " + speed);
        yield return new WaitForSeconds(duration);
        // Restaurar la velocidad original
        speed = originalSpeed;
        Debug.Log("Slow terminado: velocidad restaurada a " + speed);
    }
}
