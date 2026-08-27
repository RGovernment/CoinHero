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
    }

    public void OnStay()
    {
    }
}
