using static Enums;

using UnityEngine;
public class BattleEndState : IState
{

    private BattleManager manager;

    public BattleEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
        Debug.Log("BattleEndState start");
        BattleContinueCk();
    }

    public void OnStay()
    {
    }

    public void BattleContinueCk()
    {
        if(manager.GetNowPlayerCards().Count <= 0 && 
            manager.GetNowEnemyCards().Count <= 0)
        {
            Debug.Log("턴 종료");
            manager.state.ChangeState(manager.stateGroup[BattleStateType.TurnEnd]);
        }
        else
        {
            Debug.Log("다음 카드 사용");
            manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleStart]);
        }

    }
}
