using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using static Enums;
using static Constants;
using static Utility;

public class TurnEndState : IState
{

    private BattleManager manager;

    public TurnEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        if (!manager.EnemyDeadTurn)
            manager.enemyActionOrderCount++;
        manager.EnemyDeadTurn = false;
        manager.GetPlayerCombat().Character.OnDead -= manager.PlayerDead;
    }

    public void OnStart()
    {
        Debug.Log("TurnEndState start");
        if(manager.state.GetStateType(false) != BattleStateType.DeadDelay)
        {
            manager.GetPlayerZone().ResetCardZone();
            manager.GetEnemyZone().ResetCardZone();
            manager.GetHandManager().HandDrop();

            // 턴 종료 시의 디버프/버프 목록 처리 추가


        }

        BattleUIDisable();
        BattleEndCk();
    }

    public void OnStay()
    {
    }

    /// <summary>
    /// 전투중 활성화된 UI 초기화
    /// </summary>
    public void BattleUIDisable()
    {
        manager.GetPlayerCombat()
            .CoinUI.Release();
        if(manager.GetEnemyCombat().Count > 0 &&
            (manager.state.GetStateType(false) != BattleStateType.DeadDelay || 
            manager.EnemyDeadTurn != true))
            manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()]
                .CoinUI.Release();
    }

    /// <summary>
    /// 전투가 끝났는지 확인하고, 아닐 경우 다음 턴, 맞을 경우 전투 종료
    /// </summary>
    public void BattleEndCk()
    {
        if(manager.GetPlayerCombat().Character.IsDead || manager.GetEnemyCombat().Count <= 0)
            manager.state.ChangeState(manager.stateGroup[BattleStateType.RoundEnd]);
        else
            CharaReturnBasePos().Forget();
    }
    #pragma warning disable CS4014
    /// <summary>
    /// 캐릭터가 원래 자리로 돌아가도록 하는 함수
    /// </summary>
    /// <returns></returns>
    public async UniTask CharaReturnBasePos()
    {
        Sequence seq = DOTween.Sequence().Pause();

        PlayerCombat player = manager.GetPlayerCombat();
        EnemyCombat enemy = null;

        // 플레이어 사망 시 스킵
        if (!player.Character.IsDead)
        {
            // 플레이어 이동
            ToggleYRotation(player.transform);
            player.AnimatorManager.OnMove();
            seq
              .Join(
                  player.transform.DOMove(
                  manager.playerSpawnPoint.position, MOVE_TIMER)
              );
        }

        // 적 전멸시 스킵
        if (manager.GetEnemyCombat().Count > 0 ||
            manager.state.GetStateType(false) != BattleStateType.DeadDelay ||
            manager.EnemyDeadTurn != true)
        {
            enemy = manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()];
            ToggleYRotation(enemy.transform);
            enemy.AnimatorManager.OnMove();
            seq.Join(
                enemy
                .transform.DOMove(manager.enemyBeforePos, MOVE_TIMER)
            );
        }
        
        // 이동 연출
        await seq.Play().ToUniTask();

        if (!player.Character.IsDead)
        {
            ToggleYRotation(player.transform);
            player.AnimatorManager.OnIdle();
        }

        // 
        if(manager.GetEnemyCombat().Count > 0 ||
           manager.state.GetStateType(false) != BattleStateType.DeadDelay ||
            manager.EnemyDeadTurn != true)
        {
            ToggleYRotation(enemy.transform);

            enemy.AnimatorManager.OnIdle();
        }
        

        manager.state.ChangeState(manager.stateGroup[BattleStateType.TurnStart]);
    }
}
