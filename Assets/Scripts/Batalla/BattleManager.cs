using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    // Player
    private bool parryWindowActive = false;
    private float parryWindowEndTime;

    public GameObject player;
    private PlayerSnapshot savedSnapshot;
    public Transform tpPlayer;

    public PlayerCombat playerCombat;

    // Cámaras
    public GameObject OVCamera;
    public GameObject BattleCamera;

    // Enemigo
    public GameObject enemy;
    public EnemyLoader enemyLoader;
    private EnemyData currentEnemy;
    public EnemyData enemyToLoad;

    // UI
    public Canvas battleCanvas;
    public ScreenFader fader;
    public BattleUIManager uiManager;

    private bool battleIsActive = false;
    private Coroutine activeAttackRoutine;

    public bool IsStaggered = false;
    public int StaggerRN = 0;

    [Header("Parry Indicator")]
    public GameObject parryIndicatorPrefab;
    private GameObject activeParryIndicator;



     public bool IsBattleActive()
    {
        return battleIsActive;
    }

    public void StartBattle()
    {
        playerCombat.enabled = true;
        if (uiManager != null)
            uiManager.ShowUI(true);

        savedSnapshot = SavePlayerState(player);
        StartCoroutine(StartBattleRoutine());
    }

    IEnumerator StartBattleRoutine()
    {
        currentEnemy = enemyToLoad;

        enemyLoader.ResetCurrentStagger();

        yield return StartCoroutine(fader.FadeOut(0.5f));
        yield return new WaitForSeconds(2f);

        if (enemyLoader != null && enemyToLoad != null)
        {
            enemyLoader.LoadEnemy(enemyToLoad);
            BattleUIManager.LogMessage("La batalla ha comenzado contra " + enemyToLoad.enemyName);
        }
        else
        {
            Debug.LogError("No se asignó EnemyLoader o EnemyData en el inspector.");
        }

        BattleCamera.SetActive(true);
        OVCamera.SetActive(false);
        TeleportPlayerToBattle();

        battleIsActive = true;
        battleCanvas.gameObject.SetActive(true);
        if (uiManager != null) uiManager.gameObject.SetActive(true);

        player.GetComponent<MovimientoBasico>().enabled = false;

        yield return StartCoroutine(fader.FadeIn(0.5f));

        StartCoroutine(TurnBasedLoop());
    }

    IEnumerator TurnBasedLoop()
    {
        while (battleIsActive)
        {
            // TURNO DEL JUGADOR
            yield return PlayerTurn();
            if (!battleIsActive) break;

            // TURNO DEL ENEMIGO
            yield return EnemyTurn();
            if (!battleIsActive) break;
        }
    }

    IEnumerator PlayerTurn()
    {
        Debug.Log("Turno del jugador");

        // Daño directo temporal
        if (enemyLoader != null && currentEnemy != null)
        {
            int damage = 5;
            enemyLoader.TakeDamage(damage);
            BattleUIManager.LogMessage("Atacas al enemigo por " + damage);
        }

        yield return new WaitForSeconds(1f);
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Turno del enemigo");

        if (!IsStaggered && currentEnemy != null && currentEnemy.attacks.Count > 0)
        {
            int index = Random.Range(0, currentEnemy.attacks.Count);
            AttackData chosenAttack = currentEnemy.attacks[index];
            BattleUIManager.LogMessage(currentEnemy.enemyName + " usa " + chosenAttack.attackName);

            yield return StartCoroutine(EnemyAttackRoutine(chosenAttack));
        }
        else
        {
            Debug.Log("El enemigo está staggered o sin ataques");
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator EnemyAttackRoutine(AttackData attack)
    {
        // Anticipación
        yield return new WaitForSeconds(1f);

        // Activar ventana de parry
        parryWindowActive = true;
        parryWindowEndTime = Time.time + attack.windUpTime;
        ShowParryIndicator(attack.windUpTime);

        // Esperar duración del windUp
        yield return new WaitForSeconds(attack.windUpTime);

        // Si no se hizo parry, daña al jugador
        if (parryWindowActive)
        {
            playerCombat.TakeDamage(attack.damage);
            playerCombat.GainEnergy(2);
        }

        // Cerrar ventana de parry
        parryWindowActive = false;

        if (activeParryIndicator != null)
        {
            Destroy(activeParryIndicator);
        }

        activeAttackRoutine = null;
    }

    public void TryParry()
    {
        if (parryWindowActive && Time.time <= parryWindowEndTime)
        {
            BattleUIManager.LogMessage("¡Parry exitoso!");

            if (activeParryIndicator != null)
                Destroy(activeParryIndicator);

            parryWindowActive = false;
            EnergyGive();

            if (enemyLoader != null)
            {
                enemyLoader.ReduceStagger(5);

                if (enemyLoader.GetCurrentStagger() == 0)
                {
                    BattleUIManager.LogMessage(enemyLoader.GetEnemyName() + " está STAGGEREADO!");
                }
            }

            if (activeAttackRoutine != null)
            {
                StopCoroutine(activeAttackRoutine);
                activeAttackRoutine = null;
            }

            if (enemyLoader != null && enemyLoader.spawnPoint != null)
            {
                enemyLoader.transform.position = enemyLoader.spawnPoint.position;
            }
        }
        else
        {
            Debug.Log("Parry fallido.");
        }
    }

    public void EnergyGive()
    {
        if (playerCombat != null)
        {
            playerCombat.GainEnergy(2);
        }
    }

    public void EndBattle()
    {
        battleIsActive = false;
        StopAllCoroutines();
        playerCombat.enabled = false;

        battleCanvas.gameObject.SetActive(false);
        if (uiManager != null) uiManager.gameObject.SetActive(false);

        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        movimiento.enabled = true;
        OVCamera.SetActive(true);
        BattleCamera.SetActive(false);

        BattleUIManager.LogMessage("Combate finalizado.");

        RestorePlayerState(player, savedSnapshot);

        if (uiManager != null)
            uiManager.ShowUI(false);
    }

    private void Update()
    {
        CheckStagger();
    }

    private void CheckStagger()
    {
        if (enemyLoader != null)
        {
            StaggerRN = enemyLoader.GetCurrentStagger();
            IsStaggered = (StaggerRN <= 0);
        }
    }

    public PlayerSnapshot SavePlayerState(GameObject player)
    {
        PlayerSnapshot snapshot = new PlayerSnapshot();

        snapshot.position = player.transform.position;
        snapshot.rotation = player.transform.rotation;
        snapshot.parent = player.transform.parent;

        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        if (movimiento != null)
        {
            snapshot.movementEnabled = movimiento.enabled;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            snapshot.rbIsKinematic = rb.isKinematic;
            snapshot.rbVelocity = rb.linearVelocity;
        }

        return snapshot;
    }

    public void RestorePlayerState(GameObject player, PlayerSnapshot snapshot)
    {
        player.transform.SetParent(snapshot.parent);
        player.transform.position = snapshot.position;
        player.transform.rotation = snapshot.rotation;

        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        if (movimiento != null)
        {
            movimiento.enabled = snapshot.movementEnabled;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = snapshot.rbIsKinematic;
            rb.linearVelocity = snapshot.rbVelocity;
        }
    }

    public void TeleportPlayerToBattle()
    {
        if (tpPlayer != null && player != null)
        {
            player.transform.position = tpPlayer.position;
            player.transform.rotation = tpPlayer.rotation;
        }
    }

    public EnemyData GetCurrentEnemy()
    {
        return currentEnemy;
    }

    
    private void ShowParryIndicator(float duration)
    {
        if (parryIndicatorPrefab == null || enemyLoader == null) return;

        if (activeParryIndicator != null)
        {
            Destroy(activeParryIndicator);
        }

        Vector3 spawnPos = enemyLoader.transform.position + Vector3.up * 2f;
        Quaternion rot = parryIndicatorPrefab.transform.rotation * Quaternion.Euler(0, 180, 0);

        activeParryIndicator = Instantiate(parryIndicatorPrefab, spawnPos, rot, enemyLoader.transform);

        activeParryIndicator.AddComponent<FollowTarget>().Init(enemyLoader.transform, Vector3.up * 2f);

        StartCoroutine(AnimateParryIndicator(activeParryIndicator.transform, duration));
    }

    private IEnumerator AnimateParryIndicator(Transform indicator, float duration)
    {
        float half = duration / 2f;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float scale = (timer < half)
                ? Mathf.Lerp(0.2f, 1.2f, timer / half)
                : Mathf.Lerp(1.2f, 0.0f, (timer - half) / half);

            indicator.localScale = Vector3.one * scale;

            yield return null;
        }

        Destroy(indicator.gameObject);
    }
}
