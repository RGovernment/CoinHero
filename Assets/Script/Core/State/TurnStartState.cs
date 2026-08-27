using UnityEngine;

public class TurnStartState : IState
{

    private BattleManager manager;

    public TurnStartState(BattleManager manager)
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
