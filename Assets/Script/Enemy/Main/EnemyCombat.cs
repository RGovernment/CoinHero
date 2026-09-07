using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyCombat : CombatBase
{
    private void Awake()
    {
        hitMat = new();

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
