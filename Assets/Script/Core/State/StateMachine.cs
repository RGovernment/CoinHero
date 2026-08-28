using UnityEngine;

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
}