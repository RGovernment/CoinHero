using UnityEngine;

public class RoundEndState : IState
{

    private BattleManager manager;

    public RoundEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
        Debug.Log("RoundEndState start");
    }

    public void OnStay()
    {
    }
}
