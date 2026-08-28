using System.Collections.Generic;
using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public Enemy Enemy { get; set; }

    private void Awake()
    {
        List<Card> cd = new();
        foreach (var item in ResourceManager.Instance.EnemyCardData)
        {
            cd.Add(item.Value);
        }

        Enemy = new Enemy(10, 50, cd);
    }

    private void Start()
    {
        BattleManager.Instance.RegisterEnemy(this);
    }
}
