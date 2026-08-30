using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;

public class EnemyCombat : MonoBehaviour, ICombat
{
    public Character Character { get; set; }
    [SF] private BattleCoinUI coinUI;
    public BattleCoinUI CoinUI { get => coinUI; set => coinUI = value; }

    private void Awake()
    {
        List<Card> cd = new();
        foreach (var item in ResourceManager.Instance.EnemyCardData)
        {
            cd.Add(item.Value);
        }

        Character = new Enemy(50, 50, cd);
        CoinUI.gameObject.SetActive(false);
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
        return card.Value + card.Coin;
    }
}
