using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine;
using static Enums;

[Serializable]
public class StatusEffectData
{
    public int Id;
    public string Name;
    public EffectType Type;
    public int Value;
    public int Duration;

    /// <summary>
    /// 효과 설명
    /// </summary>
    public string Description;

    /// <summary>
    /// 발동 조건
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public CardTrigger Trigger;

    /// <summary>
    /// 발동 위치
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public CardZone Zone;

    /// <summary>
    /// 발동 대상
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public TargetType Target;
}
