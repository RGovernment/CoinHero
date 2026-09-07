using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using static Enums;
public class Player : Character
{
    [JsonConverter(typeof(StringEnumConverter))]
    public PlayerClassType ClassType { get; private set; }

    public Player(int id,string name, int maxHp,int nowHp,PlayerClassType classType, List<Card> data) : base(id, name, maxHp, data)
    {
        ClassType = classType;
        HP = nowHp;
    }
}
