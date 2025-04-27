using UnityEngine;
using TMPro;

public class EnemyStatsDisplay : MonoBehaviour
{
    [Header("Referencias de UI")]
    public EnemyGenerator enemyGenerator;  // Referencia al generador de enemigos
    public GameObject infoPanel;            // Panel de UI que contiene la información
    public TMP_Text infoText;               // Componente TMP_Text para mostrar las estadísticas

    [Header("Configuración")]
    public KeyCode toggleKey = KeyCode.Tab; // Tecla para mostrar/ocultar el panel

    private bool isVisible = true;

    void Update()
    {
        // Alterna la visibilidad del panel al presionar la tecla asignada
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            if (infoPanel != null)
                infoPanel.SetActive(isVisible);
        }

        // Obtener información del enemigo
        string infoEnemigo = "";
        if (enemyGenerator != null && enemyGenerator.CurrentEnemy != null)
        {
            PcgEnemy enemigo = enemyGenerator.CurrentEnemy;  // Accedemos al enemigo actual desde EnemyGenerator
            if (enemigo != null)
            {
                // Convertir el tipo de movimiento en texto descriptivo
                string estadoMovimiento = "";
                switch (enemigo.tipoMovimiento)  // Acceder a tipoMovimiento desde EnemyBase
                {
                    case 0:
                        estadoMovimiento = "Quieto";
                        break;
                    case 1:
                        estadoMovimiento = "Persiguiendo";
                        break;
                    case 2:
                        estadoMovimiento = "Huyendo";
                        break;
                    default:
                        estadoMovimiento = "Desconocido";
                        break;
                }

                // Se utiliza attackDamage como "Daño por golpe"
                float dañoPorGolpe = enemigo.stats.attackDamage;

                // Componer la información del enemigo
                infoEnemigo = $"<b><size=18>Estadísticas del Enemigo</size></b>\n" +
                              $"<color=white><b>Vida:</b></color> {enemigo.stats.maxHP:F1}\n" +
                              $"<color=white><b>Daño:</b></color> {enemigo.stats.attackDamage:F1}\n" +
                              $"<color=white><b>Velocidad de Ataque:</b></color> {enemigo.stats.attackRate:F2}\n" +
                              $"<color=white><b>Velocidad:</b></color> {enemigo.stats.movementSpeed:F1}\n" +
                              $"<color=white><b>Dificultad:</b></color> {enemigo.stats.difficultyValue:F2}\n" +
                              $"<color=white><b>Efecto Único:</b></color> {enemigo.uniqueEffect}\n" +  // Acceder a uniqueEffect desde EnemyBase
                              $"<color=white><b>Movimiento:</b></color> {estadoMovimiento}\n" +
                              $"<color=white><b>Daño por golpe:</b></color> {dañoPorGolpe:F1}\n" +
                              $"<color=white><b>Daño Total:</b></color> {enemigo.totalDamageInflicted:F1}\n";
            }
        }
        else
        {
            infoEnemigo = "<i>No se generó ningún enemigo.</i>\n";
        }

        // Obtener información del jugador (PlayerHealth)
        string infoJugador = "";
        PlayerHealth jugador = FindObjectOfType<PlayerHealth>();
        if (jugador != null)
        {
            infoJugador = $"<b><size=18>Estadísticas del Jugador</size></b>\n" +
                          $"<color=white><b>Vida:</b></color> {jugador.health:F1}\n" +
                          $"<color=white><b>Modo Inmortal:</b></color> {(jugador.isImmortal ? "Sí" : "No")}\n";
        }
        else
        {
            infoJugador = "<i>No se encontró al jugador.</i>\n";
        }

        // Combinar la información del enemigo y del jugador y actualizar el texto del panel
        infoText.text = infoEnemigo + "\n" + infoJugador;
    }

}
