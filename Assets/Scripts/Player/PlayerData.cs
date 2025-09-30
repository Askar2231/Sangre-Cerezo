using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "RPG/Player Data")]
public class PlayerData : ScriptableObject
{
    public string playerName = "Heroe";

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    // ✅ Energía ahora de 1 a 6
    public int maxEnergy = 6;
    public int currentEnergy;

    public int attackPower = 10;
    public int defense = 5;

    [Header("Combate especial")]
    public float dodgeChance = 0.1f;
    public float parryWindow = 0.2f;

    [Header("Habilidades del jugador (4 slots fijos)")]
    public AbilityData[] abilities = new AbilityData[4]; 
    // Si un slot está vacío, quedará como null.

    public void ResetStats()
    {
        currentHealth = maxHealth;
        currentEnergy = maxEnergy;
    }
}



