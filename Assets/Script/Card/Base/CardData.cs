using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardData : MonoBehaviour
{
    public Card cardData;
    public Character user;

    // 이하 카드 데이터 처리 함수 추가
    [Header("카드 데이터 관련")]
    [Header("코인 관련")]
    public CoinObject[] coinObject;
    public GameObject coinSlot;

    [Header("카드 외형")]
    public Image itemImage;
    public GameObject starSlot;
    public Image starImage;
    

    [Header("카드 텍스트")]
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI description;
    
}
