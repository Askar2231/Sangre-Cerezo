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

    // UI y otros
    public Canvas battleCanvas;
    public ScreenFader fader;

    private bool battleIsActive = false;
    private Coroutine activeAttackRoutine; // referencia al ataque en curso

    public bool IsStaggered = false;
    public int StaggerRN = 0;

    public void StartBattle()
    {
        playerCombat.enabled = true;

        savedSnapshot = SavePlayerState(player);
        StartCoroutine(StartBattleRoutine());
    }

    IEnumerator StartBattleRoutine()
    {
        currentEnemy = enemyToLoad;

        // Reiniciar stagger al iniciar la batalla
        enemyLoader.ResetCurrentStagger();

        yield return StartCoroutine(fader.FadeOut(0.5f));
        yield return new WaitForSeconds(2f);

        if (enemyLoader != null && enemyToLoad != null)
        {
            enemyLoader.LoadEnemy(enemyToLoad);
            enemyLoader.battleManager = this; // <--- vincula manager

            Debug.Log("La batalla ha comenzado contra " + enemyToLoad.enemyName);
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
        player.GetComponent<MovimientoBasico>().enabled = false;

        yield return StartCoroutine(fader.FadeIn(0.5f));

        StartCoroutine(EnemyAttackLoop());
    }

    IEnumerator EnemyAttackLoop()
    {
        while (battleIsActive)
        {
            float waitTime = Random.Range(6f, 9f);
            yield return new WaitForSeconds(waitTime);
            EnemyAttack();
        }
    }

    public void EnemyAttack()
    {
        if (!IsStaggered && currentEnemy != null && currentEnemy.attacks.Count > 0)
        {
            int index = Random.Range(0, currentEnemy.attacks.Count);
            AttackData chosenAttack = currentEnemy.attacks[index];
            Debug.Log(currentEnemy.enemyName + " prepara " + chosenAttack.attackName);

            // guarda la referencia al ataque activo
            activeAttackRoutine = StartCoroutine(EnemyAttackRoutine(chosenAttack));
        }
    }

    IEnumerator EnemyAttackRoutine(AttackData attack)
    {
        Vector3 originalPos = enemyLoader.transform.position;

        // calcular posición frente al jugador (1.5 unidades de distancia)
        Vector3 dir = (player.transform.position - enemyLoader.transform.position).normalized;
        Vector3 targetPos = player.transform.position - dir * 1.5f;

        // 1. Avanzar hacia jugador
        float t = 0f;
        while (t < 0.3f)
        {
            enemyLoader.transform.position = Vector3.Lerp(originalPos, targetPos, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        enemyLoader.transform.position = targetPos;

        // 2. Esperar un segundo (anticipación)
        yield return new WaitForSeconds(1f);

        // 3. Activar parry window
        parryWindowActive = true;
        parryWindowEndTime = Time.time + attack.windUpTime;
        Debug.Log("¡Parry Window activa!");

        // 4. Esperar windup
        yield return new WaitForSeconds(attack.windUpTime);

        if (parryWindowActive)  // <--- aquí deberías chequear además que NO se haya cancelado
        {
            Debug.Log("El ataque de " + attack.attackName + " impacta al jugador.");
            playerCombat.TakeDamage(attack.damage);
            playerCombat.GainEnergy(2);
        }


        // 5. Resetear estado y volver al lugar original
        parryWindowActive = false;
        t = 0f;
        while (t < 0.3f)
        {
            enemyLoader.transform.position = Vector3.Lerp(targetPos, originalPos, t / 0.3f);
            t += Time.deltaTime;
            yield return null;
        }
        enemyLoader.transform.position = originalPos;

        activeAttackRoutine = null; // ataque completado
    }

    public void EnergyGive()
    {
        if (playerCombat != null)
        {
            playerCombat.GainEnergy(2);
        }
        else
        {
            Debug.LogWarning("No se encontró PlayerCombat en el BattleManager.");
        }
    }

    public void TryDodge()
    {
        Debug.Log("Jugador intenta esquivar.");
    }

    public void TryParry()
    {
        if (parryWindowActive && Time.time <= parryWindowEndTime)
        {
            Debug.Log("¡Parry exitoso!");
            parryWindowActive = false;
            parryWindowEndTime = 0f;   // fuerza cierre total de la ventana

            if (enemyLoader != null)
            {
                enemyLoader.ReduceStagger(5);
            }
            else if (currentEnemy != null)
            {
                // fallback (no recomendado): modifica asset solo en caso extremo
                currentEnemy.Stagger = Mathf.Max(0, currentEnemy.Stagger - 5);
                Debug.LogWarning("EnemyLoader no asignado, modificando EnemyData.Stagger (no recomendado).");
            }

            // cancelar ataque en curso
            if (activeAttackRoutine != null)
            {
                StopCoroutine(activeAttackRoutine);
                activeAttackRoutine = null;
            }

            // volver a posición inicial
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



    public void Clash()
    {
        Debug.Log("Ataques chocan en el mismo instante.");
    }

    public void EndBattle()
    {
        battleIsActive = false;
        StopAllCoroutines();
        playerCombat.enabled = false;

        battleCanvas.gameObject.SetActive(false);
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        movimiento.enabled = true;
        OVCamera.SetActive(true);
        BattleCamera.SetActive(false);

        Debug.Log("Combate finalizado.");
        RestorePlayerState(player, savedSnapshot);
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
            return;
        }

        if (currentEnemy == null) return;

        StaggerRN = currentEnemy.Stagger;
        IsStaggered = (StaggerRN <= 0);
    }



    public EnemyData GetCurrentEnemy()
    {
        return currentEnemy;
    }

    public bool IsBattleActive()
    {
        return battleIsActive;
    }
}









