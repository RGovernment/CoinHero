using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Enums;

[Serializable]
public class MapNode
{
    public int id;

    public int x; // 층 내에서의 위치 (시각적으로는 세로)
    public int y; // 층수 (시각적으로는 가로)

    public MapType mapType;

    public bool isVisited;
    public bool isCurrent;
    
    public List<MapNode> nextNodes = new();
}

[Serializable]
public class MapNodeData
{
    public int id;

    public int x;
    public int y;

    public MapType mapType;

    public bool isVisited;

    public List<int> nextNodeIds = new();
}

public class MapGraph
{
    // 각 층(y)별 노드(x) 리스트
    public List<List<MapNode>> floors = new();
}

[Serializable]
public class MapGraphData
{
    public List<MapNodeData> nodes = new();
}

[Serializable]
public class MapConfig
{
    public MapType mapType;
    // 등장 가중치
    public int weight;
    // 최소 등장 층수
    public int minFloor;
    // 연속 등장 가능 여부
    public bool allowConsecutive;
}
