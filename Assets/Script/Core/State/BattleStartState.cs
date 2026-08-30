public class BattleStartState : IState
{

    private BattleManager manager;

    public BattleStartState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
    }

    public void OnStay()
    {
    }
}
