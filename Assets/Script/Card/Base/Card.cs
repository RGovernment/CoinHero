using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using static Enums;
using static Constants;
using static EffectTextParser;

[Serializable]
public class Card
{
    public Card(CardWriteOnly card)
    {
        Name = card.Name;
        Id = card.Id;
        Type = card.Type;
        Value = card.Value;
        Coin = card.Coin;
        CoinPoint = card.CoinPoint;
        CurrentUpgradeLv = card.CurrentUpgradeLv;
        MaxUpgradeLv = card.MaxUpgradeLv;
        UpgradeData = card.UpgradeData;
        Effect = card.Effect;
        Description = card.Description;
    }

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
        /*StringBuilder sb = new(Description);*/
        string result = Description;
        
        /*foreach (var item in Effect)
        {
            if(list.TryGetValue(item.EffectId, out StatusEffectData data))
            {

                string enumText = Enum.GetName(typeof(EffectType), data.Type);

                // 디버프/버프 효과 텍스트 변환
                sb.Replace($"{{{enumText}}}", $"<link={data.Id}>[{data.Name}]</link>")
                    .Replace($"[{enumText}_{VALUE}]", $"{data.Value}")
                    // 수치 텍스트 변환
                    // 코인값 제공
                    .Replace($"[{COIN}]", $"{Coin}")
                    // 코인 위력 값 제공
                    .Replace($"[{COIN_POINT}]", $"{CoinPoint}")
                    // (모두 앞면일 경우) 코인 위력 제공
                    .Replace($"[{COIN}{MULTIPLY}{COIN_POINT}]", $"{Coin * CoinPoint}")
                    // (모두 앞면일 경우) 전체 위력 제공
                    .Replace($"[{VALUE}{PLUS}{COIN}{MULTIPLY}{COIN_POINT}]", $"{Value + Coin * CoinPoint}")
                    // 기본 위력 제공
                    .Replace($"[{VALUE}]", $"{Value}");

            }

        }*/
        foreach (var item in Effect)
        {
            if (list.TryGetValue(item.EffectId, out StatusEffectData data))
            {
                // {EffectType} 태그 변환 (예: {Heal} -> <link=101>[체력 회복]</link>)
                string enumText = Enum.GetName(typeof(EffectType), data.Type);
                Debug.Log(enumText);
                if (!string.IsNullOrEmpty(enumText))
                {
                    string effectLink = $"<link={data.Id}>[{data.Name}]</link>";
                    result = result
                        .Replace($"{{{enumText}}}", effectLink)
                        .Replace($"[{enumText}_Value]", item.Value.ToString());
                }
            }
        }
        Debug.Log(result);
        return result.ParseDescription(this);
    }
}
