using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using SF = UnityEngine.SerializeField;

public class HandManager : MonoBehaviour
{
    [Header("기본 참조")]
    [SF] private CanvasGroup handCanvas;
    [SF] private CardData baseCardObj;
    [SF] private Transform baseHolderPos;
    [SF] private RectTransform cardHolderPos;

    [SF] private Button closeBtn;

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

    public UniTaskCompletionSource cardDrawTrigger;

    public bool isDrawTime;

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
            card.parentTransform = handCanvas.transform;
            card.handTransform = cardHolderPos;
            card.CloseBtn = closeBtn;
            allCards.Add(card);
        }

        allCards.Shuffle();
        deckCards.AddRange(allCards);
    }
#pragma warning disable CS4014
    public async UniTask HandDrop()
    {
        // 카드 돌아가는 애니메이션 추가
        Sequence mainSeq = DOTween.Sequence();
        float jumpPower = HAND_DROP_JUMP_POWER;
        float turnTime = HAND_DROP_TURN_TIME;
        float turnDropTime = HAND_DROP_TIME;
        float scale = HAND_DROP_SCALE;

        int count = 0;

        foreach (var item in handCards)
        {
            // 비활성화된 객체는 바로 복귀
            if(!item.gameObject.activeSelf)
            {
                item.transform.SetParent(baseHolderPos);
                item.transform.localPosition = Vector3.zero;
                item.cardBehind.SetActive(true);
                item.labelImage.SetActive(false);
                item.typeIcon.gameObject.SetActive(false);
                item.starSlot.SetActive(false);
                continue;
            }

            float nowX = item.rect.localPosition.x;
            float nowY = item.rect.localPosition.y;
            Vector3 nowScale = item.rect.localScale;
            Sequence inSeq = DOTween.Sequence();
            item.transform.SetParent(baseHolderPos);
            inSeq
                // 회전 중 위로 이동
                .Join(item.rect.DOLocalMove(
                            new Vector3(nowX, nowY + jumpPower, 0), turnTime)
                        .SetEase(Ease.Linear)
                        )
                // 회전 반
                .Join(item.rect.DOLocalRotate(
                            new Vector3(0, 90, 0), turnTime / 2)
                        .SetEase(Ease.Linear)
                        .OnComplete(() =>
                        {
                            item.cardBehind.SetActive(true);
                            item.labelImage.SetActive(false);
                            item.typeIcon.gameObject.SetActive(false);
                            item.starSlot.SetActive(false);
                        }))
                // 회전 나머지 반
                .Insert(turnTime / 2, item.rect.DOLocalRotate(
                            Vector3.zero, turnTime / 2)
                        .SetEase(Ease.Linear))
                // 축소
                .Insert(turnTime, item.rect.DOScale(
                            nowScale * scale, turnDropTime)
                        .SetEase(Ease.Linear))
                .Insert(turnTime, item.canvasGroup.DOFade(
                            0.3f, turnDropTime)
                        .SetEase(Ease.Linear))
                .Insert(turnTime, item.rect.DOLocalMove(
                            Vector3.zero, turnDropTime)
                        .SetEase(Ease.Linear)
                        .OnComplete(()=>
                        {
                            item.gameObject.SetActive(false);
                            item.rect.localScale = Vector3.one;
                            item.canvasGroup.alpha = 1;
                        }));

            mainSeq.Insert(HAND_DROP_GAP * count, inSeq);
            count++;
        }

        await mainSeq.Play();

        if (handCards.Count > 0)
        {
            discardCards.AddRange(handCards);
            handCards.Clear();
        }
    }

    /// <summary>
    /// 카드 셔플 및 핸드 설정
    /// </summary>
    public void ShuffleAndSettingHand()
    {
        cardDrawTrigger = new();
        if (deckCards.Count < 5 && discardCards.Count > 0)
        {
            discardCards.Shuffle();
            deckCards.AddRange(discardCards);
            discardCards.Clear();
        }
        int count = 0;



        while (handCards.Count < 5 && deckCards.Count > 0)
        {
            deckCards[0].transform.SetParent(cardHolderPos);
            deckCards[0].posIndex = count;
            handCards.Add(deckCards[0]);
            deckCards.RemoveAt(0);
            count++;
        }
    }

    /// <summary>
    /// 핸드 위치 정렬 및 애니메이션 실행
    /// </summary>
    public void UpdateHandPos()
    {
        if (handCards == null) return;

        int activeCardCount = 0;
        int cardCount = handCards.Count;

        foreach (var item in handCards)
        {
            if (item.gameObject.activeSelf)
                activeCardCount++;

        }

        if (cardCount == 0) return;

        DrawAnimation(activeCardCount, cardCount).Forget();
    }

    /// <summary>
    /// 드로우 용 애니메이션
    /// </summary>
    /// <param name="activeCardCount"></param>
    /// <param name="cardCount"></param>
    /// <returns></returns>
    private async UniTask DrawAnimation(int activeCardCount, int cardCount)
    {
        // 카드 총 간격이 칸 간격 보다 적으면 간격대로, 많으면 폭 내에서 존재하도록 재계산 
        float totalWidth = (activeCardCount - 1) * cardSpacing;
        if (totalWidth > maxHandWidth)
        {
            totalWidth = maxHandWidth;
        }

        // 카드가 한장일 경우 정중앙에 배치
        float currentSpacing = (activeCardCount > 1) ? totalWidth / (activeCardCount - 1) : 0f;

        // 시작점 계산
        float startX = -totalWidth / 2f;
        int activeIdx = 0;
        Sequence mainSequence = DOTween.Sequence();

        for (int i = 0; i < cardCount; i++)
        {
            if (!handCards[i].gameObject.activeSelf) continue;

            // 상대 위치 계산
            float normalizedPosition = (activeCardCount > 1) ? (activeIdx / (float)(activeCardCount - 1)) * 2f - 1f : 0f;

            // X축 간격 지정
            float xOffset = startX + (activeIdx * currentSpacing);

            // y축 위치 지정
            // 중앙이 0, 나머지는 curveStrength값에 따라 아래로 내려감
            float yOffset = (normalizedPosition * normalizedPosition) * -curveStrength;

            // Z축 회전, 왼쪽에서 오른쪽으로 - ~ + 값으로 휘어짐
            float zRotation = normalizedPosition * -rotationStrength;

            RectTransform rect = handCards[i].rect;
            
            Sequence seq = DOTween.Sequence();
            int lockIndex = i;
            float jumpPower = DRAW_JUMP_POWER; ;
            float turnTime = DRAW_TURN_TIME;
            float turnDrawTime = DRAW_TIME;
            float handTime = HAND_SORT_TIME;

            float nowX = rect.localPosition.x;
            float nowY = rect.localPosition.y;
            if (rect != null)
            {
                if(!rect.gameObject.activeSelf)
                    rect.gameObject.SetActive(true);

                if (isDrawTime)
                {
                    rect.localRotation = Quaternion.Euler(0, 180f, 0);
                    // 부모 변경
                    rect.SetParent(cardHolderPos, false);
                    rect.SetAsFirstSibling();

                    seq
                        .Join(rect.DOLocalRotate(
                            new Vector3(0, 90, 0), turnTime / 2)
                        .SetEase(Ease.Linear)
                        .OnComplete(() => {
                            handCards[lockIndex].typeIcon.gameObject.SetActive(true);
                            handCards[lockIndex].labelImage.SetActive(true);
                            handCards[lockIndex].starSlot.SetActive(true);
                            handCards[lockIndex].cardBehind.SetActive(false);
                        })
                        )
                        .Insert(turnTime / 2, rect.DOLocalRotate(
                            new Vector3(0, 0, 0), turnTime / 2)
                        .SetEase(Ease.Linear))
                        .Join(rect.DOLocalMove(
                            new Vector3(nowX, nowY + jumpPower, 0), turnTime)
                        .SetEase(Ease.Linear)
                        )
                        .Insert(turnTime,
                            rect.DOLocalRotateQuaternion(
                            Quaternion.Euler(0, 0, zRotation), turnDrawTime)
                        .SetEase(Ease.OutQuad)
                        )
                        .Insert(turnTime,
                            rect.DOLocalMove(
                            new Vector3(xOffset, yOffset, 0), turnDrawTime)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => {
                            rect.SetSiblingIndex(activeIdx);
                        })
                        );
                }
                else
                {
                    seq.Join(
                            rect.DOLocalRotateQuaternion(
                            Quaternion.Euler(0, 0, zRotation), handTime)
                        .SetEase(Ease.OutQuad)
                        )
                        .Join(
                            rect.DOLocalMove(
                            new Vector3(xOffset, yOffset, 0), handTime)
                        .SetEase(Ease.OutQuad)
                        );
                }

                rect.tag = HAND_TAG;
            }
            if(isDrawTime)
                mainSequence.Insert(turnTime / 2 * activeIdx, seq);
            else
                mainSequence.Insert(handTime / 2 * activeIdx, seq);
            activeIdx++;
        }
        await mainSequence.Play().ToUniTask();

        cardDrawTrigger?.TrySetResult();
    }

    public void HandActive()
    {
        foreach (Transform item in cardHolderPos)
        {
            item.gameObject.SetActive(true);
        }
    }
}
