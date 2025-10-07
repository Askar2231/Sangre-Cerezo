using UnityEngine;

public class KarmaManager : MonoBehaviour
{
    public static KarmaManager Instance { get; private set; }
    
    private int currentKarma = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddKarma(int amount)
    {
        currentKarma += amount;
        Debug.Log($"Karma añadido: {amount}. Karma total actual: {currentKarma}");
    }

    public int GetCurrentKarma()
    {
        return currentKarma;
    }
}