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

        // 1. 플레이어 카드 드로우
        manager.GetHandManager().CreateAllCard(manager.GetPlayerCombat().Character.CardList, manager.GetPlayerCombat().Character);
        manager.GetHandManager().ShuffleAndSettingHand();
        manager.GetHandManager().HandActive();
        manager.GetHandManager().UpdateHandPos();

        // 2. 적 카드 오픈 및 큐 적재
        EnemyCardOpen();

        // 선택 단계로 상태 전환
        manager.state.ChangeState(manager.stateGroup[BattleStateType.PlayerChoosePhase]);
    }

    public void OnEnd()
    {
        
    }



    public void OnStay()
    {
    }

    public void EnemyCardOpen()
    {
        if (manager.GetEnemyCombatCount() == 0)
        {
            return;
        }

        List<Card> dummy = manager.GetEnemyHandManager()
            .CardSelect(manager.GetEnemyCombat()[manager.enemyActionOrderCount % manager.GetEnemyCombatCount()]
            .Character.CardList);

        manager.GetNowEnemyCards().Clear();

        foreach (var item in dummy)
        {
            manager.GetNowEnemyCards().Enqueue(item);
            BehindCardData bd = manager.GetEnemyHandManager().CardCreate(item);
            bd.SetTypeIcon();
            manager.GetEnemyZone().SetCardToEnemyZone(bd);
        }
    }
}
