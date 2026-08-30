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

    public bool[] CoinToss(Card card)
    {
        int coinCount = card.FinalCoin();

        bool[] result = new bool[coinCount];
        for (int i = 0; i < coinCount; i++)
        {
            result[i] = Character.Sanity < Random.Range(0, 100);
        }

        return result;
    }
}
