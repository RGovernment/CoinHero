using System.Collections.Generic;
using UnityEngine;
using SF = UnityEngine.SerializeField;

public class PlayerCombat : MonoBehaviour, ICombat
{
    public Character Character { get; set; }
    [SF] private BattleCoinUI coinUI;
    public BattleCoinUI CoinUI { get => coinUI; set => coinUI = value; }

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
    }

    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
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
