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
        manager.GetPlayerZone().CardZoneOpen();
        manager.GetEnemyZone().CardZoneOpen();

        Debug.Log("TurnStartState start");
        
        StringBuilder sb = new();

        sb.Append($"플레이어 HP : {manager.GetPlayerCombat().Character.HP}\n");

        foreach(EnemyCombat cb in manager.GetEnemyCombat())
        {
            sb.Append($"적 HP : {cb.Character.HP}\n");
        }

        Debug.Log(sb);

        manager.DrawPhaseDelay().Forget();
    }

    public void OnStay()
    {
    }
}
