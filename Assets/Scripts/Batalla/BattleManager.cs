using UnityEngine;
using System.Collections;

public class BattleManager : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;
    public Canvas battleCanvas;

    private bool battleIsActive = false;

    void Start()
    {
    }

    public void StartBattle()
    {
        battleIsActive = true;
        battleCanvas.gameObject.SetActive(true);

        // Bloquear movimiento libre
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        movimiento.enabled = false;

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
    }
}

