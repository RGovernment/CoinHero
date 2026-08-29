using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public bool isPopup = false;
    public int posIndex = -1;

    private Vector3 beforeLocalPos;
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

    }

    public async UniTaskVoid OpenInfo()
    {
        if (!CompareTag("Hand")) return;
        // 확대시
        if (!isPopup && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            isPopup = true;
            beforeLocalPos = rect.localPosition;
            beforeLocalRotate = rect.localRotation;

            Sequence seq = DOTween.Sequence();
            CloseBtn.gameObject.SetActive(true);

            outline1.enabled = false;
            outline2.enabled = false;
            await seq
                .Join(rect.DOLocalMove(rect.localPosition + new Vector3(0, baseRect.sizeDelta.y, 0), 0.15f))
                .Join(rect.DOScale(Vector3.one * 1.25f, 0.15f))
                .Join(rect.DORotate(Vector3.zero, 0.15f)).ToUniTask();

            transform.SetParent(parentTransform);

            
            CloseBtn.onClick.RemoveAllListeners();
            CloseBtn.onClick.AddListener(CloseBtnToEvent);
        }
        // 선택시
        else if (!isPopup && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            outline1.enabled = false;
            outline2.enabled = false;
            BattleManager.Instance.SelectCard(this);
        }
    }
    public void OpenInfoEvent()
    {
        OpenInfo().Forget();
    }

    public async UniTaskVoid CloseBtnActive()
    {
        Sequence seq = DOTween.Sequence();
        CloseBtn.onClick.RemoveAllListeners();
        transform.SetParent(handTransform);
        transform.SetSiblingIndex(posIndex);
        await seq
            .Join(rect.DOLocalMove(beforeLocalPos, 0.15f))
            .Join(rect.DOScale(Vector3.one, 0.15f))
            .Join(rect.DOLocalRotate(beforeLocalRotate.eulerAngles, 0.15f)).ToUniTask();
        
        CloseBtn.gameObject.SetActive(false);
        isPopup = false;
    }

    public void CloseBtnToEvent()
    {
        CloseBtnActive().Forget();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isPopup && CompareTag("Hand"))
        {
            transform.SetAsLastSibling();
            outline1.enabled = true;
            outline2.enabled = true;
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isPopup && CompareTag("Hand"))
        {
            transform.SetSiblingIndex(posIndex);
            outline1.enabled = false;
            outline2.enabled = false;
        }
            
    }
}
