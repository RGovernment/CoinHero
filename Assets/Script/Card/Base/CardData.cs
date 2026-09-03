using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
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
    public GameObject labelImage;

    [Header("기타")]
    public LayoutElement layoutElement;

    public bool isPopup = false;
    public int posIndex = -1;

    private Vector3 beforeLocalPos;
    private Vector3 beforeScale;
    private Quaternion beforeLocalRotate;
    
    private void OnEnable()
    {
        if (cardData == null) return;
        if(ResourceManager.Instance.EffectData == null) return;

        string name = cardData.Name;
        int value = cardData.FinalValue();
        int coin = cardData.FinalCoin();
        int coinPoint = cardData.FinalCoinPoint();
        int typeInt = (int)cardData.Type;
        string descriptionText 
            = cardData.GetDescription(ResourceManager.Instance.EffectData);

        description.text = descriptionText;
        nameText.text = name;
        valueText.text = value.ToString();
        coinValueText.text = coinPoint.ToString();
        coinCountText.text = $"x{coin}";
        typeIcon.sprite = typeIconSprite[typeInt];
        if(ResourceManager.Instance.CardImageData.TryGetValue(cardData.Id,out Sprite sp))
            itemImage.sprite = sp;
        description.raycastTarget = false;
    }

    public void Init(Card cardData)
    {
        this.cardData = cardData.Init(cardData);
    }


    public async UniTaskVoid OpenInfo()
    {
        if (!CompareTag(HAND_TAG) && !CompareTag(INVEN_TAG)) return;

        if (!isPopup && 
            CompareTag(INVEN_TAG) && 
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            beforeLocalRotate = rect.localRotation;
            beforeScale = rect.localScale;

            description.raycastTarget = true; 
            isPopup = true;

            Sequence seq = DOTween.Sequence();
            CloseBtn.gameObject.SetActive(true);

            outline1.enabled = false;
            outline2.enabled = false;
            await seq
            .Join(rect.DOScale(Vector3.one * CARD_EXPAND_SCALE, CARD_EXPAND_TIMER))
            .Join(rect.DORotate(Vector3.zero, CARD_EXPAND_TIMER)).ToUniTask();

            CloseBtn.onClick.RemoveAllListeners();
            CloseBtn.onClick.AddListener(CloseBtnToEvent);

            layoutElement.ignoreLayout = true;
            return;
        }

        // 확대시
        if (!isPopup && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            description.raycastTarget = true;
            isPopup = true;
            beforeLocalPos = rect.localPosition;
            beforeLocalRotate = rect.localRotation;

            Sequence seq = DOTween.Sequence();

            outline1.enabled = false;
            outline2.enabled = false;
            await seq
                .Join(rect.DOLocalMove(rect.localPosition + new Vector3(0, baseRect.sizeDelta.y, 0), CARD_EXPAND_TIMER))
                .Join(rect.DOScale(Vector3.one * CARD_EXPAND_SCALE, CARD_EXPAND_TIMER))
                .Join(rect.DORotate(Vector3.zero, CARD_EXPAND_TIMER)).ToUniTask();

            transform.SetParent(parentTransform);
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
            transform.SetSiblingIndex(posIndex);
            CloseBtn.onClick.RemoveAllListeners();
            transform.SetParent(handTransform);
            seq.Join(rect.DOLocalMove(beforeLocalPos, CARD_EXPAND_TIMER));
            seq.Join(rect.DOScale(Vector3.one, CARD_EXPAND_TIMER));
        }    
            
        if (CompareTag(INVEN_TAG))
            seq.Join(rect.DOScale(beforeScale, CARD_EXPAND_TIMER));
        
            
        
        seq.Join(rect.DOLocalRotate(beforeLocalRotate.eulerAngles, CARD_EXPAND_TIMER));

        await seq.Play().ToUniTask();
        if (CompareTag(INVEN_TAG))
            layoutElement.ignoreLayout = false;
        
        CloseBtn.gameObject.SetActive(false);
        isPopup = false;
        description.raycastTarget = false;
    }

    public void CloseBtnToEvent()
    {
        CloseBtnActive().Forget();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CompareTag(INVEN_TAG)) 
        {
            beforeScale = transform.localScale;
            transform.localScale = transform.localScale * 1.1f;
            outline1.enabled = true;
            outline2.enabled = true;

            return;
        }

        if (!isPopup && (CompareTag(HAND_TAG)) && 
            BattleManager.Instance.state.GetStateType() == 
            Enums.BattleStateType.PlayerChoosePhase)
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
                transform.localScale = beforeScale;
            return;
        }

        if (!isPopup && CompareTag(HAND_TAG) &&
            BattleManager.Instance.state.GetStateType() ==
            Enums.BattleStateType.PlayerChoosePhase)
        {
            transform.localScale = Vector3.one;
            transform.SetSiblingIndex(posIndex);
            outline1.enabled = false;
            outline2.enabled = false;
        }
            
    }
}
