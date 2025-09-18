using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    // Player
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
        if (currentEnemy != null)
        {
            currentEnemy.Stagger = 10; // valor inicial
        }

        yield return StartCoroutine(fader.FadeOut(0.5f));
        yield return new WaitForSeconds(2f);

        if (enemyLoader != null && enemyToLoad != null)
        {
            enemyLoader.LoadEnemy(enemyToLoad);
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

            Debug.Log(currentEnemy.enemyName + " usa " + chosenAttack.attackName);

            // Aquí se aplica daño real al jugador
            playerCombat.TakeDamage(chosenAttack.damage);

            // Dar energía al jugador por cada ataque recibido
            EnergyGive();
        }
    }


    public void EnergyGive()
    {
        if (playerCombat != null)
        {
            playerCombat.GainEnergy(5); // ⚡ puedes ajustar el valor
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
        Debug.Log("Jugador intenta parrear.");
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
        if (currentEnemy == null) return;

        StaggerRN = currentEnemy.Stagger;
        IsStaggered = (StaggerRN <= 0);
    }

    public EnemyData GetCurrentEnemy()
    {
        return currentEnemy;
    }
    
    // al final de la clase BattleManager
    public bool IsBattleActive()
    {
        return battleIsActive;
    }


}






