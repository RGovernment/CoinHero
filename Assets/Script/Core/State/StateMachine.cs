using UnityEngine;
using static Enums;
public class StateMachine
{
    private IState nowState;
    private IState beforeState;

    /// <summary>
    /// 상태를 전환하는 함수
    /// </summary>
    /// <param name="nextState"></param>
    public void ChangeState(IState nextState)
    {
        if (nowState == null)
        {
            nowState = nextState;
            nowState?.OnStart();
            return;
        }

        nowState.OnEnd();
        beforeState = nowState;
        nowState = nextState;
        nextState.OnStart();
    }

    public void Stay()
    {
        nowState?.OnStay();
    }

    /// <summary>
    /// 현재 / 이전 상태가 무엇이었는지 가져오는 함수
    /// </summary>
    /// <param name="now">현재 = true / 이전 = false</param>
    /// <returns>호출된 지점의 상태값</returns>
    public BattleStateType GetStateType(bool now = true)
    {
        IState callState = now ? nowState : beforeState;

        if (callState is RoundStartState)
            return BattleStateType.RoundStart;

        else if (callState is TurnStartState)
            return BattleStateType.TurnStart;

        else if (callState is DrawPhaseState)
            return BattleStateType.DrawPhase;
        else if (callState is PlayerChoosePhaseState)
            return BattleStateType.PlayerChoosePhase;

        else if (callState is BattleStartState)
            return BattleStateType.BattleStart;
        else if (callState is BattlePhaseState)
            return BattleStateType.BattlePhase;
        else if (callState is BattleEndState)
            return BattleStateType.BattleEnd;

        else if (callState is TurnEndState)
            return BattleStateType.TurnEnd;

        else if (callState is RoundEndState)
            return BattleStateType.RoundEnd;

        else
            return BattleStateType.DeadDelay;
    }
}