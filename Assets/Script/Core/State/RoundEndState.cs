using Cysharp.Threading.Tasks;
using UnityEngine;

public class RoundEndState : IState
{

    private BattleManager manager;

    public RoundEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        
    }

    public void OnStart()
    {
        Debug.Log("RoundEndState start");
        WinCk().Forget();
    }

    public void OnStay()
    {
    }

    public async UniTask WinCk()
    {
        await manager.RoundEnd(!manager.GetPlayerCombat().Character.IsDead);
    }
}
