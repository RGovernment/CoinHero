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

        foreach (var item in manager.GetEnemyCombat())
        {
            item.Character.OnDead += manager.EnemyRemove;
        }
        CharaPosSet();
    }

    public void OnStay()
    {
    }

    public void CharaPosSet()
    {
        manager.GetPlayerCombat().transform.position = manager.playerSpawnPoint.position;
        int enemyCount = manager.GetEnemyCombat().Count;
        if (enemyCount == 1)
            manager.GetEnemyCombat()[0].transform.position
                = manager.enemySpawnPoint[0].position;
        else if (enemyCount == 2)
        {
            manager.GetEnemyCombat()[0].transform.position
                = manager.enemySpawnPoint[1].position;
            manager.GetEnemyCombat()[1].transform.position
                = manager.enemySpawnPoint[2].position;
        }
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
