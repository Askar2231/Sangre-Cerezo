using UnityEngine;

public class PLACEHOLDERENEMY : MonoBehaviour
{
    public EnemyLoader enemyLoader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyLoader = GetComponent<EnemyLoader>();
        if (enemyLoader == null)
        {
            Debug.LogError("EnemyLoader component not found on the GameObject.");
        }
        else
        {
            int enemyHealth = enemyLoader.GetCurrentHealth();
            int enemyStagger = enemyLoader.GetCurrentStagger();
            string enemyName = enemyLoader.GetEnemyName();

            Debug.Log("Enemigo: " + enemyName);
            Debug.Log("Vida actual del enemigo: " + enemyHealth);
            Debug.Log("Stagger actual del enemigo: " + enemyStagger);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
