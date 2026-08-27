using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Enums;
[Serializable]
public class CardWriteOnly
{
    /// <summary>
    /// 카드 이름
    /// </summary>
    public string Name;

    /// <summary>
    /// 카드 아이디(유일성 보장)
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter)), JsonProperty]
    public int Id;
    /// <summary>
    /// 카드 종류
    /// </summary>
    public CardType Type;

    /// <summary>
    /// 카드의 기본 위력
    /// </summary>
    [Header("위력/코인 정보")]
    public int Value;
    /// <summary>
    /// 카드의 코인 개수
    /// </summary>
    public int Coin;
    /// <summary>
    /// 코인의 위력
    /// </summary>
    public int CoinPoint;
    /// <summary>
    /// 현재 강화 단계
    /// </summary>
    [Header("강화 정보")]
    public int CurrentUpgradeLv;
    /// <summary>
    /// 최대 강화 단계
    /// </summary>
    public int MaxUpgradeLv;

    // 구조 예시
    //  "upgradeData": [
    //      { "statType": "value", "value": 2 },
    //      { "statType": "value", "value": 3 },
    //      { "statType": "coinPoint", "value": 1 },
    //  ]
    /// <summary>
    /// 카드가 가진 각 강화 별 수치 데이터 리스트
    /// </summary>
    public List<CardUpgradeStat> UpgradeData;


    /// <summary>
    /// 카드가 가진 효과 데이터 리스트
    /// </summary>
    [Header("효과 및 설명")]
    public List<CardEffectData> Effect;

    /// <summary>
    /// 카드 설명
    /// </summary>
    [TextArea]
    public string Description;

}
