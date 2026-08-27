using UnityEngine;

public class StateMachine : MonoBehaviour
{
    private IState nowState;

    public void ChangeState(IState nextState)
    {
        if (nowState == null)
        {
            nowState = nextState;
            nowState?.Start();
            return;
        }

        nowState.End();
        nowState = nextState;
        nextState.Start();
    }

    public void Continue()
    {
        nowState?.Continue();
    }
}