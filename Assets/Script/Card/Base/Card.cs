using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

[Serializable]
public class Card
{
    /// <summary>
    /// 카드 이름
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 카드 아이디(유일성 보장)
    /// </summary>
    
    public int Id { get; set; }
    /// <summary>
    /// 카드 종류
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public CardType Type { get; set; }

    /// <summary>
    /// 카드의 기본 위력
    /// </summary>
    [Header("위력/코인 정보")]
    public int Value { get; set; }

    /// <summary>
    /// 카드의 코인 개수
    /// </summary>
    public int Coin { get; set; }

    /// <summary>
    /// 코인의 위력
    /// </summary>
    public int CoinPoint { get; set; }

    /// <summary>
    /// 현재 강화 단계
    /// </summary>
    [Header("강화 정보")]
    public int CurrentUpgradeLv { get; set; }

    /// <summary>
    /// 최대 강화 단계
    /// </summary>
    public int MaxUpgradeLv { get; set; }

    // 구조 예시
    //  "upgradeData": [
    //      { "statType": "value", "value": 2 },
    //      { "statType": "coinPoint", "value": 1 },
    //      { "statType": "value", "value": 3 }
    //  ]
    /// <summary>
    /// 카드가 가진 각 강화 별 수치 데이터 리스트
    /// </summary>
    public List<CardUpgradeStat> UpgradeData { get; set; }


    /// <summary>
    /// 카드가 가진 효과 데이터 리스트
    /// </summary>
    [Header("효과 및 설명")]
    public List<CardEffectData> Effect { get; set; }

    /// <summary>
    /// 카드 설명
    /// </summary>
    public string Description { get; set; }

    public Card Init(Card data)
    {
        Card card = new()
        {
            Coin = data.Coin,
            CoinPoint = data.CoinPoint,
            Value = data.Value,
            Name = data.Name,
            Id = data.Id,
            Type = data.Type,
            CurrentUpgradeLv = data.CurrentUpgradeLv,
            MaxUpgradeLv = data.MaxUpgradeLv,
            UpgradeData = data.UpgradeData,
            Effect = data.Effect,
            Description = data.Description

        };
        
        return card;
    }
    public int FinalValue()
    {
        int result = 0;

        if(UpgradeData != null)
        {
            int applyCount = Mathf.Min(CurrentUpgradeLv, UpgradeData.Count);
            for (int i = 0; i < applyCount; i++)
            {
                if (UpgradeData[i].statType == StatType.Value)
                    result += UpgradeData[i].value;

            }
        }

        // 카드 자체 효과 (CardEffect) 추가시 여기에서 효과 처리 추가

        return Value + result;
    }

    public int FinalCoinPoint()
    {
        int result = 0;
        if (UpgradeData != null) {

            int applyCount = Mathf.Min(CurrentUpgradeLv, UpgradeData.Count);

            for (int i = 0; i < applyCount; i++)
            {
                if (UpgradeData[i].statType == StatType.CoinPoint)
                {
                    result += UpgradeData[i].value;
                }
            }
        }
        

        // 카드 자체 효과 (CardEffect) 추가시 여기에서 효과 처리 추가

        return CoinPoint + result;
    }

    public int FinalCoin()
    {
        int result = 0;
        if (UpgradeData != null)
        {

            int applyCount = Mathf.Min(CurrentUpgradeLv, UpgradeData.Count);

            for (int i = 0; i < applyCount; i++)
            {
                if (UpgradeData[i].statType == StatType.Coin)
                {
                    result += UpgradeData[i].value;
                }
            }
        }


        // 추후 Contants로 이관
        int MAX_COIN = 5;
        return Mathf.Min(Coin + result, MAX_COIN);
    }

    public string GetDescription(Dictionary<int, StatusEffectData> list)
    {
        if (list == null || list.Count <= 0 ) return "";
        string result = Description;
        if(Effect != null)
        {
            foreach (var item in Effect)
            {
                if (list.TryGetValue(item.EffectId, out StatusEffectData data))
                {
                    // {EffectType} 태그 변환 (예: {Heal} -> <link=101>[체력 회복]</link>)
                    string enumText = Enum.GetName(typeof(EffectType), data.Type);

                    if (!string.IsNullOrEmpty(enumText))
                    {
                        string effectLink = $"<link={data.Id}>[{data.Name}]</link>";
                        result = result
                            .Replace($"{{{enumText}}}", effectLink)
                            .Replace($"[{enumText}_Value]", item.Value.ToString());
                    }
                }
            }
        }

        return result.ParseDescription(this);
    }

    public int CalcCoinValue(bool[] coinResults)
    {
        int finalValue = FinalValue();
        int coinPoint = FinalCoinPoint();
        for (int i = 0; i < coinResults.Length; i++)
        {
            if (coinResults[i])
            {
                finalValue += coinPoint;
            }
        }
        return finalValue;
    }

    public override string ToString()
    {
        string result = 
            $"Name : {Name}\nValue : {Value}\nCoin : {Coin}\nCoinPoint : {CoinPoint}\n" +
            $"Type : {Type}\nDescription : {Description}";
        return result;
    }
}
