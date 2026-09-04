using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static UnityEditor.Experimental.GraphView.GraphView;
using SF = UnityEngine.SerializeField;

public class RewardManager : MonoBehaviour
{
    [Header("캔버스 컨트롤")]
    [SF] private Canvas InvenBtnCanvas;
    [SF] private Transform CardDeck;
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
    private int layerID = 0;
    public void Awake()
    {
        layerID = SortingLayer.NameToID(REWARD_SORT_LAYER_NAME);
        
    }

    public void RewardSetting(int enemyNum)
    {
        rewardGold = 0;
        RewardPanel.alpha = 0;

        RewardPanel.gameObject.SetActive(true);

        InvenBtnCanvas.sortingOrder = 101;
        InvenBtnCanvas.sortingLayerID = layerID;
        

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

        RewardPanel.DOFade(ONE, DEFAULT_FADE_TIME);
    }

    public void GetGold(int gold)
    {
        GameManager.Instance.state.gold += gold;
        goldBtn.onClick.RemoveAllListeners();
        goldBtn.interactable = false;
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
        CardPanel.DOFade(ONE, DEFAULT_FADE_TIME);

        // 지금은 랜덤, 나중에 카드 희귀도를 도입할 경우 희귀도별 가중치를 구분할 것
        
        

    }

    private CardData CardMake(Card card)
    {
        CardData data = Instantiate(cardPrefab, cardLayout);
        data.Init(card);
        data.cardBehind.SetActive(false);
        data.labelImage.gameObject.SetActive(true);
        data.starSlot.SetActive(true);
        data.typeIcon.gameObject.SetActive(true);
        data.gameObject.tag = REWARD_TAG;
        data.AddComponent<Button>().onClick.AddListener(() => CardSelectEvent(data));
        data.gameObject.SetActive(true);
        return data;
    }

    private void CardSelectEvent(CardData data)
    {
        CardSelect(data).Forget();
    }

#pragma warning disable CS4014
    private async UniTask CardSelect(CardData card)
    {
        List<Card> cardList = GameManager.Instance.state.playerData.CardList;
        
        int index = cardList.FindIndex(x => x.Id == card.cardData.Id);

        float cardUpgradeTime = 0.2f;
        float cardStackTime = 0.5f;
        Vector3 rotateAngle = new(0, 0, 20);
        Debug.Log(index);
        card.GetComponent<Button>().onClick.RemoveAllListeners();
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
                card.starSlot.transform.GetChild(i).gameObject.SetActive(true);
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
                
                seq.Join(star.DOPunchScale(Vector3.forward, cardUpgradeTime, 3, 1));
            }

            await seq.Play().ToUniTask();
        }
        else
        {
            DG.Tweening.Sequence seq = DOTween.Sequence();

            cardList.Add(card.cardData);
            seq
                .Join(card.transform.DOMove(
                    CardDeck.position, cardStackTime)
                )
                .Join(card.transform.DORotate(
                    rotateAngle, cardStackTime)
                )
                .Join(card.canvasGroup.DOFade(
                    ZERO, cardStackTime).OnComplete(() =>
                    card.gameObject.SetActive(false)
                    )
                );

            await seq.Play().ToUniTask();
        }

        CardPanel.DOFade(ZERO, DEFAULT_FADE_TIME);
        CardPanel.gameObject.SetActive(false);
    }

    /// <summary>
    /// 카드를 고르지 않았을 경우
    /// </summary>
    public void CloseCardSelectView()
    {
        CardPanel.DOFade(ZERO, DEFAULT_FADE_TIME);
        CardPanel.gameObject.SetActive(false);
    }
}
