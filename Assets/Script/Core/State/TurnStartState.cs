using System.Text;
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
        manager.GetPlayerZone().ResetCardZone();
        manager.GetEnemyZone().ResetCardZone();
        manager.GetPlayerZone().CardZoneOpen();
        manager.GetEnemyZone().CardZoneOpen();

        Debug.Log("TurnStartState start");

        manager.DrawPhaseDelay().Forget();
    }

    public void OnStay()
    {
    }
}
