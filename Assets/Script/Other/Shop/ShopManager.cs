using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;
using static Enums;
using SF = UnityEngine.SerializeField;

public class ShopManager : MonoBehaviour
{
    // 상점은 변동할 일이 없으므로 동적 생성하지 않고 고정
    [Serializable]
    public struct CardBox
    {
        public Transform cardStack;
        public TextMeshProUGUI priceText;
    }
    [Header("인벤토리 관련")]
    [SF] private OtherBattleDeckManager manager;

    [Header("상점 내 컨텐츠 관련")]
    [SF] private CardBox[] cardSlot;
    [SF] private CardData cardPrefab;
    [SF] private Transform cardDeck;

    [Header("UI 관련")]
    [SF] private CanvasGroup fadeCanvas;
    [SF] private Button exitBtn;

    private void Start()
    {
        ShopIn();
        CardSlotSetting();
    }

    private void CardSlotSetting()
    {
        Card[] getData = new Card[2];

        var cards = ResourceManager.Instance.CardData.Values
            .Where(x => x.MaxUpgradeLv > x.CurrentUpgradeLv)
            .ToList()
            .Shuffle();

        int cardGetCount = 0;

        for (int i = 0; i < getData.Length; i++)
        {
            int index = GameManager.Instance.state.playerData.CardList.FindIndex(x =>
                cards[i].Id == cards[cardGetCount].Id
            );

            if (index > -1)
            {
                Card playerCard = GameManager.Instance.state.playerData.CardList[i];
                // 카드 업그레이드 가능 횟수가 이미 최대일 경우 목록에서 제외
                if (playerCard.CurrentUpgradeLv >= playerCard.MaxUpgradeLv)
                    continue;
                getData[i] = cards[cardGetCount];

                cardGetCount++;
            }

            // 적합한 카드를 찾지 못한 경우 강제 종료
            if (cardGetCount >= cards.Count) break;
        }

        for (int i = 0; i < getData.Length; i++)
        {
            int price = UnityEngine.Random.Range(SHOP_CARD_MIN_PRICE, SHOP_CARD_MAX_PRICE);

            if (getData[i] != null)
            {
                CardBox box = cardSlot[i];
                CardData data = Instantiate(cardPrefab, box.cardStack);

                data.Init(getData[i]);

                data.cardBehind.SetActive(false);
                data.labelImage.gameObject.SetActive(true);
                data.starSlot.SetActive(true);
                data.typeIcon.gameObject.SetActive(true);
                data.gameObject.tag = REWARD_TAG;
                data.description.raycastTarget = true;

                data.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                data.transform.SetAsLastSibling();

                box.priceText.text = price.ToString();
                data.rewardAndShopBtn.enabled = true;
                data.rewardAndShopBtn.onClick.AddListener(() => CardSelectEvent(data, price));

                data.gameObject.SetActive(true);
            }
            else
            {
                CardBox box = cardSlot[i];

                box.priceText.text = "-";
            }
        }

    }
    private void CardSelectEvent(CardData data, int price)
    {
        CardSelect(data, price).Forget();
    }

#pragma warning disable CS4014
    /// <summary>
    /// 카드 선택시 작동하는 메인 함수
    /// </summary>
    /// <param name="card"></param>
    /// <returns></returns>
    private async UniTask CardSelect(CardData card, int price)
    {
        if (price > GameManager.Instance.state.gold)
        {
            DamageSkinSpawner.Instance.TextSpawn(card.transform.position,
                $"<color=#{GOLD_NOT_ENOUGH_COLOR}>골드 부족</color>");
            return;
        }

        List<Card> cardList = GameManager.Instance.state.playerData.CardList;

        int index = cardList.FindIndex(x => x.Id == card.cardData.Id);
        card.canvasGroup.interactable = false;
        float cardStackTime = CARD_GET_ANIMATION_TIME;
        Vector3 rotateAngle = new(0, 0, 20);

        SoundManager.Instance.PlaySystemSFX(SystemSoundType.Reward);

        // 얻은 카드가 카드 목록에 존재할 경우 
        if (index > -1)
        {
            Card cardData = cardList[index];

            // 업그레이드가 가능할 경우
            if (cardData.MaxUpgradeLv > cardData.CurrentUpgradeLv)
            {
                Debug.Log($"{card.cardData.Name} {cardData.CurrentUpgradeLv} -> {cardData.CurrentUpgradeLv + 1} 업그레이드");
                cardData.CurrentUpgradeLv++;
            }

            // 업그레이드가 불가능할 경우
            else
            {
                GameManager.Instance.GoldSet(DEFAULT_MAX_CARD_REWARD_GOLD);
                return;
            }

            manager.UpdateCard(cardData);
        }

        // 목록에 존재하지 않을 경우
        else
        {
            Debug.Log($"카드 {card.cardData.Name} 추가");
            cardList.Add(card.cardData);
            manager.NewDeckCardAdd(card.cardData);
        }

        // 쓴 금액 만큼 골드 감소
        GameManager.Instance.GoldSet(-price);
        Sequence seq = DOTween.Sequence();

        seq
            .Join(card.transform.DOMove(
                cardDeck.position, cardStackTime)
            )
            .Join(card.transform.DORotate(
                rotateAngle, cardStackTime)
                .SetEase(Ease.Linear)
            )
            .Join(card.transform.DOScale(
                Vector3.one * HAND_DROP_SCALE, cardStackTime)
            )
            .Join(card.canvasGroup.DOFade(
                ZERO, cardStackTime).OnComplete(() =>
                card.gameObject.SetActive(false)
                )
            );

        await seq.Play().ToUniTask();

        card.rewardAndShopBtn.onClick.RemoveAllListeners();
        card.rewardAndShopBtn.interactable = false;
        card.gameObject.SetActive(false);

        SaveManager.Instance.Save().Forget();
    }

    private void ShopIn()
    {
        fadeCanvas.alpha = 1;
        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.DOFade(ZERO, DEFAULT_FADE_TIME).OnComplete(() =>
        {
            fadeCanvas.gameObject.SetActive(false);
        });
    }

    public void ShopExitBtn()
    {
        exitBtn.interactable = false;
        ShopExit().Forget();
    }

    private async UniTask ShopExit()
    {
        fadeCanvas.alpha = 0;
        fadeCanvas.gameObject.SetActive(true);
        await fadeCanvas.DOFade(ONE, DEFAULT_FADE_TIME).ToUniTask();

        GameManager.Instance.nextScene = SceneType.Map;
        SceneManager.LoadScene((int)SceneType.Loading);
    }
}
