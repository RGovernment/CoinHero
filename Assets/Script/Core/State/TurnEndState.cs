using UnityEngine;
using static Enums;

public class TurnEndState : IState
{

    private BattleManager manager;

    public TurnEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        manager.enemyActionOrderCount++;
    }

    public void OnStart()
    {
        Debug.Log("TurnEndState start");
        manager.GetPlayerZone().ResetCardZone();
        manager.GetEnemyZone().ResetCardZone();
        manager.GetHandManager().HandDrop();
        BattleUIDisable();
        BattleEndCk();
    }

    public void OnStay()
    {
    }

    public void BattleUIDisable()
    {
        manager.GetPlayerCombat()
            .CoinUI.Release();
        manager.GetEnemyCombat()[manager.enemyActionOrderCount % manager.GetEnemyCombatCount()]
            .CoinUI.Release();
    }

    public void BattleEndCk()
    {
        if(manager.GetPlayerCombat().Character.IsDaed || manager.GetEnemyCombat().Count <= 0)
            manager.state.ChangeState(manager.stateGroup[BattleStateType.RoundEnd]);
        else
            manager.state.ChangeState(manager.stateGroup[BattleStateType.TurnStart]);
    }
}
