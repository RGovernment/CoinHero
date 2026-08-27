using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;
public class HandManager : MonoBehaviour
{
    [Header("기본 참조")]
    [SF] private CanvasGroup handCanvas;
    [SF] private CardData baseCardObj;
    [SF] private Transform baseHolderPos;
    [SF] private RectTransform cardHolderPos;
    [Header("드래그 앤 드롭 용 더미 객체")]
    [SF] private CardData ClickCard;
    [Header("카드 배치 부채꼴 설정")]
    public float maxHandWidth => cardHolderPos.sizeDelta.x; // 최대 가로 폭
    public float cardSpacing = 150f;    // 카드 간격
    public float curveStrength = 30f;   // 카드 휨 강도(Y축)
    public float rotationStrength = 8f; // 회전 강도 (Z축)

    /// <summary>
    /// 전체 카드, 핸드, 덱, 묘지 카드는 여기서 주고받아 조작됨 
    /// 특수한 능력을 제외하면 변경되지 않음
    /// 초기 상태 보존용
    /// </summary>
    private List<CardData> allCards;
    /// <summary>
    /// 핸드 카드
    /// </summary>
    private List<CardData> handCards;

    /// <summary>
    /// 덱 내 카드, 스택으로 순서 보장
    /// </summary>
    private List<CardData> deckCards;

    /// <summary>
    /// 버려진 카드
    /// </summary>
    private List<CardData> discardCards;

    public void CreateAllCard(List<Card> data,Character user)
    {
        allCards = new List<CardData>();
        handCards = new List<CardData>();
        deckCards = new List<CardData>();
        discardCards = new List<CardData>();

        foreach (var item in data)
        {
            CardData card = Instantiate(baseCardObj, baseHolderPos);
            card.gameObject.SetActive(false);
            card.cardData = item;
            card.user = user;

            allCards.Add(card);
        }

        allCards.Shuffle();
        deckCards.AddRange(allCards);
    }

    public void HandDrop()
    {
        discardCards.AddRange(handCards);
        handCards.Clear();
    }

    public void ShuffleAndSettingHand()
    {
        if (deckCards.Count < 5 && discardCards.Count > 0)
        {
            discardCards.Shuffle();
            deckCards.AddRange(discardCards);
            discardCards.Clear();
        }

        while (handCards.Count < 5 && deckCards.Count > 0)
        {
            deckCards[0].transform.SetParent(cardHolderPos);
            handCards.Add(deckCards[0]);
            deckCards.RemoveAt(0);
        }
    }

    public void UpdateHandPos()
    {
        int cardCount = handCards.Count;
        if (cardCount == 0) return;

        // 카드 총 간격이 칸 간격 보다 적으면 간격대로, 많으면 폭 내에서 존재하도록 재계산 
        float totalWidth = (cardCount - 1) * cardSpacing;
        if (totalWidth > maxHandWidth)
        {
            totalWidth = maxHandWidth;
        }

        // 카드가 한장일 경우 정중앙에 배치
        float currentSpacing = (cardCount > 1) ? totalWidth / (cardCount - 1) : 0f;

        // 시작점 계산
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            // 상대 위치 계산
            float normalizedPosition = (cardCount > 1) ? (i / (float)(cardCount - 1)) * 2f - 1f : 0f;

            // X축 간격 지정
            float xOffset = startX + (i * currentSpacing);

            // y축 위치 지정
            // 중앙이 0, 나머지는 curveStrength값에 따라 아래로 내려감
            float yOffset = (normalizedPosition * normalizedPosition) * -curveStrength;

            // Z축 회전, 왼쪽에서 오른쪽으로 - ~ + 값으로 휘어짐
            float zRotation = normalizedPosition * -rotationStrength;

            RectTransform rect = handCards[i].rect;

            if (rect != null)
            {
                // 부모 변경
                rect.SetParent(cardHolderPos, false);

                rect.localPosition = new Vector3(xOffset, yOffset, 0);
                rect.localRotation = Quaternion.Euler(0, 0, zRotation);

                rect.SetSiblingIndex(i);
                rect.tag = "Hand";
            }
            rect.gameObject.SetActive(true);
        }
    }
}
