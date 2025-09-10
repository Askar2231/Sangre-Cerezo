using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "RPG/Enemy")]
public class EnemyData : ScriptableObject
{
    //ENemigo
    public string enemyName;
    [TextArea] public string description;
    public GameObject prefab;
    


    //stats
    public int Health = 100;
    public int defense = 5;
    public int Stagger = 10;


    //ataques

    public List<AttackData> attacks;
    

}
