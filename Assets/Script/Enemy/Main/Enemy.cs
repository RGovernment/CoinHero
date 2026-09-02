using System.Collections.Generic;
using UnityEngine;

public class Enemy : Character
{
    public Enemy(int id, string name, int maxHp, List<Card> data) : base(id, name, maxHp, data)
    {
    }
}
