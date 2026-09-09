using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    // 전체 상태
    public bool IsTutorialCompleted;
    public int NowRound;
    public bool IsBoss;
    public bool IsBattle;
    public List<Enemy> nextRoundEnemies;

    // 맵 상태
    public MapGraphData mapStatus;

    public int currentMapNodeId;

    // 플레이어 상태
    public int gold;
    public Player playerData;
}
