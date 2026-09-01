using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework.Internal;
using System;
using System.Text;
using UnityEngine;
using static Enums;

public abstract class StatusEffect
{
    public StatusEffectData EffectData { get; private set; }

    public int Value;
    public int Duration;

    public StatusEffect(StatusEffectData data)
    {
        EffectData = data;
        Value = EffectData.Value;
        Duration = EffectData.Duration;
    }

    /// <summary>
    /// 턴 시작 시 작동
    /// </summary>
    public virtual void OnTurnStart() { }
    
    /// <summary>
    /// 턴 종료 시 작동
    /// </summary>
    public virtual void OnTurnEnd() { }

    /// <summary>
    /// 합 진행 시 작동
    /// </summary>
    public virtual void OnPlayStart() { }

    /// <summary>
    /// 합 종료 시 작동
    /// </summary>
    /// <param name="isWin">합 승리 여부</param>
    public virtual void OnPlayEnd(bool isWin) { }

    /// <summary>
    /// 피해를 줄 때 발동
    /// </summary>
    /// <param name="baseDamage">원본 대미지</param>
    /// <returns>계산된 대미지</returns>
    public virtual int OnModifyAttackDamage(int baseDamage) => baseDamage;

    /// <summary>
    /// 피해를 받을 때 발동
    /// </summary>
    /// <param name="baseDamage">원본 대미지</param>
    /// <returns>계산된 대미지</returns>
    public virtual int OnModifyTakeDamage(int baseDamage) => baseDamage;

    /// <summary>
    /// 힐을 받을 때 발동
    /// </summary>
    /// <param name="baseDamage">원본 대미지</param>
    /// <returns>계산된 대미지</returns>
    public virtual int OnModifyTakeHeal(int baseDamage) => baseDamage;

    /// <summary>
    /// richText로 변환된 설명을 반환하는 함수
    /// </summary>
    /// <returns>설명 Data string</returns>
    public string GetDescription(string description)
    {
        StringBuilder sb = new(description);

        string enumText = Enum.GetName(typeof(EffectType), EffectData.Type);

        sb.Replace($"{{{enumText}}}", $"<link={EffectData.Id}>[{EffectData.Name}]</link>")
            .Replace($"[{enumText}_Value]", $"{EffectData.Value}");

        return sb.ToString();
    }
}
