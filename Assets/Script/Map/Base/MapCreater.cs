using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Constants;
using static Enums;

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
        int nodeId = 0;
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
                    id = nodeId++,
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
}