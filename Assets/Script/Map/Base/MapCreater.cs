using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static Enums;
using static Constants;

public class MapCreater : MonoBehaviour
{
    [Header("Map Settings")]
    // 전체 층수
    public int totalFloors = 10;

    // 한 층당 노드 개수
    public int mapWidth = 3;

    [Header("Room Configurations")]
    public List<MapConfig> mapConfigs;

    public MapGraph CreateMap()
    {
        MapGraph graph = new();

        // 전체 노드 생성
        for (int i = 0; i < totalFloors; i++)
        {
            List<MapNode> currentFloor = new();

            // 마지막층은 보스방이므로 항상 한개의 노드만 생성
            int nodesInThisFloor = (i == 0 || i == totalFloors - 1) ? 1 : mapWidth;

            for (int x = 0; x < nodesInThisFloor; x++)
            {
                MapNode node = new()
                {
                    x = x,
                    y = i
                };

                currentFloor.Add(node);
            }
            graph.floors.Add(currentFloor);
        }

        // 노드 연결
        ConnectNodes(graph);

        // 규칙에 맞게 방 타입 지정
        AssignRoomTypes(graph);

        // 라인에 상점이 하나도 없으면 랜덤 노드 하나를 상점으로 강제 변경
        EnsureShopExists(graph);

        return graph;
    }

    /// <summary>
    /// 각 줄기당 상점이 최소 하나라도 존재하도록 보장하는 함수
    /// </summary>
    /// <param name="graph"></param>
    private void EnsureShopExists(MapGraph graph)
    {
        Dictionary<int, List<MapNode>> nodesBySlot = new()
        {
            { 0, new List<MapNode>() },
            { 1, new List<MapNode>() },
            { 2, new List<MapNode>() },
        };

        for (int i = 1; i < totalFloors - 1; i++)
        {
            var floor = graph.floors[i];
            foreach (var node in floor)
            {
                int slot = GetSlotForNode(floor, node);
                if (nodesBySlot.ContainsKey(slot))
                {
                    nodesBySlot[slot].Add(node);
                }
            }
        }

        // 각 라인 별로 상점이 하나도 없으면, 그 줄기 안에서 랜덤 노드 하나를 상점으로 변경
        foreach (var kvp in nodesBySlot)
        {
            var slotNodes = kvp.Value;
            if (slotNodes.Count == 0) continue;

            bool hasShop = slotNodes.Exists(n => n.mapType == MapType.Shop);
            if (hasShop) continue;

            int randomIndex = Random.Range(0, slotNodes.Count);
            slotNodes[randomIndex].mapType = MapType.Shop;
        }
    }

    /// <summary>
    /// 노드를 연결하는 함수
    /// </summary>
    /// <param name="graph"></param>
    private void ConnectNodes(MapGraph graph)
    {
        for (int y = 0; y < graph.floors.Count - 1; y++)
        {
            var currentFloor = graph.floors[y];
            var nextFloor = graph.floors[y + 1];

            // 각 노드에서 나가는 길(1~3개) 무작위 생성
            foreach (var node in currentFloor)
            {
                if (currentFloor.Count == 1)
                {
                    // 1층은 모든 층 연결
                    for (int nextX = 0; nextX < nextFloor.Count; nextX++)
                    {
                        node.nextNodes.Add(nextFloor[nextX]);
                    }
                }
                else if (nextFloor.Count == 1)
                {
                    // 보스 직전 층 -> 보스방
                    node.nextNodes.Add(nextFloor[0]);
                }
                else
                {
                    int currentSlot = GetSlotForNode(currentFloor, node);

                    List<MapNode> candidateNodes = new();

                    // 연결 가능한 노드 탐색
                    foreach (var nextNode in nextFloor)
                    {
                        int nextSlot = GetSlotForNode(nextFloor, nextNode);
                        if (Mathf.Abs(currentSlot - nextSlot) <= 1)
                        {
                            candidateNodes.Add(nextNode);
                        }
                    }

                    // 셔플하여 랜덤 연결
                    candidateNodes.Shuffle();

                    int maxConnect = Mathf.Min(3, candidateNodes.Count);
                    float roll = Random.value;
                    int connectCount =
                        (maxConnect >= 3 && roll > NODE_LINE_BY_THREE_ABLE) ? 3 :
                        (maxConnect >= 2 && roll > NODE_LINE_BY_TWO_ABLE) ? 2 : 1;

                    for (int i = 0; i < connectCount; i++)
                        node.nextNodes.Add(candidateNodes[i]);
                    
                }
            }

            // 노드에 들어오는 길이 없다면 이전 노드와 강제 연결
            if (nextFloor.Count > 1)
            {
                foreach (var nextNode in nextFloor)
                {
                    // 노드 연결 확인
                    bool hasIncoming = currentFloor.Exists(n => n.nextNodes.Contains(nextNode));

                    if (!hasIncoming)
                    {
                        // 가장 가까운 이전 층 노드 하나를 찾아서 연결해줌
                        int nextSlot = GetSlotForNode(nextFloor, nextNode);
                        var closestNode = currentFloor
                            .OrderBy(n => Mathf.Abs(GetSlotForNode(currentFloor, n) - nextSlot))
                            .First();

                        closestNode.nextNodes.Add(nextNode);
                    }
                }
            }
        }

        // 노드 연결 확인
#if UNITY_EDITOR
        var bossNode = graph.floors[^1][0];
        foreach (var startNode in graph.floors[0])
        {
            bool canReach = CanReachBoss(startNode, bossNode);
            Debug.Assert(canReach, "노드 생성 오류 : [보스방으로 갈 수 없는 노드 존재]");
        }
#endif
    }

    /// <summary>
    /// 노드 연결 확인 (BFS)
    /// </summary>
    /// <param name="start"></param>
    /// <param name="boss"></param>
    /// <returns></returns>
    private bool CanReachBoss(MapNode start, MapNode boss)
    {
        var visited = new HashSet<MapNode>();
        var queue = new Queue<MapNode>();
        queue.Enqueue(start);
        visited.Add(start);

        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == boss) return true;

            if (cur.nextNodes == null) continue;

            foreach (var next in cur.nextNodes)
            {
                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 생성된 노드에 가중치 및 연결 규칙에 맞게 맵을 배치
    /// </summary>
    /// <param name="graph"></param>
    private void AssignRoomTypes(MapGraph graph)
    {
        for (int i = 0; i < totalFloors; i++)
        {
            foreach (var node in graph.floors[i])
            {
                // 1층은 반드시 일반 전투
                if (i == 0)
                    node.mapType = MapType.Normal;

                // 마지막 층은 반드시 보스 전투
                else if (i == totalFloors - 1)
                    node.mapType = MapType.Boss;

                else
                {
                    // 실제로 이 노드로 연결선이 들어오는 이전 층 노드들만 수집
                    var prevFloor = graph.floors[i - 1];
                    var incomingTypes = prevFloor
                        .Where(n => n.nextNodes != null && n.nextNodes.Contains(node))
                        .Select(n => n.mapType)
                        .ToHashSet();

                    node.mapType = SelectRandomRoomType(i, incomingTypes);
                }
            }
        }
    }

    private int GetSlotForNode(List<MapNode> floor, MapNode targetNode)
    {
        int index = floor.IndexOf(targetNode);
        if (floor.Count == 1) return 1; // 중앙
        if (floor.Count == 2) return (index == 0) ? 0 : 2; // 상단, 하단
        return index; // 3개일 땐 0, 1, 2
    }

    private MapType SelectRandomRoomType(int floor, HashSet<MapType> incomingTypes)
    {
        var validConfigs = mapConfigs.Where(config =>
            // 맵이 보스방이 아니면서
            config.mapType != MapType.Boss &&
            // 요구하는 최소 등장층 조건을 만족하고
            floor >= config.minFloor &&
            // 연속 등장 불가 조건이 있을 때 해당 조건을 만족하는지 확인
            (config.allowConsecutive || !incomingTypes.Contains(config.mapType))
        ).ToList();

        // 후보가 하나도 없을 경우 예외처리
        if (validConfigs.Count == 0)
        {
            validConfigs = mapConfigs.Where(config =>
                config.mapType != MapType.Boss &&
                floor >= config.minFloor
            ).ToList();
        }

        // 2. 가중치 합산
        int totalWeight = validConfigs.Sum(config => config.weight);
        int randomValue = Random.Range(0, totalWeight);

        // 3. 가중치 기반 랜덤 선택
        int currentSum = 0;
        foreach (var config in validConfigs)
        {
            currentSum += config.weight;
            if (randomValue < currentSum)
            {
                return config.mapType;
            }
        }

        return MapType.Normal; // 예외 처리 기본값
    }


    [ContextMenu("Test Print Map")]
    public void TestPrintMap()
    {
        // 기존 맵 생성 함수 호출
        MapGraph graph = CreateMap();
        PrintMapToConsole(graph);
    }

    private void PrintMapToConsole(MapGraph graph)
    {
        StringBuilder sb = new();
        sb.AppendLine("\n<b>=== [중앙 정렬 맵 시각화 (좌:1층 ➔ 우:보스방)] ===</b>\n");

        int maxSlotsInFloor = 3; // 3단 높이 기준 (0: 상단, 1: 중앙, 2: 하단)

        // 3단 높이의 각 슬롯(줄)을 순회하며 가로 방향으로 출력
        for (int slotIndex = 0; slotIndex < maxSlotsInFloor; slotIndex++)
        {
            for (int floorIndex = 0; floorIndex < graph.floors.Count; floorIndex++)
            {
                var currentFloor = graph.floors[floorIndex];

                // 현재 줄(slotIndex)에 배치될 노드 탐색
                MapNode node = GetNodeForSlot(currentFloor, slotIndex);

                if (node != null)
                {
                    string colorHex = GetRoomColorHex(node.mapType);

                    // 1. 이전 층에서 들어오는 연결선 (In) - 항상 1글자로 통일
                    string inSymbols = GetIncomingSymbols(graph, node);

                    // 2. 방 노드 텍스트 ([In-Type])
                    string nodeText = $"[{inSymbols}<color={colorHex}>{node.mapType,-6}</color>]";

                    // 3. 다음 층으로 나가는 연결선 (Out)
                    string outSymbols = GetOutgoingSymbols(graph, node);

                    sb.Append($"{nodeText}{outSymbols}");
                }
                else
                {
                    // 노드가 없는 슬롯은 열 맞춤용 공백 출력
                    sb.Append("               ");
                }
            }
            sb.AppendLine("\n"); // 줄간격 확보
        }

        sb.AppendLine("=========================================");
        Debug.Log(sb.ToString());
    }

    // [중앙 정렬 핵심] 층의 노드 개수(1, 2, 3개)에 따라 콘솔 줄(0, 1, 2)에 노드를 매핑
    private MapNode GetNodeForSlot(List<MapNode> floor, int slotIndex)
    {
        int count = floor.Count;

        // 노드 1개: 1번 슬롯(정중앙)에 배치 (예: 1층, 보스방)
        if (count == 1)
        {
            return (slotIndex == 1) ? floor[0] : null;
        }
        // 노드 2개: 0번(상단), 2번(하단) 슬롯에 대칭 배치
        else if (count == 2)
        {
            if (slotIndex == 0) return floor[0];
            if (slotIndex == 2) return floor[1];
            return null; // 1번(중앙)은 비움
        }
        // 노드 3개: 0, 1, 2번 슬롯 전체 배치
        else if (count == 3)
        {
            if (slotIndex >= 0 && slotIndex < floor.Count) return floor[slotIndex];
        }

        return null;
    }

    // 오른쪽으로 나가는 연결선 기호
    // 주의: 반드시 "실제 x값"이 아니라 "화면상 줄(slot)" 기준으로 비교해야 한다.
    // 노드가 1개뿐인 층(1층, 보스방)은 실제 x=0 이지만 화면에는 항상 가운데 줄(slot 1)에 그려지기 때문.
    private string GetOutgoingSymbols(MapGraph graph, MapNode node)
    {
        if (node.nextNodes == null || node.nextNodes.Count == 0) return "   ";

        var currentFloor = graph.floors[node.y];
        int currentSlot = GetSlotForNode(currentFloor, node);

        bool hasStraight = false;
        bool hasUp = false;   // 위 대각선 (/)
        bool hasDown = false; // 아래 대각선 (\)

        foreach (var nextNode in node.nextNodes)
        {
            var nextFloor = graph.floors[nextNode.y];
            int nextSlot = GetSlotForNode(nextFloor, nextNode);

            if (nextSlot == currentSlot) hasStraight = true;
            else if (nextSlot < currentSlot) hasUp = true;   // 다음 노드가 화면상 더 위쪽 줄
            else hasDown = true;                             // 다음 노드가 화면상 더 아래쪽 줄
        }

        StringBuilder symbols = new();
        if (hasUp) symbols.Append("/");
        if (hasStraight) symbols.Append("─");
        if (hasDown) symbols.Append("\\");

        // 출력 정렬을 위한 3자 패딩
        while (symbols.Length < 3) symbols.Append(" ");
        return symbols.ToString();
    }

    // 왼쪽에서 들어오는 연결선 기호 (항상 1글자 리턴 -> 정렬 깨짐 방지)
    // 마찬가지로 실제 x값이 아니라 화면상 줄(slot) 기준으로 비교한다.
    private string GetIncomingSymbols(MapGraph graph, MapNode currentNode)
    {
        if (currentNode.y == 0) return " "; // 1층은 들어오는 선 없음 (공백 1칸)

        var currentFloor = graph.floors[currentNode.y];
        int currentSlot = GetSlotForNode(currentFloor, currentNode);

        var prevFloor = graph.floors[currentNode.y - 1];
        bool fromUp = false;   // 화면상 위쪽 줄에서 내려옴 (\)
        bool fromDown = false; // 화면상 아래쪽 줄에서 올라옴 (/)

        foreach (var prevNode in prevFloor)
        {
            if (prevNode.nextNodes != null && prevNode.nextNodes.Contains(currentNode))
            {
                int prevSlot = GetSlotForNode(prevFloor, prevNode);
                if (prevSlot < currentSlot) fromUp = true;
                if (prevSlot > currentSlot) fromDown = true;
            }
        }

        if (fromUp && fromDown) return "X";
        if (fromUp) return "\\";
        if (fromDown) return "/";
        return " "; // 들어오는 선이 없을 때도 공백 1칸으로 통일
    }

    // 방 타입별 콘솔 리치 텍스트 색상
    private string GetRoomColorHex(MapType type)
    {
        return type switch
        {
            MapType.Normal => "#FF6B6B", // 빨강 (전투)
            MapType.Shop => "#FFE600", // 노랑 (상점)
            MapType.Boss => "#FF00FF", // 보라 (보스)
            _ => "#FFFFFF", // 기본 하양
        };
    }
}