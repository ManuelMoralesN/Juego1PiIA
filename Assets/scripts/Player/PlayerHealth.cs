using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int health = 100;
    private Animator animator;
    private bool isDead = false;

    private UIManager uiManager;

    [Header("Inmortalidad")]
    public bool isImmortal = false; // Nueva variable para controlar si el jugador es inmortal

    [Header("Efecto de Daño")]
    public Renderer playerRenderer;
    public Color damageColor = Color.red;
    public float flashDuration = 0.2f;

    [Header("Audio Settings")]
    public AudioClip damageSound;
    public AudioSource audioSource;

    private Color originalColor;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError("BaseEnemy: No se encontró un AudioSource en el enemigo.");
        }

        animator = GetComponent<Animator>();
        uiManager = FindObjectOfType<UIManager>();

        if (playerRenderer == null)
        {
            playerRenderer = GetComponentInChildren<Renderer>();
        }
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
        else
        {
            Debug.LogError("PlayerHealth: No se encontró un Renderer asignado.");
        }
    }

    void Update()
    {
        // Alternar inmortalidad con la tecla U
        if (Input.GetKeyDown(KeyCode.U))
        {
            isImmortal = !isImmortal;
            Debug.Log("Modo Inmortal: " + (isImmortal ? "Activado" : "Desactivado"));
        }
    }

    public void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        PlaySound(damageSound);

        // Mostrar el daño aunque sea inmortal
        Debug.Log("Jugador recibió daño. Daño: " + damage);

        // Solo reducir salud si NO es inmortal
        if (!isImmortal)
        {
            health -= damage;
            Debug.Log("Salud restante: " + health);
        }

        StartCoroutine(FlashDamageEffect());

        if (health <= 0 && !isImmortal)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log("El jugador ha muerto");

        animator.SetTrigger("Die");
        GetComponent<PlayerMovement>().enabled = false;

        uiManager.ShowGameOver();
    }

    private IEnumerator FlashDamageEffect()
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = damageColor;
            yield return new WaitForSeconds(flashDuration);
            playerRenderer.material.color = originalColor;
        }
    }
}
