using UnityEngine;
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
        Debug.Log("Round start");
        manager.GetPlayerZone().CardZoneOpen();
        manager.GetEnemyZone().CardZoneOpen();
    }

    public void OnStay()
    {
    }
}
