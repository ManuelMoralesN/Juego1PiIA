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
    public bool isImmortal = false;
    public float invulnDuration = 1f;
    private bool isInvulnerable = false;

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
            Debug.LogError("PlayerHealth: No se encontró un AudioSource.");
        }
        animator = GetComponent<Animator>();

        // Intenta encontrar el UIManager en la escena
        uiManager = FindObjectOfType<UIManager>();
        if(uiManager == null)
        {
            Debug.LogWarning("PlayerHealth: No se encontró UIManager en la escena.");
        }

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
        if (isImmortal || isInvulnerable) return;

        PlaySound(damageSound);
        Debug.Log("Jugador recibió daño. Daño: " + damage);
        health -= damage;
        Debug.Log("Salud restante: " + health);
        StartCoroutine(InvulnerabilityCoroutine(invulnDuration));
        StartCoroutine(FlashDamageEffect());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator InvulnerabilityCoroutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }

    public void ApplyUniqueEffect(UniqueEffect effect)
    {
        switch (effect)
        {
            case UniqueEffect.Slow:
                Debug.Log("Jugador ralentizado!");
                PlayerMovement pm = GetComponent<PlayerMovement>();
                if (pm != null)
                {
                    pm.ApplySlow(0.5f, 3f);
                }
                break;
            case UniqueEffect.Burn:
                Debug.Log("Jugador en quemadura!");
                StartCoroutine(BurnCoroutine(5f, 3f));
                break;
            case UniqueEffect.Stun:
                Debug.Log("Jugador aturdido!");
                 StartCoroutine(StunCoroutine(1f)); 
                break;
            default:
                break;
        }
    }

    private IEnumerator BurnCoroutine(float burnDamage, float duration)
    {
        while (isInvulnerable)
        {
            yield return null;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            yield return new WaitForSeconds(1f);
            if (!isDead)
            {
                health -= (int)burnDamage;
                Debug.Log("Quemadura: se aplicaron " + burnDamage + " puntos de daño. Salud: " + health);
                if (health <= 0)
                {
                    Die();
                    yield break;
                }
            }
            elapsed += 1f;
        }
    }

    private IEnumerator StunCoroutine(float duration)
    {
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.enabled = false;
        }
        yield return new WaitForSeconds(duration);
        if (pm != null)
        {
            pm.enabled = true;
        }
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


    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("El jugador ha muerto");
        animator.SetTrigger("Die");
        GetComponent<PlayerMovement>().enabled = false;
        // MOSTRAR GAME OVER
    if (uiManager != null)
    {
        uiManager.ShowGameOver();
    }
    }

    /// <summary>
    /// Reinicia el estado del jugador para que aparezca vivo.
    /// </summary>
    public void ResetPlayer()
    {
        isDead = false;
        health = 100;
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.enabled = true;
        }

    }
}
