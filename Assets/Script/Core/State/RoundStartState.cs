using UnityEngine;

public class RoundStartState : MonoBehaviour, IState
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
    }

    public void OnStay()
    {
    }
}
