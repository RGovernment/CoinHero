using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using static Enums;
using static Constants;

public class BattleStartState : IState
{

    private BattleManager manager;

    public BattleStartState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
        Debug.Log("BattleStartState start");
        manager.GetPlayerZone().gameObject.SetActive(false);
        manager.GetEnemyZone().gameObject.SetActive(false);
        
        manager.GetPlayerZone().BtnClose();

        BattleTypeCheck().Forget();

    }

    public void OnStay()
    {
    }

    public async UniTaskVoid BattleTypeCheck()
    {
        // 전투 순회 연출 필요할 경우 여기에 추가
        await UniTask.DelayFrame(0);

        manager.state.ChangeState(manager.stateGroup[BattleStateType.BattlePhase]);
    }
}
