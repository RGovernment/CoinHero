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
    }

    public void OnStart()
    {
        Debug.Log("Player Choose Phase start");
    }

    public void OnStay()
    {

    }
}
