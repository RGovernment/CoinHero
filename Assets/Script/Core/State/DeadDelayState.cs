using UnityEngine;

public class DeadDelayState : IState
{
    BattleManager manager;
    public DeadDelayState(BattleManager manager)
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
