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
    }

    public void OnStay()
    {
    }
}
