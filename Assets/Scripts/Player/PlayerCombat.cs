using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerData playerData;
    public AbilityData[] abilities;
    public float globalCooldown = 0.5f;
    public BattleManager battleManager;

    private float[] lastUsedTime;

    void Start()
    {
        if (playerData != null)
        {
            playerData.currentHealth = playerData.maxHealth;
            playerData.currentEnergy = playerData.maxEnergy;
        }

        int len = (abilities != null) ? abilities.Length : 0;
        lastUsedTime = new float[len];
        for (int i = 0; i < len; i++) lastUsedTime[i] = -9999f;

        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManager>();
    }

    void Update()
{
    if (battleManager != null && battleManager.IsBattleActive())
    {
        // Input de habilidades
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseAbility(0);

        // Input de parry
        if (Input.GetKeyDown(KeyCode.Space))
            battleManager.TryParry();
    }
}

    public void GainEnergy(int amount)
    {
        if (playerData == null) return;
        playerData.currentEnergy = Mathf.Min(playerData.currentEnergy + amount, playerData.maxEnergy);
        Debug.Log("El jugador gana " + amount + " de energía. Energía actual: " + playerData.currentEnergy);
    }

    public void UseAbility(int index, EnemyLoader enemyLoader)
    {
        if (playerData == null) return;
        if (abilities == null || index < 0 || index >= abilities.Length) return;

        AbilityData ability = abilities[index];

        if (playerData.currentEnergy < ability.energyCost)
        {
            Debug.Log("No hay energía suficiente para usar " + ability.abilityName);
            return;
        }

        if (Time.time - lastUsedTime[index] < globalCooldown)
        {
            Debug.Log("Habilidad en cooldown.");
            return;
        }

        playerData.currentEnergy -= ability.energyCost;
        lastUsedTime[index] = Time.time;

        if (ability.isDefensive)
        {
            Debug.Log("Jugador usa " + ability.abilityName + " (defensiva).");
            return;
        }

        if (enemyLoader != null)
        {
            enemyLoader.TakeDamage(ability.damage);
            Debug.Log("El jugador usa " + ability.abilityName +
                      " e inflige " + ability.damage + " al enemigo.");
        }
        else
        {
            Debug.Log("El jugador usa " + ability.abilityName +
                      " pero no hay enemigo objetivo (EnemyLoader).");
        }
    }

    public void UseAbility(int index)
    {
        EnemyLoader el = null;
        if (battleManager != null) el = battleManager.enemyLoader;
        if (el == null)
        {
            BattleManager bm = FindFirstObjectByType<BattleManager>();
            if (bm != null)
            {
                battleManager = bm;
                el = bm.enemyLoader;
            }
        }
        UseAbility(index, el);
    }

    public void TakeDamage(int damage)
    {
        if (playerData == null) return;

        int finalDamage = Mathf.Max(damage - playerData.defense, 0);
        playerData.currentHealth -= finalDamage;
        Debug.Log("El jugador recibe " + finalDamage + " de daño. Vida actual: " + playerData.currentHealth);

        if (playerData.currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("El jugador ha muerto.");
        if (battleManager == null) battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager != null) battleManager.EndBattle();
    }
}


