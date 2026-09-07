using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utility
{    
     /// <summary>
     /// Y축 180도 반전 회전
     /// </summary>
     /// <param name="transform">반전 시킬 객체</param>
    public static void ToggleYRotation(Transform transform)
    {
        Vector3 currentEuler = transform.localEulerAngles;

        float currentY = currentEuler.y % 360f;
        if (currentY < 0) currentY += 360f;

        float targetY = (currentY + 180f) % 360f;

        currentEuler.y = targetY;
        transform.localEulerAngles = currentEuler;
    }


    /// <summary>
    /// 맵 데이터 저장용 데이터 변환 함수
    /// </summary>
    /// <param name="graph"></param>
    /// <returns></returns>
    public static MapGraphData SaveMapSaveData(MapGraph graph)
    {
        MapGraphData data = new();

        List<MapNode> allNodes = graph.floors
            .SelectMany(floor => floor)
            .ToList();

        foreach (MapNode node in allNodes)
        {
            MapNodeData nodeData = new()
            {
                id = node.id,
                x = node.x,
                y = node.y,
                mapType = node.mapType,
                isVisited = node.isVisited
            };

            foreach (MapNode nextNode in node.nextNodes)
            {
                nodeData.nextNodeIds.Add(nextNode.id);
            }

            data.nodes.Add(nodeData);
        }

        return data;
    }


    /// <summary>
    /// 세이브용 맵 데이터를 로드해 다시 저장하는 함수
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static MapGraph LoadMapSaveData(MapGraphData data)
    {
        MapGraph graph = new();

        // ID -> 실제 MapNode
        Dictionary<int, MapNode> nodeDictionary = new();

        foreach (MapNodeData nodeData in data.nodes)
        {
            MapNode node = new()
            {
                id = nodeData.id,
                x = nodeData.x,
                y = nodeData.y,
                mapType = nodeData.mapType,
                isVisited = nodeData.isVisited
            };

            nodeDictionary.Add(node.id, node);

            while (graph.floors.Count <= node.y)
            {
                graph.floors.Add(new List<MapNode>());
            }

            graph.floors[node.y].Add(node);
        }

        foreach (MapNodeData nodeData in data.nodes)
        {
            MapNode node = nodeDictionary[nodeData.id];

            foreach (int nextNodeId in nodeData.nextNodeIds)
            {
                if (nodeDictionary.TryGetValue(nextNodeId, out MapNode nextNode))
                {
                    node.nextNodes.Add(nextNode);
                }
            }
        }

        return graph;
    }

    /// <summary>
    /// 노드 id를 추출해 반환
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static int ToNodeId(MapNode node)
    {
        return node != null ? node.id : -1;
    }

    /// <summary>
    /// 저장된 Node ID를 MapNode로 변환
    /// </summary>
    public static MapNode ToMapNode(MapGraph graph, int nodeId)
    {
        if (nodeId < 0)
            return null;

        foreach (var floor in graph.floors)
        {
            foreach (var node in floor)
            {
                if (node.id == nodeId)
                    return node;
            }
        }

        return null;
    }
}
