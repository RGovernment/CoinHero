using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class Enemy : Character
{
    public int RoundValue { get; private set; }
    public EnemyType Type { get; private set; }

    public Enemy(int id, string name, int maxHp, List<Card> data) : base(id, name, maxHp, data)
    {
    }
}
