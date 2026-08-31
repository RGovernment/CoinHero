using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;

public class PlayerCombat : MonoBehaviour, ICombat
{
    public Character Character { get; set; }
    
    [SF] private Animator animator;
    [SF] private BattleCoinUI coinUI;
    [SF] private CombatAnimatorManager animatorManager;
    public CombatAnimatorManager AnimatorManager { get => animatorManager; set => animatorManager = value; }
    public BattleCoinUI CoinUI { get => coinUI; set => coinUI = value; }
    public Animator Animator { get => animator; set => animator = value; }

    private void Awake()
    {
        // 임시 데이터, 플레이어별 시작 카드 프리셋을 만들어둘 것
        List<Card> cd = new();
        foreach (var item in ResourceManager.Instance.CardData)
        {
            cd.Add(item.Value);
        }

        Character = new Player(10, 50, cd);
        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
    }

    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
    }

    public bool[] CoinToss(Card card, int SanitySet = -1)
    {
        int coinCount = card.FinalCoin();

        bool[] result = new bool[coinCount];
        for (int i = 0; i < coinCount; i++)
        {
            result[i] = (SanitySet < 0 ? Character.Sanity : SanitySet) < Random.Range(0, 100);
        }

        return result;
    }
    
    public int TotalValueByWin(Card card, int APDiscount = 0)
    {
        // 승리 시 밸류 + 남은 코인 * 코인 위력 리턴
        return Mathf.Max(1, (card.Value  + card.Coin * card.CoinPoint) - APDiscount);
    }

    public int APDiscountByLose(Card card)
    {
        return card.Value + card.Coin;
    }
}
