using UnityEngine;
public class RoundStartState : IState
{

    private BattleManager manager;

    public RoundStartState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
        Debug.Log("RoundStartState start");
        // 이번턴에 플레이어가 사용할 덱 초기화 
        manager.GetHandManager().CreateAllCard(manager.GetPlayerCombat().Character.CardList, manager.GetPlayerCombat().Character);

        foreach (var item in manager.GetEnemyCombat())
        {
            item.Character.OnDead += manager.EnemyRemove;
        }
    }

    public void OnStay()
    {
    }
}
