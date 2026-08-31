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
        manager.enemyActionOrderCount++;
    }

    public void OnStart()
    {
        Debug.Log("TurnEndState start");
        manager.GetPlayerCombat().Character.OnDead -= manager.PlayerDead;
        manager.GetPlayerZone().ResetCardZone();
        manager.GetEnemyZone().ResetCardZone();
        manager.GetHandManager().HandDrop();
        BattleUIDisable();
        BattleEndCk();
    }

    public void OnStay()
    {
    }

    public void BattleUIDisable()
    {
        manager.GetPlayerCombat()
            .CoinUI.Release();
        manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()]
            .CoinUI.Release();
    }

    public void BattleEndCk()
    {
        if(manager.GetPlayerCombat().Character.IsDead || manager.GetEnemyCombat().Count <= 0)
            manager.state.ChangeState(manager.stateGroup[BattleStateType.RoundEnd]);
        else
            CharaReturnBasePos().Forget();
        
            
    }
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
                manager.playerBattlePoint.position, MOVE_TIMER)
            )
            .Join(
                enemy
                .transform.DOMove(manager.enemyBattlePoint.position, MOVE_TIMER)
            ).ToUniTask();

        ToggleYRotation(player.transform);
        ToggleYRotation(enemy.transform);
        player.AnimatorManager.OnIdle();
        enemy.AnimatorManager.OnIdle();

        manager.state.ChangeState(manager.stateGroup[BattleStateType.TurnStart]);
    }

    public void ToggleYRotation(Transform transform)
    {
        // 1. 현재 로컬 오일러 각도를 가져옵니다.
        Vector3 currentEuler = transform.localEulerAngles;

        float currentY = currentEuler.y % 360f;
        if (currentY < 0) currentY += 360f;

        float targetY = (currentY + 180f) % 360f;

        currentEuler.y = targetY;
        transform.localEulerAngles = currentEuler;
    }
}
