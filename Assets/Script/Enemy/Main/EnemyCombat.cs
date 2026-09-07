using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyCombat : CombatBase
{

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
