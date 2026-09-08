using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using UnityEngine;
using static Enums;
using static Constants;
public class Enemy : Character
{
    public int RoundValue { get; private set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public EnemyType Type { get; private set; }
    [JsonConverter(typeof(StringEnumConverter))]
    public EnemyClassType ClassType { get; private set; }

    public Enemy(int id, string name, int maxHp,
        int roundValue ,EnemyType type, EnemyClassType classType ,List<Card> data,
        int sanity = DEFAULT_SANITY_VALUE) 
        : base(id, name, maxHp, sanity, data)
    {
        RoundValue = roundValue;
        Type = type;
        ClassType = classType;
    }
}
