using UnityEngine;

public class CombatUIManager : MonoBehaviour
{
    public BattleManager battleManager;

    void Start()
    {
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }
    }

    
    void Update()
    {   
         
        if (battleManager == null || !battleManager.IsBattleActive())
        {
            return; 
        }
        
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnAttackButtonPressed(); 
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnParryButtonPressed(); 
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            OnDodgeButtonPressed(); 
        }
    }



    public void OnAttackButtonPressed()
    {
        Debug.Log("¡Jugador presiona Q para ATACAR!");
       
    }

    public void OnDodgeButtonPressed()
    {
        Debug.Log("¡Jugador presiona F para ESQUIVAR!");
        battleManager.TryDodge();
    }

    public void OnParryButtonPressed()
    {
        Debug.Log("¡Jugador presiona E para PARRY!");
        battleManager.TryParry();
    }
}