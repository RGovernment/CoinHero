using UnityEngine;

public class TurnEndState : IState
{

    private BattleManager manager;

    public TurnEndState(BattleManager manager)
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
