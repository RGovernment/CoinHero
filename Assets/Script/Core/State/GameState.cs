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

    public MapGraph mapStatus;

    public MapNode currentMapNode;
    //세이브 로드 구현중 빌드를 위한 임시 제거
    // 맵 상태
    //public MapGraphData mapStatus;

    //public int currentMapNodeId;

    // 플레이어 상태
    public int gold;
    public Player playerData;
}
