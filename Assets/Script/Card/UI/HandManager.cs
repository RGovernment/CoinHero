using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;
public class HandManager : MonoBehaviour
{
    [Header("기본 참조")]
    [SF] private CanvasGroup handCanvas;
    [SF] private CardData baseCardObj;
    [Header("드래그 앤 드롭 용 더미 객체")]
    [SF] private CardData ClickCard;

    /// <summary>
    /// 전체 카드, 핸드, 덱, 묘지 카드는 여기서 주고받아 조작됨 
    /// 특수한 능력을 제외하면 변경되지 않음
    /// </summary>
    private List<CardData> allCards;
    /// <summary>
    /// 핸드 카드
    /// </summary>
    private List<CardData> handCards;

    /// <summary>
    /// 덱 내 카드, 스택으로 순서 보장
    /// </summary>
    private Stack<CardData> deckCards;

    /// <summary>
    /// 버려진 카드
    /// </summary>
    private List<CardData> discardCards;

    public void CreateHandCard(List<Card> data,Character user)
    {
        foreach (var item in data)
        {
            CardData card = new()
            {
                cardData = item,
                user = user
            };

            allCards.Add(card);
        }
    }
}
