using UnityEngine;

public class PlayerCompleteState : IState
{

    private BattleManager manager;

    public PlayerCompleteState(BattleManager manager)
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
