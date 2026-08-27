using System.ComponentModel.Design;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
    public Card cardData;
    public Character user;

    [Header("카드 외형")]
    public Image itemImage;
    public GameObject starSlot;
    public Image starImage;
    public Image typeIcon;
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

    private Vector3 beforeLocalPos;
    private Quaternion beforeLocalRotate;
    private void OnEnable()
    {
        if (cardData == null) return;

        string name = cardData.Name;
        int value = cardData.Value;
        int coin = cardData.Coin;
        int coinPoint = cardData.CoinPoint;
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

    public void OpenInfo()
    {
        Debug.Log("작동함?");
        if (transform.CompareTag("Hand") && Mouse.current.rightButton.wasPressedThisFrame)
        {
            beforeLocalPos = rect.localPosition;
            beforeLocalRotate = rect.localRotation;
            Vector2 ancPos = rect.anchoredPosition;
            ancPos.y += 200;
            transform.SetAsLastSibling();

            rect.localRotation = Quaternion.identity;
            rect.tag = "HandInfo";
        }
    }
}
