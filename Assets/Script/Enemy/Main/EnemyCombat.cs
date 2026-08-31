using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;

public class EnemyCombat : MonoBehaviour, ICombat
{
    public Character Character { get; set; }
    [SF] private Transform baseCharaObj;
    [SF] private Animator animator;
    [SF] private BattleCoinUI coinUI;
    [SF] private CombatAnimatorManager animatorManager;
    public Transform BaseCharaObj { get => baseCharaObj; set => baseCharaObj = value; }
    public CombatAnimatorManager AnimatorManager { get => animatorManager; set => animatorManager = value; }
    public BattleCoinUI CoinUI { get => coinUI; set => coinUI = value; }
    public Animator Animator { get => animator; set => animator = value; }

    private void Awake()
    {
        List<Card> cd = new();
        foreach (var item in ResourceManager.Instance.EnemyCardData)
        {
            cd.Add(item.Value);
        }

        Character = new Enemy(50, 20, cd);
        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;

    }

    private void Start()
    {
        BattleManager.Instance.RegisterEnemy(this);
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
        return Mathf.Max(1, (card.Value + card.Coin * card.CoinPoint) - APDiscount);
    }

    public int APDiscountByLose(Card card)
    {
        int coinVal = 0;
        if(card.Id >= 1000 && card.Id < 5000)
            coinVal = ResourceManager.Instance.CardData[card.Id].Coin;

        else if(card.Id >= 5000 && card.Id < 9000)
            coinVal = ResourceManager.Instance.EnemyCardData[card.Id].Coin;
        
        
        return card.Value + coinVal;
    }
}
