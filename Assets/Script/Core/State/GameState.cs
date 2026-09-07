using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    // 전체 상태
    public bool isTutorialCompleted;
    public int NowRound;
    public bool IsBoss;
    public bool IsBattle;
    public List<Enemy> nextRoundEnemies;

    // 맵 상태
    public MapGraph mapStatus;
    public MapNode currentMapNode;

    // 플레이어 상태
    public int gold;
    public Player playerData;
}
