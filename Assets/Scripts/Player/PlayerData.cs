using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "RPG/Player Data")]
public class PlayerData : ScriptableObject
{
    public string playerName = "Heroe";

    [Header("Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    public int maxEnergy = 50;
    public int currentEnergy;

    public int attackPower = 10;
    public int defense = 5;

    [Header("Combate especial")]
    public float dodgeChance = 0.1f; // 10% por ejemplo
    public float parryWindow = 0.2f; // segundos de ventana de parry
}

