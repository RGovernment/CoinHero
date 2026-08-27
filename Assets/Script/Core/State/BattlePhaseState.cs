using UnityEngine;

public class BattlePhaseState : IState
{

    private BattleManager manager;

    public BattlePhaseState(BattleManager manager)
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
