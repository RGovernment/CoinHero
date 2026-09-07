using UnityEditor;
using UnityEngine;
public class RoundStartState : IState
{

    private BattleManager manager;

    public RoundStartState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        manager.GetPlayerCombat().Character.OnDead += manager.PlayerDead;
    }

    public void OnStart()
    {
        Debug.Log("RoundStartState start");
        // 이번턴에 플레이어가 사용할 덱 초기화 

        manager.GetHandManager().CreateAllCard(manager.GetPlayerCombat().Character.CardList, manager.GetPlayerCombat().Character);

        CharaPosSet();
    }

    public void OnStay()
    {
    }

    /// <summary>
    /// 적 및 플레이어의 시작 위치 배치
    /// </summary>
    public void CharaPosSet()
    {
        manager.GetPlayerCombat().transform.position = manager.playerSpawnPoint.position;
        
        int enemyCount = manager.GetEnemyCombat().Count;
        manager.totalEnemy = enemyCount;
        // 혼자일 경우 중앙
        if (enemyCount == 1)
            manager.GetEnemyCombat()[0].transform.position
                = manager.enemySpawnPoint[0].position;
        // 둘일 경우 위/아래
        else if (enemyCount == 2)
        {
            manager.GetEnemyCombat()[0].transform.position
                = manager.enemySpawnPoint[1].position;
            manager.GetEnemyCombat()[1].transform.position
                = manager.enemySpawnPoint[2].position;
        }
        // 셋일 경우 모든 위치
        else if (enemyCount == 3) 
        {
            for (int i = 0; i < enemyCount; i++)
            {
                manager.GetEnemyCombat()[i].transform.position 
                    = manager.enemySpawnPoint[i].position;
            }
        }
    }
}
