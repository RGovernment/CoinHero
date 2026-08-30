using static Enums;
public class BattleEndState : IState
{

    private BattleManager manager;

    public BattleEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
        BattleContinueCk();
    }

    public void OnStay()
    {
    }

    public void BattleContinueCk()
    {
        if(manager.GetNowPlayerCards().Count <= 0 && 
            manager.GetNowEnemyCards().Count <= 0)
        {
            manager.state.ChangeState(manager.stateGroup[BattleStateType.TurnEnd]);
        }
        else
        {
            manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleStart]);
        }

    }
}
