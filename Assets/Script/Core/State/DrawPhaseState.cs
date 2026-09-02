using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class DrawPhaseState : IState
{

    private BattleManager manager;

    public DrawPhaseState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnStart()
    {
        // 드로우 단계 시작
        Debug.Log("DrawPhaseState start");
        manager.GetPlayerZone().gameObject.SetActive(true);
        manager.GetEnemyZone().gameObject.SetActive(true);
        manager.GetHandManager().isDrawTime = true;
        // 1. 플레이어 카드 드로우
        manager.GetHandManager().ShuffleAndSettingHand();
        manager.GetHandManager().HandActive();
        manager.GetHandManager().UpdateHandPos();

        // 2. 적 카드 오픈 및 큐 적재
        EnemyCardOpen();
        
        // 선택 단계로 상태 전환
        DrawFinishWait().Forget();
    }

    public void OnEnd()
    {
        
    }



    public void OnStay()
    {
    }

    public async UniTask DrawFinishWait()
    {
        await manager.GetHandManager().cardDrawTrigger.Task;
        manager.GetHandManager().isDrawTime = false;
        manager.state.ChangeState(manager.stateGroup[BattleStateType.PlayerChoosePhase]);
    }

    public void EnemyCardOpen()
    {
        if (manager.GetEnemyCombat().Count == 0)
        {
            return;
        }

        List<Card> dummy = manager.GetEnemyHandManager()
            .CardSelect(manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()]
            .Character.CardList);

        manager.GetNowEnemyCards().Clear();

        foreach (var item in dummy)
        {
            Card card = item.Init(item);

            manager.GetNowEnemyCards().Enqueue(card);
            BehindCardData bd = manager.GetEnemyHandManager().CardCreate(card);
            bd.SetTypeIcon();
            manager.GetEnemyZone().SetCardToEnemyZone(bd);
        }
    }
}
