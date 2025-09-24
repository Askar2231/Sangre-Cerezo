using System.Collections;
using UnityEngine;

public class EnemyLoader : MonoBehaviour
{
    [Header("Datos (asignar en inspector o mediante LoadEnemy)")]
    public EnemyData enemyData;

    [Header("Spawn point (EnemyPlaceholderLoader)")]
    public Transform spawnPoint;

    private GameObject enemyModel;
    private Animator enemyAnimator;

    [Header("Referencias")]
    public BattleManager battleManager;

    private int currentHealth;
    private int currentStagger;

    [Header("Stagger Settings")]
    public float staggerRecoveryTime = 3f; // tiempo que dura el stagger
    private bool isStaggered = false;

    #region INIT
    public void LoadEnemy(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogError("[EnemyLoader] LoadEnemy llamado con null EnemyData!");
            return;
        }

        // limpiar instancias previas
        if (enemyModel != null)
        {
            Destroy(enemyModel);
            enemyModel = null;
            enemyAnimator = null;
        }

        enemyData = data;

        if (enemyData.prefab == null)
        {
            Debug.LogError($"[EnemyLoader] EnemyData '{enemyData.enemyName}' no tiene prefab asignado!");
            return;
        }

        // cargar stats iniciales
        currentHealth = enemyData.Health;
        currentStagger = enemyData.Stagger;

        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        // Instanciar modelo
        enemyModel = Instantiate(enemyData.prefab, pos, rot, spawnPoint);

        if (enemyModel == null)
        {
            Debug.LogError("[EnemyLoader] Falló Instantiate por alguna razón.");
            return;
        }

        enemyModel.SetActive(true);
        enemyModel.name = $"{enemyData.enemyName}_Instance";

        // Buscar animator
        enemyAnimator = enemyModel.GetComponent<Animator>();
        if (enemyAnimator == null)
            enemyAnimator = enemyModel.GetComponentInChildren<Animator>();

        if (enemyAnimator == null)
            Debug.LogWarning($"[EnemyLoader] No se encontró Animator en prefab '{enemyData.prefab.name}'.");

        Debug.Log($"[EnemyLoader] Instanciado '{enemyModel.name}' en {pos}. HP: {currentHealth}, Stagger: {currentStagger}");

        // Vincular BattleManager si no está
        if (battleManager == null)
            battleManager = FindFirstObjectByType<BattleManager>();
    }
    #endregion

    #region GETTERS
    public int GetCurrentHealth() => currentHealth;
    public int GetCurrentStagger() => currentStagger;
    public string GetEnemyName() => enemyData != null ? enemyData.enemyName : "NoEnemy";
    #endregion

    #region COMBATE
    public AttackData GetRandomAttack()
    {
        if (enemyData == null || enemyData.attacks == null || enemyData.attacks.Count == 0)
            return null;
        int i = Random.Range(0, enemyData.attacks.Count);
        return enemyData.attacks[i];
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        BattleUIManager.LogMessage($"[EnemyLoader] {GetEnemyName()} recibe {amount} de daño. HP restante: {currentHealth}");

        if (currentHealth <= 0) OnDeath();
    }

    public void ReduceStagger(int amount)
    {
        if (isStaggered) return; // no seguir reduciendo si ya está en stagger

        currentStagger -= amount;
        if (currentStagger < 0) currentStagger = 0;

        BattleUIManager.LogMessage($"[EnemyLoader] {GetEnemyName()} pierde {amount} de stagger. Stagger restante: {currentStagger}");

        if (currentStagger == 0)
            StartCoroutine(HandleStagger());
    }
    #endregion

    #region STAGGER
    private IEnumerator HandleStagger()
    {
        isStaggered = true;
        BattleUIManager.LogMessage($"[EnemyLoader] {GetEnemyName()} está STAGGERED!");

        // Aquí podrías notificar al BattleManager si quieres detener ataques, etc.
        yield return new WaitForSeconds(staggerRecoveryTime);

        ResetCurrentStagger();
        isStaggered = false;
        BattleUIManager.LogMessage($"[EnemyLoader] {GetEnemyName()} se recuperó del stagger. Stagger restaurado a {currentStagger}");
    }

    public void ResetCurrentStagger()
    {
        currentStagger = (enemyData != null) ? enemyData.Stagger : 0;
    }

    public void SetCurrentStagger(int value)
    {
        currentStagger = Mathf.Max(0, value);
    }
    #endregion

    #region MUERTE
    private void OnDeath()
    {
        BattleUIManager.LogMessage($"[EnemyLoader] {GetEnemyName()} ha muerto.");

        if (battleManager != null)
        {
            battleManager.EndBattle();
        }
        else
        {
            Debug.LogWarning("No se asignó BattleManager en EnemyLoader.");
        }
    }
    #endregion
}


