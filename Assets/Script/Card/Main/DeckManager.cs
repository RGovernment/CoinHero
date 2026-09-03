using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Enums;
using static Constants;

using SF = UnityEngine.SerializeField;

public class DeckManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SF] private HandManager handManager;
    [SF] private Button deckIcon;
    [SF] private CanvasGroup deckCanvasGroup;
    [SF] private GameObject deckOutLine;
    [SF] private GameObject cardTempStack;

    [Header("내부 / 덱")]
    [SF] private RectTransform deckContent;
    [SF] private RectTransform deckViewPort;

    [Header("내부 / 버려진 카드")]
    [SF] private RectTransform discardContent;
    [SF] private RectTransform discardViewPort;

    private List<CardData> deckCards;

    [SF] private float FadeTime;

    public void Start()
    {
        deckCards = new();
    }

    public void OnEnable()
    {
        handManager.OnDeckReload += DeckInfoLoad;
    }

    public void OnDisable()
    {
        handManager.OnDeckReload -= DeckInfoLoad;
    }

    public void DeckOpen()
    {
        deckCanvasGroup.alpha = 0;
        deckCanvasGroup.gameObject.SetActive(true);
        deckCanvasGroup.DOFade(1, FadeTime);

        DeckInfoLoad(handManager.GetAllCards(), handManager.GetDiscardCards());
    }

    public void DeckClose()
    {
        deckCanvasGroup.alpha = 1;
        deckCanvasGroup.DOFade(0, FadeTime);
        deckCanvasGroup.gameObject.SetActive(false);

    }
    public void NewDeckCardAdd(CardData card)
    {
        CardData deckCard = Instantiate(card, cardTempStack.transform);
  
        deckCard.Init(card.cardData);
        deckCard.cardBehind.SetActive(false);
        deckCard.labelImage.gameObject.SetActive(true);
        deckCard.starSlot.SetActive(true);
        deckCard.typeIcon.gameObject.SetActive(true);

        deckCard.gameObject.tag = INVEN_TAG;
        deckCards.Add(deckCard);
    }

    public void DeckInfoLoad(List<CardData> allCard, List<CardData> discardCard)
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
        
        
        foreach (var item in deckCards)
        {
            item.rect.localScale = Vector3.one * INVEN_CARD_SCALE;
            item.rect.Rotate(Vector3.zero);
            
            // 버려진 카드는 버려진 카드 덱으로
            if (discardCard.Any(data => data.cardData.Id == item.cardData.Id))
            {
                item.canvasGroup.alpha = 0;
                item.gameObject.SetActive(true);
                item.transform.SetParent(discardContent);

                item.rect.Rotate(
                    new(0,0,Random.Range(ROTATE_MIN_ANGlE, ROTATE_MAX_ANGlE))
                    );
                item.canvasGroup.alpha = 1;
                continue;
            }

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
        if(deckOutLine != null)
        {
            deckOutLine.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(deckOutLine != null)
        {
            deckOutLine.SetActive(false);
        }
    }
}
