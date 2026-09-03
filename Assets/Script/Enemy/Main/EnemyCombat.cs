using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;


public class EnemyCombat : CombatBase
{
    private static int nextEnemyInstanceId = 50;

    private void Awake()
    {
        List<Card> cd = new();
        hitMat = new();
        foreach (var item in ResourceManager.Instance.EnemyCardData)
        {
            cd.Add(item.Value);
        }

        Character = new Enemy(nextEnemyInstanceId++, $"Dummy_{name}", 20, cd);
        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
        statUI.combat = this;
        statUI.Init(Character.HP, Character.SP, Character.Sanity);
        renders = animator.transform.GetComponentsInChildren<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Character.OnDead += EnemyDead;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Character.OnDead -= EnemyDead;
    }

    private void Start()
    {
        BattleManager.Instance.RegisterEnemy(this);
    }

    public override int APDiscountByLose(Card card)
    {
        int coinVal = ResourceManager.Instance.EnemyCardData[card.Id].Coin;
        
        return card.Value + coinVal;
    }
    public void EnemyDead(Character chara)
    {
        RemoveDelay(chara).Forget();
    }

    public void DestroySelf()
    {
        Character.OnDead -= EnemyDead;
        Destroy(gameObject);
    }
}
