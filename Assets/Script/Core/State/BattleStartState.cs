using Cysharp.Threading.Tasks;
using static Enums;

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
        manager.SetNowPlayerCards(manager.GetPlayerZone().GetCardList());

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

        // 연출 필요시 이곳에 추가
        await UniTask.Delay(0);


        manager.state.ChangeState(manager.stateGroup[BattleStateType.BattlePhase]);
    }
}
