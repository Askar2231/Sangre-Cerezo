using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public Canvas battleCanvas;
    public ScreenFader fader;
    private PlayerSnapshot savedSnapshot;

    private bool battleIsActive = false;

    void Start()
    {
    }

    public void StartBattle()
    {
        savedSnapshot = SavePlayerState(player);
        StartCoroutine(StartBattleRoutine());


    }

    IEnumerator StartBattleRoutine()
    {
        // Pantalla a negro
        yield return StartCoroutine(fader.FadeOut(0.5f));

        // Espera mientras está negro
        yield return new WaitForSeconds(0.5f);

        // Preparar combate (jugador/enemigo/UI)
        battleIsActive = true;
        battleCanvas.gameObject.SetActive(true);
        player.GetComponent<MovimientoBasico>().enabled = false;

        // Volver a mostrar
        yield return StartCoroutine(fader.FadeIn(0.5f));

        // Iniciar ataques del enemigo
        StartCoroutine(EnemyAttackLoop());
    }

   

    IEnumerator EnemyAttackLoop()
    {
        while (battleIsActive)
        {
            float waitTime = Random.Range(1.5f, 3f);
            yield return new WaitForSeconds(waitTime);
            EnemyAttack();
        }
    }

    public void EnemyAttack()
    {
        Debug.Log("¡El enemigo ataca!");
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
            rb.velocity = snapshot.rbVelocity;
        }
    }

}






