using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class Enemy : Character
{
    public int RoundValue { get; private set; }
    public EnemyType Type { get; private set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public EnemyClassType ClassType { get; private set; }

    public Enemy(int id, string name, int maxHp,
        int roundValue ,EnemyType type, EnemyClassType classType ,List<Card> data) 
        : base(id, name, maxHp, data)
    {
        RoundValue = roundValue;
        Type = type;
        ClassType = classType;
    }
}
