using UnityEngine;
using static Enums;
public class StateMachine
{
    private IState nowState;


    public void ChangeState(IState nextState)
    {
        if (nowState == null)
        {
            nowState = nextState;
            nowState?.OnStart();
            return;
        }

        nowState.OnEnd();
        nowState = nextState;
        nextState.OnStart();
    }

    public void Stay()
    {
        nowState?.OnStay();
    }

    public BattleStateType GetStateType()
    {
        if (nowState is RoundStartState)
            return BattleStateType.RoundStart;

        else if (nowState is TurnStartState)
            return BattleStateType.TurnStart;

        else if (nowState is DrawPhaseState)
            return BattleStateType.DrawPhase;
        else if (nowState is PlayerChoosePhaseState)
            return BattleStateType.PlayerChoosePhase;

        else if (nowState is BattleStartState)
            return BattleStateType.BattleStart;
        else if(nowState is BattlePhaseState)
            return BattleStateType.BattlePhase;
        else if (nowState is BattleEndState)
            return BattleStateType.BattleEnd;

        else if (nowState is TurnEndState)
            return BattleStateType.TurnEnd;

        else
            return BattleStateType.RoundEnd;
    }
}