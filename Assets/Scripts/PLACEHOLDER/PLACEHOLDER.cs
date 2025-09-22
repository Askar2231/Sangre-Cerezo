using UnityEngine;

public class PLACEHOLDER : MonoBehaviour
{
    public PlayerCombat playerCombat;

    void Start()
    {
        playerCombat = GetComponent<PlayerCombat>();
        if (playerCombat == null)
        {
            Debug.LogError("PlayerCombat component not found on the GameObject.");
        }
        else if (playerCombat.playerData != null)
        {
            int playerMaxHealth = playerCombat.playerData.maxHealth;
            int playerMaxEnergy = playerCombat.playerData.maxEnergy;
            int playerCurrentHealth = playerCombat.playerData.currentHealth;
            int playerCurrentEnergy = playerCombat.playerData.currentEnergy;

            Debug.Log("Vida máxima del jugador: " + playerMaxHealth);
            Debug.Log("Energía máxima del jugador: " + playerMaxEnergy);
            Debug.Log("Vida actual del jugador: " + playerCurrentHealth);
            Debug.Log("Energía actual del jugador: " + playerCurrentEnergy);
        }
        else
        {
            Debug.LogError("PlayerData not assigned in PlayerCombat.");
        }
    }

    void Update()
    {
        
    }
}
