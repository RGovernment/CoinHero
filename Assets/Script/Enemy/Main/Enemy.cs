using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public Enemy(int id, int maxHp, List<Card> data) : base(id, maxHp, data)
    {
    }
}
