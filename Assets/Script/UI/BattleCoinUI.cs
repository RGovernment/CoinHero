using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
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
    public void Awake()
    {
        coinGroup = new();
    }

    public void CoinSet(Card card)
    {
        if(coinGroup != null && coinGroup.Count > 0)
        {
            // 코인 그룹 내 코인들의 이벤트 정리 및 리스트 비우기
            for (int i = coinGroup.Count - 1; i >= 0; i--)
            {
                CoinObject coin = coinGroup[i];

                if (coin != null)
                {
                    coin.OnBrokenComplete -= RemoveCoinFromGroup;
                    Destroy(coin);
                }
            }
        }

        coinGroup.Clear();

        nowCount = 0;
        this.card = card;
        nameText.text = card.Name;
        valueText.text = card.Value.ToString();
        nowVal = card.Value;
        iconImage.sprite = ResourceManager.Instance.CardImageData[card.Id];

        gameObject.SetActive(true);

        for (int i = coinAreaParent.childCount - 1; i >= 0; i--)
        {
            Destroy(coinAreaParent.GetChild(i).gameObject);
        }

        int makeCount = card.FinalCoin();
        Debug.Log(card.ToString());

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



    public async UniTask CoinBroken(CancellationToken cancellationToken = default)
    {
        CoinObject obj = coinGroup[^1];
        var utcs = new UniTaskCompletionSource();

        void OnCompleteHandler(CoinObject coin)
        {
            obj.OnBrokenComplete -= OnCompleteHandler;
            if (coinGroup.Contains(coin))
            {
                coinGroup.Remove(coin);
            }
            utcs.TrySetResult();
        }


        obj.OnBrokenComplete += OnCompleteHandler;
        obj.Broken();

        using (cancellationToken.Register(() =>
        {
            obj.OnBrokenComplete -= OnCompleteHandler;
            utcs.TrySetCanceled();
        }))
        {
            bool isCanceled = await utcs.Task.SuppressCancellationThrow();
            if (isCanceled)
            {
                obj.OnBrokenComplete -= OnCompleteHandler;
            }
        }
    }

    public void Release()
    {
        // 코인 그룹 내 코인들의 이벤트 정리 및 리스트 비우기
        for(int i = coinGroup.Count -1; i >= 0; i--)
        {
            CoinObject coin = coinGroup[i];

            if (coin != null)
            {
                coin.OnBrokenComplete -= RemoveCoinFromGroup;
                Destroy(coin);
            }
        }

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
