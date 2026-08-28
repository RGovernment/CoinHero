using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Player Player { get; set; }

    private void Awake()
    {
        // 임시 데이터, 플레이어별 시작 카드 프리셋을 만들어둘 것
        List<Card> cd = new();
        foreach (var item in ResourceManager.Instance.CardData)
        {
            cd.Add(item.Value);
        }

        Player = new Player(10,50, cd);
    }

    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
    }
}
