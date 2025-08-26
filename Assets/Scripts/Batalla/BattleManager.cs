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
}
