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
        BattleUIDisable();
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
}
