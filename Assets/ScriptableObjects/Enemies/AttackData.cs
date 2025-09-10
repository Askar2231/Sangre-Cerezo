using UnityEngine;

[CreateAssetMenu(fileName = "NewAttack", menuName = "RPG/Attack Data")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public int damage;
    public float windUpTime;   // tiempo de preparación
     // tiempo de recuperación
    public int hitCount = 1;   // golpes por ataque
    public AnimationClip animation; // animación asociada
    public AudioClip sfx; // sonido opcional
}

