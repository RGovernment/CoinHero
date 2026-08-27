using TMPro;
using UnityEngine;
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
}
