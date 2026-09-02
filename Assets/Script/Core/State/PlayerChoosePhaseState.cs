using UnityEngine;
using static Enums;

public class PlayerChoosePhaseState : IState
{

    private BattleManager manager;

    public PlayerChoosePhaseState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        manager.SetNowPlayerCards(manager.GetPlayerZone().GetCardList());
    }

    public void OnStart()
    {
        Debug.Log("PlayerChoosePhase start");
    }

    public void OnStay()
    {

    }

    
}
