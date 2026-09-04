using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Constants;
using static Enums;

public class CardData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Card cardData;
    
    public Character user;
    public Button CloseBtn;
    public Transform parentTransform;
    public Transform handTransform;

    [Header("카드 외형")]
    public RectTransform baseRect;
    public Image itemImage;
    public GameObject starSlot;
    public Image starImage;
    public Image typeIcon;
    public Outline outline1;
    public Outline outline2;
    /// <summary>
    /// (int)CardType.x로 호출해 사용
    /// </summary>
    public Sprite[] typeIconSprite;


    [Header("카드 텍스트")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI coinValueText;
    public TextMeshProUGUI coinCountText;
    public TextMeshProUGUI description;

    [Header("카드 이미지 전환용")]
    public CanvasGroup canvasGroup;
    public RectTransform rect;
    public GameObject cardBehind;
    public Image labelImage;

    public ScrollRect scroll;

    public bool isPopup = false;
    public int posIndex = -1;

    private Vector3 beforeLocalPos;
    private Quaternion beforeLocalRotate;

    private void OnEnable()
    {
        if (cardData == null) return;
        if(ResourceManager.Instance.EffectData == null) return;

        string name = cardData.Name;
        int value = cardData.FinalValue(user);
        int coin = cardData.FinalCoin(user);
        int coinPoint = cardData.FinalCoinPoint(user);
        int typeInt = (int)cardData.Type;
        string descriptionText 
            = cardData.GetDescription(ResourceManager.Instance.EffectData, user);

        description.text = descriptionText;
        nameText.text = name;
        valueText.text = value.ToString();
        coinValueText.text = coinPoint.ToString();
        coinCountText.text = $"x{coin}";
        typeIcon.sprite = typeIconSprite[typeInt];
        if(ResourceManager.Instance.CardImageData.TryGetValue(cardData.Id,out Sprite sp))
            itemImage.sprite = sp;
    }

    public void Init(Card cardData)
    {
        this.cardData = cardData.Init(cardData);
    }


    public async UniTaskVoid OpenInfo()
    {
        if (!CompareTag(HAND_TAG)) return;
        // 확대시
        if (!isPopup && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            description.raycastTarget = true;
            isPopup = true;
            beforeLocalPos = rect.localPosition;
            beforeLocalRotate = rect.localRotation;

            Sequence seq = DOTween.Sequence();
            CloseBtn.gameObject.SetActive(true);

            outline1.enabled = false;
            outline2.enabled = false;
            await seq
                .Join(rect.DOLocalMove(rect.localPosition + new Vector3(0, baseRect.sizeDelta.y, 0), CARD_EXPAND_TIMER))
                .Join(rect.DOScale(Vector3.one * CARD_EXPAND_SCALE, CARD_EXPAND_TIMER))
                .Join(rect.DORotate(Vector3.zero, CARD_EXPAND_TIMER)).ToUniTask();

            transform.SetParent(parentTransform);

            CloseBtn.onClick.RemoveAllListeners();
            CloseBtn.onClick.AddListener(CloseBtnToEvent);
        }
        // 선택시
        else if (!isPopup && Mouse.current.leftButton.wasReleasedThisFrame && 
            BattleManager.Instance.state.GetStateType() == BattleStateType.PlayerChoosePhase)
        {
            transform.localScale = Vector3.one;
            transform.SetSiblingIndex(posIndex);
            outline1.enabled = false;
            outline2.enabled = false;
            BattleManager.Instance.SelectCard(this);
        }
    }
    public void OpenInfoEvent()
    {
        OpenInfo().Forget();
    }
#pragma warning disable CS4014
    public async UniTaskVoid CloseBtnActive()
    {
        Sequence seq = DOTween.Sequence();
        
        if(CompareTag(HAND_TAG))
        {
            transform.SetParent(handTransform);
            transform.SetSiblingIndex(posIndex);
            CloseBtn.onClick.RemoveAllListeners();
            seq.Join(rect.DOLocalMove(beforeLocalPos, CARD_EXPAND_TIMER));
            seq.Join(rect.DOScale(Vector3.one, CARD_EXPAND_TIMER));
        }    
            
        if (CompareTag(INVEN_TAG))
            seq.Join(rect.DOScale(Vector3.one * INVEN_CARD_SCALE, CARD_EXPAND_TIMER));
        
            
        
        seq.Join(rect.DOLocalRotate(beforeLocalRotate.eulerAngles, CARD_EXPAND_TIMER));

        await seq.Play().ToUniTask();

        if (CompareTag(HAND_TAG))
        {
            CloseBtn.gameObject.SetActive(false);
            description.raycastTarget = false;
        }
            
        else if (CompareTag(INVEN_TAG))
        {
            starSlot.SetActive(true);
        }
            
        isPopup = false;
    }

    public void CloseBtnToEvent()
    {
        CloseBtnActive().Forget();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CompareTag(INVEN_TAG)) 
        {
            transform.localScale = 1.1f * INVEN_CARD_SCALE * Vector3.one;
            outline1.enabled = true;
            outline2.enabled = true;

            return;
        }

        if (!isPopup && CompareTag(HAND_TAG) && 
            BattleManager.Instance.state.GetStateType() == 
            BattleStateType.PlayerChoosePhase)
        {
            transform.localScale = Vector3.one * 1.1f; 
            transform.SetAsLastSibling();
            outline1.enabled = true;
            outline2.enabled = true;
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CompareTag(INVEN_TAG))
        {
            outline1.enabled = false;
            outline2.enabled = false;
            if (isPopup)
                CloseBtnActive().Forget();
            else
                transform.localScale = Vector3.one * INVEN_CARD_SCALE;
            return;
        }

        if (!isPopup && CompareTag(HAND_TAG) &&
            BattleManager.Instance.state.GetStateType() ==
            BattleStateType.PlayerChoosePhase)
        {
            transform.localScale = Vector3.one;
            transform.SetSiblingIndex(posIndex);
            outline1.enabled = false;
            outline2.enabled = false;
        }
            
    }

    public void OnScroll(BaseEventData eventData)
    {
        PointerEventData data = eventData as PointerEventData;

        if(scroll != null)
            scroll.OnScroll(data);
    }
}
