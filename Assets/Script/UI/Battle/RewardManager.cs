using DG.Tweening;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using static Constants;

using SF = UnityEngine.SerializeField;
using Unity.VisualScripting;

public class RewardManager : MonoBehaviour
{
    [Header("캔버스 컨트롤")]
    [SF] private Canvas InvenBtnCanvas;
    [SF] private CanvasGroup RewardPanel;
    [SF] private CanvasGroup CardPanel;

    [Header("구성 요소 / 버튼")]
    [SF] private Button goldBtn;
    [SF] private TextMeshProUGUI goldText;
    [SF] private Button cardBtn;
    [SF] private TextMeshProUGUI cardText;
    [SF] private Button giveUpBtn;

    [Header("구성 요소 / 카드 보상")]
    [SF] private Transform cardLayout;

    [Header("카드 프리팹")]
    [SF] private CardData cardPrefab;

    private int rewardCardNum = 1;
    private int rewardGold = 0;

    public void RewardSetting(int enemyNum)
    {
        RewardPanel.alpha = 0;
        InvenBtnCanvas.sortingLayerName = "StatusUI";
        rewardGold = 0;
        for (int i = 0; i < enemyNum; i++) {
            int randomGold = Random.Range(20, 51);
            rewardGold += randomGold;
        }
        
        goldText.text = $"골드 {rewardGold} 획득";
        goldBtn.gameObject.SetActive(true);
        // 카드를 어떤 방식으로 추가로 얻을지 정하면 {rewardCardNum} > 해당 값으로 대체
        cardText.text = $"카드 {rewardCardNum}장 획득";
        goldBtn.onClick.RemoveAllListeners();
        goldBtn.onClick.AddListener(() => GetGold(rewardGold));

        cardBtn.onClick.RemoveAllListeners();
        // 카드를 어떤 방식으로 추가로 얻을지 정하면 (rewardCardNum) > 해당 값으로 대체
        cardBtn.onClick.AddListener(() => OpenCardSelectView(rewardCardNum));
        cardBtn.gameObject.SetActive(true);

        RewardPanel.DOFade(ONE, 0.3f);
    }

    public void GetGold(int gold)
    {
        GameManager.Instance.state.gold += gold;
        goldBtn.onClick.RemoveAllListeners();
        goldBtn.enabled = false;
    }

    public void OpenCardSelectView(int num)
    {
        cardBtn.enabled = false;
        goldBtn.onClick.RemoveAllListeners();
        CardPanel.alpha = 0;

        List<Card> cardList = ResourceManager.Instance.CardData.Values.ToList().Shuffle();
        for (int i = 0; i < num; i++)
        {
            Card card = cardList[i];

            CardData data = CardMake(card);
        }

        CardPanel.gameObject.SetActive(true);
        CardPanel.DOFade(ONE, 0.3f);

        // 지금은 랜덤, 나중에 카드 희귀도를 도입할 경우 희귀도별 가중치를 구분할 것
        
        

    }

    private CardData CardMake(Card card)
    {
        CardData data = Instantiate(cardPrefab, cardLayout);
        data.Init(card);
        data.AddComponent<Button>().onClick.AddListener(() => CardSelect(data));
        data.gameObject.SetActive(true);
        return data;
    }

    private void CardSelect(CardData card)
    {
        List<Card> cardList = GameManager.Instance.state.playerData.CardList;

        int index = cardList.FindIndex(x => card.cardData.Id == card.cardData.Id);

        float cardUpgradeTime = 0.2f;
        float cardStackTime = 0.5f;
        Vector3 rotateAngle = new(0, 0, 40);

        if (index > -1)
        {
            Card cardData = cardList[index];

            if (cardData.MaxUpgradeLv > cardData.CurrentUpgradeLv)
            {
                cardData.CurrentUpgradeLv++;
            }
            else
            {
                // 업그레이드 불가능 처리 대신 돈 올라감
                GameManager.Instance.state.gold += DEFAULT_MAX_CARD_REWARD_GOLD;
                return;
            }

            for (int i = 0; i < cardData.CurrentUpgradeLv; i++)
            {
                card.starSlot.transform.GetChild(i);
            }
            DG.Tweening.Sequence seq = DOTween.Sequence();
            
            seq
                .Join(card.transform.DOScale(
                    Vector3.one * CARD_DEFAULT_EXPAND_SCALE, cardUpgradeTime)
                )
                .Insert(cardUpgradeTime, card.transform.DOScale(
                    Vector3.one , cardUpgradeTime)
                );

            for(int i = 0; i <cardData.CurrentUpgradeLv; i++)
            {
                Transform star = card.starSlot.transform.GetChild(i);
                
                star.DOPunchScale(Vector3.forward, cardUpgradeTime, 3, 1);
            }
        }
        else
        {
            DG.Tweening.Sequence seq = DOTween.Sequence();

            cardList.Add(card.cardData);
            seq
                .Join(card.transform.DOMove(
                    InvenBtnCanvas.transform.position, cardStackTime)
                )
                .Join(card.transform.DORotate(
                    rotateAngle, cardStackTime)
                )
                .Join(card.canvasGroup.DOFade(
                    ZERO, cardStackTime).OnComplete(() =>
                    card.gameObject.SetActive(false)
                    )
                );
        }
    }

    /// <summary>
    /// 카드를 고르지 않았을 경우
    /// </summary>
    public void CloseCardSelectView()
    {
        CardPanel.DOFade(ZERO, 0.3f);
        CardPanel.gameObject.SetActive(false);
    }
}
