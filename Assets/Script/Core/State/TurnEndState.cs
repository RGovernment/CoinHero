using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using static Enums;
using static Constants;

public class TurnEndState : IState
{

    private BattleManager manager;

    public TurnEndState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        manager.GetPlayerCombat().Character.OnDead -= manager.PlayerDead;
        manager.enemyActionOrderCount++;
    }

    public void OnStart()
    {
        Debug.Log("TurnEndState start");
        
        manager.GetPlayerZone().ResetCardZone();
        manager.GetEnemyZone().ResetCardZone();
        manager.GetHandManager().HandDrop();

        // 턴 종료 시의 디버프/버프 목록 처리 이후 캐릭터의 상태에 문제가 없을 경우(사망x 등)
        // 실행하도록 추후 변경
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

    /// <summary>
    /// 캐릭터가 원래 자리로 돌아가도록 하는 함수
    /// </summary>
    /// <returns></returns>
    public async UniTask CharaReturnBasePos()
    {
        Sequence seq = DOTween.Sequence();
        PlayerCombat player = manager.GetPlayerCombat();
        EnemyCombat enemy = manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()];
        ToggleYRotation(player.transform);
        ToggleYRotation(enemy.transform);
        player.AnimatorManager.OnMove();
        enemy.AnimatorManager.OnMove();
        // 이동 연출
        await
            seq
            .Join(
                player.transform.DOMove(
                manager.playerSpawnPoint.position, MOVE_TIMER)
            )
            .Join(
                enemy
                .transform.DOMove(manager.enemyBeforePos, MOVE_TIMER)
            ).ToUniTask();

        ToggleYRotation(player.transform);
        ToggleYRotation(enemy.transform);
        player.AnimatorManager.OnIdle();
        enemy.AnimatorManager.OnIdle();

        manager.state.ChangeState(manager.stateGroup[BattleStateType.TurnStart]);
    }

    /// <summary>
    /// Y축 180도 반전 회전
    /// </summary>
    /// <param name="transform">반전 시킬 객체</param>
    public void ToggleYRotation(Transform transform)
    {
        Vector3 currentEuler = transform.localEulerAngles;

        float currentY = currentEuler.y % 360f;
        if (currentY < 0) currentY += 360f;

        float targetY = (currentY + 180f) % 360f;

        currentEuler.y = targetY;
        transform.localEulerAngles = currentEuler;
    }
}
