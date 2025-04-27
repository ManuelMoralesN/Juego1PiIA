using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Referencias de UI")]
    public GameObject gameOverUI;  // Canvas de Game Over
    public GameObject victoryUI;   // Canvas de Victoria

    [Header("Configuración de Delays")]
    public float delayBeforeGameOver = 2f;  // Tiempo en segundos antes de mostrar Game Over
    public float delayBeforeVictory = 2f;   // Tiempo en segundos antes de mostrar Victoria

    private void Start()
    {
        // Verificar que las referencias de UI estén asignadas
        if (gameOverUI == null) Debug.LogError("UIManager: No hay un Game Over UI asignado.");
        if (victoryUI == null) Debug.LogError("UIManager: No hay un Victory UI asignado.");

        // Asegurarse de que las pantallas estén desactivadas al inicio
        gameOverUI?.SetActive(false);
        victoryUI?.SetActive(false);
    }

    /// <summary>
    /// Muestra la pantalla de Game Over.
    /// </summary>
    public void ShowGameOver()
    {
        StartCoroutine(ShowGameOverUICoroutine());
    }

    private IEnumerator ShowGameOverUICoroutine()
    {
        yield return new WaitForSecondsRealtime(delayBeforeGameOver);

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            yield return new WaitForSecondsRealtime(0.2f); // Pequeño delay para que se dibuje la UI
            Time.timeScale = 0f; // Congelar el juego
        }
    }

    /// <summary>
    /// Muestra la pantalla de Victoria.
    /// </summary>
    public void ShowVictory()
    {
        StartCoroutine(ShowVictoryUICoroutine());
    }

    private IEnumerator ShowVictoryUICoroutine()
    {
        yield return new WaitForSecondsRealtime(delayBeforeVictory);

        if (victoryUI != null)
        {
            victoryUI.SetActive(true);
            yield return new WaitForSecondsRealtime(0.2f); // Pequeño delay para que se dibuje la UI
            Time.timeScale = 0f; // Congelar el juego
        }
    }

    /// <summary>
    /// Reinicia el nivel actual.
    /// </summary>
    public void RestartLevel()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo esté activo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recargar escena
    }

    /// <summary>
    /// Opcional: Salir del juego.
    /// </summary>
    public void QuitGame()
    {
        Time.timeScale = 1f; // Asegurar que el tiempo esté activo
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Para editor
#else
        Application.Quit(); // Para build
#endif
    }
}
