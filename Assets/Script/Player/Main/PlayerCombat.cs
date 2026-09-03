using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CombatBase
{

    private void Awake()
    {
        // 임시 데이터, 플레이어별 시작 카드 프리셋을 만들어둘 것
        List<Card> cd = new();
        hitMat = new();
        foreach (var item in ResourceManager.Instance.CardData)
        {
            cd.Add(item.Value);
        }

        Character = new Player(10, "플레이어", 25, cd);
        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
        statUI.combat = this;
        statUI.Init(Character.HP, Character.SP, Character.Sanity);
        renders = animator.transform.GetComponentsInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
    }

    public override int APDiscountByLose(Card card)
    {
        int coinVal = ResourceManager.Instance.CardData[card.Id].Coin;

        return card.Value + coinVal;
    }
}
