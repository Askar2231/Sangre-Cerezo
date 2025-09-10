using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    //cuestiones del player
    public GameObject player;
    private PlayerSnapshot savedSnapshot;
    public Transform tpPlayer;
    //cuestiones de camaras
    public GameObject OVCamera;
    public GameObject BattleCamera;
    //cuestiones enemigo equisde
    public GameObject enemy;
    public EnemyLoader enemyLoader;
    private EnemyData currentEnemy; 

    public EnemyData enemyToLoad;
    //cuestiones varias la vd
    public Canvas battleCanvas;
    public ScreenFader fader;
   

    private bool battleIsActive = false;

    

    public void StartBattle()
    {
        savedSnapshot = SavePlayerState(player);
        StartCoroutine(StartBattleRoutine());
        

    }

    IEnumerator StartBattleRoutine()
    {
        currentEnemy = enemyToLoad;

        // fade in a la batalla
        yield return StartCoroutine(fader.FadeOut(0.5f));

        
        yield return new WaitForSeconds(2f);
        if (enemyLoader != null && enemyToLoad != null) {

        //Inicializar enemigo
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
        // Activar y desactivar componentes
        battleIsActive = true;
        battleCanvas.gameObject.SetActive(true);
        player.GetComponent<MovimientoBasico>().enabled = false;

        // quitar la pantalla negro
        yield return StartCoroutine(fader.FadeIn(0.5f));

        // Iniciar ataques del enemigo
        StartCoroutine(EnemyAttackLoop());
    }


   //ciclo de ataques del enemigo.
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
         if (currentEnemy == null || currentEnemy.attacks.Count == 0)
        {
            Debug.LogWarning("El enemigo no tiene ataques configurados.");
            return;
        }

        
        int index = Random.Range(0, currentEnemy.attacks.Count);
        AttackData chosenAttack = currentEnemy.attacks[index];

        //waha
        Debug.Log(currentEnemy.enemyName + " usa " + chosenAttack.attackName);

   
        EnergyGive();
    }

    public void EnergyGive()
    {
        Debug.Log("Jugador recibe energía por ataque enemigo.");
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

        battleCanvas.gameObject.SetActive(false);
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        movimiento.enabled = true;
        OVCamera.SetActive(true);
        BattleCamera.SetActive(false);

        Debug.Log("Combate finalizado.");
        RestorePlayerState(player, savedSnapshot);
    }

    //gestion localizacion player


    public PlayerSnapshot SavePlayerState(GameObject player)
    {
        PlayerSnapshot snapshot = new PlayerSnapshot();

        // Guardar transform
        snapshot.position = player.transform.position;
        snapshot.rotation = player.transform.rotation;
        snapshot.parent = player.transform.parent;

        // Guardar movimiento
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        if (movimiento != null)
        {
            snapshot.movementEnabled = movimiento.enabled;
        }

        // Guardar rigidbody (si existe)
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
        // Restaurar transform
        player.transform.SetParent(snapshot.parent);
        player.transform.position = snapshot.position;
        player.transform.rotation = snapshot.rotation;

        // Restaurar movimiento
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        if (movimiento != null)
        {
            movimiento.enabled = snapshot.movementEnabled;
        }

        // Restaurar rigidbody (si existe)
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = snapshot.rbIsKinematic;
            rb.linearVelocity = snapshot.rbVelocity;
        }
    }


    //tepear al player ala azona de pelea:
    public void TeleportPlayerToBattle()
{
    if (tpPlayer != null && player != null)
    {
        player.transform.position = tpPlayer.position;
        player.transform.rotation = tpPlayer.rotation;
    }
}
}






