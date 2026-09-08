using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Constants;
using static Enums;

using SF = UnityEngine.SerializeField;

public class OtherBattleDeckManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SF] private Button deckIcon;
    [SF] private CanvasGroup deckCanvasGroup;
    [SF] private GameObject deckOutLine;
    [SF] private GameObject cardTempStack;

    [Header("내부 / 덱")]
    [SF] private RectTransform deckContent;
    [SF] private RectTransform deckViewPort;
    [SF] private ScrollRect deckScroll;

    [SF] private CardData cardPrefab;

    private List<CardData> deckCards;

    [SF] private float FadeTime;

    public void Start()
    {
        deckCards = new();
    }

    public void DeckOpen()
    {
        deckCanvasGroup.alpha = ZERO;
        deckCanvasGroup.gameObject.SetActive(true);
        deckCanvasGroup.DOFade(ONE, FadeTime);

        Debug.Log(GameManager.Instance.state.playerData.CardList.Count);

        DeckInfoLoad(GameManager.Instance.state.playerData.CardList);
    }

    public void DeckClose()
    {
        deckCanvasGroup.alpha = ONE;
        deckCanvasGroup.DOFade(ZERO, FadeTime);
        deckCanvasGroup.gameObject.SetActive(false);

    }
    public void NewDeckCardAdd(Card card)
    {
        CardData deckCard = Instantiate(cardPrefab, cardTempStack.transform);

        deckCard.Init(card);
        deckCard.cardBehind.SetActive(false);
        deckCard.labelImage.gameObject.SetActive(true);
        deckCard.starSlot.SetActive(true);
        deckCard.typeIcon.gameObject.SetActive(true);
        deckCard.gameObject.tag = INVEN_TAG;
        deckCard.description.raycastTarget = true;
        deckCards.Add(deckCard);
    }

    public void DeckInfoLoad(List<Card> allCard)
    {
        // 비어있을 경우 덱에 새로 로드
        if (deckCards == null || deckCards.Count <= 0)
        {
            foreach (var item in allCard)
            {
                NewDeckCardAdd(item);
            }
        }

        // 생성 불가능한 경우 리셋
        if (deckCards == null || deckCards.Count <= 0) return;

        // 일괄 비활성화
        foreach (var item in deckCards)
            item.gameObject.SetActive(false);
        Debug.Log(deckCards.Count);
        foreach (var item in deckCards)
        {
            item.rect.localScale = Vector3.one * INVEN_CARD_SCALE;
            item.rect.Rotate(Vector3.zero);

            item.scroll = deckScroll;
            item.canvasGroup.alpha = 0;
            item.gameObject.SetActive(true);
            item.transform.SetParent(deckContent);
            //나머지는 덱에 있는 카드 목록으로 (순서 정렬 x)
            item.rect.Rotate(
            new(0, 0, Random.Range(ROTATE_MIN_ANGlE, ROTATE_MAX_ANGlE))
            );
            item.canvasGroup.alpha = 1;

        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (deckOutLine != null)
        {
            deckOutLine.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (deckOutLine != null)
        {
            deckOutLine.SetActive(false);
        }
    }

}
