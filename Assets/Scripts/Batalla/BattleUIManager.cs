using UnityEngine;
using TMPro;

public class BattleUIManager : MonoBehaviour
{
    [Header("Raíz del Canvas de batalla")]
    public GameObject battleUIRoot; 

    [Header("Referencias a UI")]
    public TMP_Text vidaJugadorText;
    public TMP_Text energiaJugadorText;
    public TMP_Text vidaEnemigoText;
    public TMP_Text staggerEnemigoText;
    public TMP_Text battleLogText;

    [Header("Referencias a Datos")]
    public PlayerCombat playerCombat;
    public EnemyLoader enemyLoader;

    // Singleton estático
    private static BattleUIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 👉 Si el Canvas está apagado, no actualizar nada
        if (battleUIRoot == null || !battleUIRoot.activeSelf) return;

        if (playerCombat != null && playerCombat.playerData != null)
        {
            vidaJugadorText.text = "HP: " + playerCombat.playerData.currentHealth;
            energiaJugadorText.text = "Energía: " + playerCombat.playerData.currentEnergy;
        }

        if (enemyLoader != null)
        {
            vidaEnemigoText.text = "HP: " + enemyLoader.GetCurrentHealth();
            staggerEnemigoText.text = "Stagger: " + enemyLoader.GetCurrentStagger();
        }
    }

    // 👉 Método estático para mostrar logs en pantalla (solo último mensaje)
    public static void LogMessage(string message)
    {
        if (instance != null && instance.battleLogText != null)
        {
            instance.battleLogText.text = message;
        }

        Debug.Log("[BattleLog] " + message);
    }

    // 👉 Mostrar / ocultar la UI de batalla
    public void ShowUI(bool state)
    {
        if (battleUIRoot != null)
            battleUIRoot.SetActive(state);
    }
}


