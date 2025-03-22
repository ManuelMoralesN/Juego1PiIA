using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class EnemyStatsDisplay : MonoBehaviour
{
    [Header("Referencias de UI")]
    public EnemyGenerator enemyGenerator;  // Referencia al generador de enemigos
    public GameObject infoPanel;            // Panel de UI para mostrar la información
    public TMP_Text infoText;               // Componente TMP_Text para mostrar los stats

    [Header("Configuración")]
    public KeyCode toggleKey = KeyCode.Tab; // Tecla para mostrar/ocultar el panel

    private bool isVisible = true;

    void Update()
    {
        // Alternar visibilidad del panel
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
            if (infoPanel != null)
                infoPanel.SetActive(isVisible);
        }

        // Si hay un enemigo generado, actualizar la info
        if (enemyGenerator != null && enemyGenerator.CurrentEnemy != null)
        {
            EnemyBase enemy = enemyGenerator.CurrentEnemy.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                infoText.text = $"<b><size=21>Enemy Stats</size></b>\n" +
                                $"<color=white><b>HP:</b></color> {enemy.maxHP:F1}\n" +
                                $"<color=white><b>Attack:</b></color> {enemy.attackPower:F1}\n" +
                                $"<color=white><b>Attack Rate:</b></color> {enemy.attackRate:F2}\n" +
                                $"<color=white><b>Move Speed:</b></color> {enemy.moveSpeed:F1}\n" +
                                $"<color=white><b>Dificultad:</b></color> {enemy.dificultadCalculada:F2}\n" +
                                $"<color=white><b>Efecto Único:</b></color> {(enemy.efectoUnico == 1 ? "Sí" : "No")}";
            }
        }
        else
        {
            infoText.text = "<i>No enemy generated.</i>";
        }
    }
}
