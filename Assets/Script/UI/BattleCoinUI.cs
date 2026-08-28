using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SF = UnityEngine.SerializeField;
public class BattleCoinUI : MonoBehaviour
{
    [SF] private Image iconImage;
    [SF] private TextMeshProUGUI valueText;
    [SF] private TextMeshProUGUI nameText;
    [SF] private Transform coinAreaParent;
    [SF] private CoinObject coinPrefab;
    public List<CoinObject> coinGroup;
    private CardData card;

    private int nowCount = 0;
    private int nowVal = 0;
    public void CoinSet(CardData card)
    {
        nameText.text = card.cardData.Name;
        valueText.text = card.cardData.Value.ToString();
        nowVal = card.cardData.Value;
        gameObject.SetActive(true);

        for (int i = coinAreaParent.childCount - 1; i >= 0; i--)
        {
            Destroy(coinAreaParent.GetChild(i).gameObject);
        }

        int makeCount = card.cardData.Coin;

        for (int i = 0; i < makeCount; i++) 
        {
            CoinObject obj= Instantiate(coinPrefab, coinAreaParent);
            coinGroup.Add(obj);
        }
    }

    public void CanvasOff()
    {
        gameObject.SetActive(false);
    }

    public void CoinFlip()
    {
        coinGroup[nowCount].Spin();
    }

    public void CoinStop(bool front)
    {
        nowVal += front ? card.cardData.CoinPoint : 0;
        coinGroup[nowCount].Stop(front);

        valueText.text = $"{nowVal}";
        nowCount++;
    }

    public void CoinNone()
    {
        nowCount = 0;
    }

    public void CoinBroken()
    {
        coinGroup[^1].Broken();
    }

}
