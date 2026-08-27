using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using static Enums;
using static EffectTextParser;
public class CardWrite : MonoBehaviour
{
    public List<CardWriteOnly> data;
    public List<StatusEffectData> effects;

    private Dictionary<int, StatusEffectData> testData = new()
    {
        [100] = new() { 
            Name ="위력 증가",
            Description = "수치만큼 위력이 증가한다." ,
            Type = EffectType.ValueUp, 
            Trigger = CardTrigger.OnPlay, 
            Zone = CardZone.Slot,
            Target = TargetType.Caster
        },
        [101] = new() {
            Name = "체력 회복",
            Description = "수치만큼 체력을 회복한다.",
            Type = EffectType.InstantHeal,
            Trigger = CardTrigger.OnPlay,
            Zone = CardZone.Slot,
            Target = TargetType.Caster
        },
        [102] = new() {
            Name = "연속 행동",
            Description = "이번 턴, 사용 가능한 슬롯이 1칸 증가한다.",
            Type = EffectType.ExtraSlot,
            Trigger = CardTrigger.OnSpecial,
            Zone = CardZone.Special,
            Target = TargetType.Slot
        },
        [200] = new() {
            Name = "위력 감소",
            Description = "수치만큼 위력이 감소한다.",
            Type = EffectType.ValueDown,
            Trigger = CardTrigger.OnPlay,
            Zone = CardZone.Slot,
            Target = TargetType.TargetEnemy
        },
        [201] = new() {
            Name = "체력 피해",
            Description = "수치만큼 대상에게 피해를 준다.",
            Type = EffectType.ValueUp,
            Trigger = CardTrigger.OnPlay,
            Zone = CardZone.Slot,
            Target = TargetType.Caster
        }
    };

    public void JsonWrite()
    {
        Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public void JsonEffectWrite()
    {
        Debug.Log(JsonConvert.SerializeObject(effects, Formatting.Indented));
    }
}
