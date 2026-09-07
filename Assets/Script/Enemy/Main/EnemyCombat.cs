using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyCombat : CombatBase
{
    public override void Init(Character chara)
    {
        Character = chara;
        hitMat = new();

        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
        statUI.combat = this;
        statUI.Init(Character.HP, Character.SP, Character.Sanity);
        renders = animator.transform.GetComponentsInChildren<SpriteRenderer>();

        BattleManager.Instance.RegisterEnemy(this);
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
