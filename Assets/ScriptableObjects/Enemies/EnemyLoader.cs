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

    public BattleManager battleManager;
    private int currentHealth;
    private int currentStagger;


    public void LoadEnemy(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogError("[EnemyLoader] LoadEnemy called with null EnemyData!");
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

        // Validaciones de datos
        if (enemyData.prefab == null)
        {
            Debug.LogError($"[EnemyLoader] EnemyData '{enemyData.enemyName}' no tiene prefab asignado!");
            return;
        }

        //cargar las stats
        currentHealth = enemyData.Health;
        currentStagger = enemyData.Stagger;


        Vector3 pos = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        // Instancia el prefab como hijo del spawnPoint para mantener orden en la jerarquía
        enemyModel = Instantiate(enemyData.prefab, pos, rot, spawnPoint);

        if (enemyModel == null)
        {
            Debug.LogError("[EnemyLoader] Falló Instantiate por alguna razón.");
            return;
        }

        enemyModel.SetActive(true);
        enemyModel.name = $"{enemyData.enemyName}_Instance";

        // Buscar el animator
        enemyAnimator = enemyModel.GetComponent<Animator>();
        if (enemyAnimator == null)
            enemyAnimator = enemyModel.GetComponentInChildren<Animator>();

        if (enemyAnimator == null)
            Debug.LogWarning($"[EnemyLoader] No se encontró Animator en prefab '{enemyData.prefab.name}'. Asegúrate que tenga un Animator o maneja animaciones manualmente.");

        Debug.Log($"[EnemyLoader] Instanciado '{enemyModel.name}' en {pos}. HP: {currentHealth}, Stagger: {currentStagger}");
    }


    public int GetCurrentHealth() => currentHealth;
    public int GetCurrentStagger() => currentStagger;
    public string GetEnemyName() => enemyData != null ? enemyData.enemyName : "NoEnemy";

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
        Debug.Log($"[EnemyLoader] {GetEnemyName()} recibe {amount} de daño. HP restante: {currentHealth}");
        if (currentHealth <= 0) OnDeath();
    }


    public void ResetCurrentStagger()
    {
        currentStagger = (enemyData != null) ? enemyData.Stagger : 0;
    }


    public void SetCurrentStagger(int value)
    {
        currentStagger = value;
    }


    [Header("Stagger Settings")]
    public float staggerRecoveryTime = 3f; // tiempo que dura stagger (puedes tunearlo)

    private bool isStaggered = false;


    public void ReduceStagger(int amount)
    {
        currentStagger -= amount;
        if (currentStagger < 0) currentStagger = 0;

        Debug.Log($"[EnemyLoader] {GetEnemyName()} pierde {amount} de stagger. Stagger restante: {currentStagger}");

        if (currentStagger == 0 && !isStaggered)
        {
            StartCoroutine(HandleStagger());
        }
    }


    private IEnumerator HandleStagger()
    {
        isStaggered = true;
        Debug.Log($"[EnemyLoader] {GetEnemyName()} está STAGGERED!");

        // Aquí podrías notificar al BattleManager (ej: que detenga ataques).
        yield return new WaitForSeconds(staggerRecoveryTime);

        ResetCurrentStagger(); // vuelve a la cantidad base definida en EnemyData
        isStaggered = false;
        Debug.Log($"[EnemyLoader] {GetEnemyName()} se recuperó del stagger. Stagger restaurado a {currentStagger}");
    }





    void OnDeath()
    {
        Debug.Log($"[EnemyLoader] {GetEnemyName()} ha muerto.");
        if (battleManager != null)
        {
            battleManager.EndBattle();
        }
        else
        {
            Debug.LogWarning("No se asignó BattleManager en EnemyLoader.");
        }
    }
}

