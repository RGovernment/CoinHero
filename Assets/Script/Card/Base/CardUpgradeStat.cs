using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using static Enums;
[Serializable]
public class CardUpgradeStat
{
    [JsonConverter(typeof(StringEnumConverter))]
    public StatType statType;
    public int value;
}
