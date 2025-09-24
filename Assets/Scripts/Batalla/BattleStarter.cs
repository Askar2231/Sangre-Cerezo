using UnityEngine;

public class BattleStarter : MonoBehaviour
{



        

    public BattleManager battleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleManager = FindFirstObjectByType<BattleManager>();
        
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            battleManager.StartBattle();
            Destroy(gameObject); // Destruye este objeto para que la batalla no se inicie de nuevo
        }
    }
}
