using UnityEngine;

[CreateAssetMenu(fileName = "NewAbility", menuName = "RPG/Ability")]
public class AbilityData : ScriptableObject
{
    public string abilityName;
    public Sprite icon;
    public int energyCost = 5;

    [TextArea]
    public string description;

    public int damage = 0;
    public bool isDefensive = false;
}
