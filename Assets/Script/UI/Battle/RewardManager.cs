using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;
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
    [SF] private Button rewardCompleteBtn;

    [Header("구성 요소 / 카드 보상")]
    [SF] private Transform cardLayout;

    [Header("카드 프리팹")]
    [SF] private CardData cardPrefab;

    private int rewardCardNum = 1;
    private int rewardGold = 0;
    private int layerID = 0;
    /// <summary>
    /// 보상 완료 버튼 수, 활성화된 버튼을 모두 클릭 시 보상 종료
    /// </summary>
    private int rewardCompleteCountMax = 0;
    private int rewardCompleteCount = 0;

    private event Action OnRewardBtnClick;

    public void Awake()
    {
        layerID = SortingLayer.NameToID(REWARD_SORT_LAYER_NAME);
        
    }

    public void OnEnable()
    {
        OnRewardBtnClick += BtnClick;
    }

    public void OnDisable()
    {
        OnRewardBtnClick -= BtnClick;
    }

    private void BtnClick()
    {
        if (rewardCompleteCount >= rewardCompleteCountMax)
        {
            rewardCompleteBtn.interactable = true;
            rewardCompleteBtn.gameObject.SetActive(true);
        }
    }

    public void RewardSetting(int enemyNum)
    {
        rewardCompleteCount = 0;
        rewardGold = 0;
        RewardPanel.alpha = 0;

        RewardPanel.gameObject.SetActive(true);

        InvenBtnCanvas.sortingOrder = 101;
        InvenBtnCanvas.sortingLayerID = layerID;

        #region 골드 획득의 경우. 항상 지급

        for (int i = 0; i < enemyNum; i++) {
            int randomGold = UnityEngine.Random.Range(20, 51);
            rewardGold += randomGold;
        }
        
        goldText.text = $"골드 {rewardGold} 획득";
        goldBtn.gameObject.SetActive(true);
        if (goldBtn.gameObject.activeSelf)
            rewardCompleteCountMax++;
        #endregion

        #region 카드 획득의 경우. 카드는 상황에 따라 지급 여부가 달라질 수 있음
        // 카드를 어떤 방식으로 추가로 얻을지 정하면 {rewardCardNum} > 해당 값으로 대체
        cardText.text = $"카드 {rewardCardNum}장 획득";
        goldBtn.onClick.RemoveAllListeners();
        goldBtn.onClick.AddListener(() => GetGold(rewardGold));

        cardBtn.onClick.RemoveAllListeners();
        // 카드를 어떤 방식으로 추가로 얻을지 정하면 (rewardCardNum) > 해당 값으로 대체
        cardBtn.onClick.AddListener(() => OpenCardSelectView(rewardCardNum));

        cardBtn.gameObject.SetActive(true);
        if(cardBtn.gameObject.activeSelf)
            rewardCompleteCountMax++;
        #endregion

        #region 아티팩트 획득의 경우. 아티팩트는 상황에 따라 지급 여부가 달라질 수 있음
        // 아티팩트 획득은 추후 구현 예정
        #endregion

        RewardPanel.DOFade(ONE, DEFAULT_FADE_TIME);
    }

    public void GetGold(int gold)
    {
        GameManager.Instance.state.gold += gold;
        goldBtn.onClick.RemoveAllListeners();
        goldBtn.interactable = false;
        rewardCompleteCount++;
        OnRewardBtnClick?.Invoke();
    }

    public void OpenCardSelectView(int num)
    {
        cardBtn.enabled = false;
        cardBtn.interactable = false;
        goldBtn.onClick.RemoveAllListeners();
        CardPanel.alpha = 0;

        List<Card> cardList = ResourceManager.Instance.CardData.Values.ToList().Shuffle();
        for (int i = 0; i < num; i++)
        {
            Card card = cardList[i];

            CardMake(card);
        }

        CardPanel.gameObject.SetActive(true);
        CardPanel.DOFade(ONE, DEFAULT_FADE_TIME);

        // 지금은 랜덤, 나중에 카드 희귀도를 도입할 경우 희귀도별 가중치를 구분할 것
        
        

    }

    private void CardMake(Card card)
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
        card.canvasGroup.interactable = false;
        float cardUpgradeTime = 0.2f;
        float starStackTime = 0.5f;
        float cardStackTime = 0.5f;
        Vector3 rotateAngle = new(0, 0, 20);
        
        // 얻은 카드가 카드 목록에 존재할 경우 
        if (index > -1)
        {
            Card cardData = cardList[index];
            
            if (cardData.MaxUpgradeLv > cardData.CurrentUpgradeLv)
            {
                Debug.Log($"{card.cardData.Name} {cardData.CurrentUpgradeLv} -> {cardData.CurrentUpgradeLv + 1} 업그레이드");
                cardData.CurrentUpgradeLv++;
            }
            else
            {
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
                Debug.Log($"Star {i}");

                seq.Insert(cardUpgradeTime + starStackTime * i, star.DOScale(
                    Vector3.one * STAR_DEFAULT_EXPAND_SCALE, starStackTime / 2)
                );
                seq.Insert(cardUpgradeTime + starStackTime * i + starStackTime / 2, star.DOScale(
                    Vector3.one, starStackTime / 2)
                );

            }

            await seq.Play().ToUniTask();
        }

        // 목록에 존재하지 않을 경우
        else
        {
            Debug.Log($"카드 {card.cardData.Name} 추가");
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

        foreach (var item in GameManager.Instance.state.playerData.CardList)
            item.ToString();

        Button cardBtn = card.GetComponent<Button>();
        cardBtn.onClick.RemoveAllListeners();
        cardBtn.interactable = false;

        rewardCompleteCount++;
        OnRewardBtnClick?.Invoke();
    }

    /// <summary>
    /// 카드를 고르지 않았을 경우
    /// </summary>
    public void CloseCardSelectView()
    {
        CardPanel.DOFade(ZERO, DEFAULT_FADE_TIME);
        CardPanel.gameObject.SetActive(false);
        rewardCompleteCount++;
    }

    public void RewardCompleteCheck()
    {
        if (rewardCompleteCount >= rewardCompleteCountMax)
        {
            RewardPanel.DOFade(ZERO, DEFAULT_FADE_TIME).OnComplete(() =>
            {
                RewardPanel.gameObject.SetActive(false);
                SceneManager.LoadScene(1);
            });
        }
    }
}
