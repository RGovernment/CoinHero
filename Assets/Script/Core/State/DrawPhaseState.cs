using UnityEngine;

public class DrawPhaseState : IState
{

    private BattleManager manager;

    public DrawPhaseState(BattleManager manager)
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
