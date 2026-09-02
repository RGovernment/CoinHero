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
        WinCk();
    }

    public void OnStay()
    {
    }

    public void WinCk()
    {
        manager.RoundEnd(!manager.GetPlayerCombat().Character.IsDead);
    }
}
