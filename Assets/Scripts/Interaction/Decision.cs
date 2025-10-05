// Assets/Scripts/Interaction/Decision.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Decision", menuName = "Sangre y Cerezo/Decision")]
public class Decision : ScriptableObject
{
    public DialogueLine[] introductoryLines; // Diálogo antes de presentar las opciones
    public List<Choice> choices; // La lista de decisiones que el jugador puede tomar
}