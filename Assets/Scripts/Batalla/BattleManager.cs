using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public GameObject player;
    public GameObject enemy;

    public Canvas battleCanvas;


    void Start()
    {
    }
    public void StartBattle()
    {
        // Initialize battle settings

        battleCanvas.gameObject.SetActive(true);
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        movimiento.enabled = false;





    }


    public void EnemyAttack()
    {
        // Implement enemy attack logic

        EnergyGive();
    }

    public void EnergyGive()
    {
        // Implement energy giving logic
    }

    public void EndBattle()
    {
        // Clean up and exit battle mode
        battleCanvas.gameObject.SetActive(false);
        MovimientoBasico movimiento = player.GetComponent<MovimientoBasico>();
        movimiento.enabled = true;

    }

}
