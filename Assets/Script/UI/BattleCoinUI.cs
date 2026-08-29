using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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
    private Card card;

    private int nowCount = 0;
    private int nowVal = 0;
    public void CoinSet(Transform chara, Card card)
    {
        nowCount = 0;
        this.card = card;
        nameText.text = card.Name;
        valueText.text = card.Value.ToString();
        nowVal = card.Value;
        transform.SetParent(chara);

        gameObject.SetActive(true);

        for (int i = coinAreaParent.childCount - 1; i >= 0; i--)
        {
            Destroy(coinAreaParent.GetChild(i).gameObject);
        }

        int makeCount = card.FinalCoin();

        for (int i = 0; i < makeCount; i++) 
        {
            CoinObject obj = Instantiate(coinPrefab, coinAreaParent);
            obj.SetCoinRectByBattleUI();
            obj.OnBrokenComplete += RemoveCoinFromGroup;
            coinGroup.Add(obj);
        }
    }

    public void CanvasOff()
    {
        gameObject.SetActive(false);
    }

    public void CoinFlip()
    {
        nowVal = card.Value;
        foreach (var item in coinGroup)
        {
            item.Spin();
        }
        nowCount = 0;
    }

    public void CoinStop(bool front)
    {
        nowVal += front ? card.FinalCoinPoint() : 0;
        coinGroup[nowCount].Stop(front);

        valueText.text = $"{nowVal}";
        nowCount++;
    }

    public void CoinNone()
    {
        nowCount = 0;
    }

    public async UniTask CoinBroken()
    {
        CoinObject obj = coinGroup[^1];
        

        var utcs = new UniTaskCompletionSource();

        Action<CoinObject> onComplete = null;
        onComplete = (coin) =>
        {
            obj.OnBrokenComplete -= onComplete;
            coinGroup.Remove(coin);
            utcs.TrySetResult(); // 대기 중인 await를 풀어줍니다.
        };

        obj.OnBrokenComplete += onComplete;
        obj.Broken();

        await utcs.Task;
    }

    public void Release()
    {
        coinGroup.Clear();
        gameObject.SetActive(false);
    }

    private void RemoveCoinFromGroup(CoinObject coin)
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        coin.OnBrokenComplete -= RemoveCoinFromGroup;

        // 리스트에서 제거
        if (coinGroup.Contains(coin))
            coinGroup.Remove(coin);
    }
}
