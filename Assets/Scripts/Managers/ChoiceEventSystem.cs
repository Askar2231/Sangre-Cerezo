using UnityEngine;
using System;
using System.Collections.Generic;

public class ChoiceEventSystem : MonoBehaviour
{
    public static ChoiceEventSystem Instance { get; private set; }

    private Dictionary<string, Action> choiceCallbacks = new Dictionary<string, Action>();

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

    public void RegisterChoice(string choiceId, Action callback)
    {
        if (!choiceCallbacks.ContainsKey(choiceId))
        {
            choiceCallbacks[choiceId] = callback;
        }
        else
        {
            choiceCallbacks[choiceId] += callback;
        }
    }

    public void UnregisterChoice(string choiceId, Action callback)
    {
        if (choiceCallbacks.ContainsKey(choiceId))
        {
            choiceCallbacks[choiceId] -= callback;
        }
    }

    public void InvokeChoice(string choiceId)
    {
        if (choiceCallbacks.ContainsKey(choiceId) && choiceCallbacks[choiceId] != null)
        {
            Debug.Log($"<color=cyan>Invocando elección:</color> {choiceId}");
            choiceCallbacks[choiceId].Invoke();
        }
        else
        {
            Debug.LogWarning($"No hay callbacks registrados para la elección: {choiceId}");
        }
    }

    private void OnDestroy()
    {
        choiceCallbacks.Clear();
    }
}