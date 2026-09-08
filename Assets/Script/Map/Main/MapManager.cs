using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;
using static Enums;
using static Utility;
using SF = UnityEngine.SerializeField;

public class MapManager : MonoBehaviour
{
    [Header("Data")]
    [SF] private MapCreater mapCreater;
    
    [Header("UI")]
    [SF] private ScrollRect mapLayout;
    [SF] private RectTransform mapContent;
    // 맵 좌우 여백
    [SF] private float horizontalPadding = 100f;
    [SF] private CanvasGroup fadeCanvas;
    //
    [SF] private float lineGap = 20f;

    [Header("버튼 프리팹")]
    [SF] private Button bossBtn;
    [SF] private Button shopBtn;
    [SF] private Button restAreaBtn;
    [SF] private Button normalBtn;
    [SF] private Button eliteBtn;
    [Header("Line Prefab")]
    [SF] private Image mapLine;

    [Header("Layout Settings")]
    // 층 간격 (좌우 여백)
    [SF] private float floorSpacing = 220f;
    // 줄기 간격 (상하 여백)
    [SF] private float slotSpacing = 160f;
    [SF] private float positionJitter = 15f;
    [SF] private float lineThickness = 6f;

    // 노드 <-> 실제 UI 오브젝트 매핑 (선 그리기 / 상태 갱신에 필요)
    private readonly Dictionary<MapNode, RectTransform> nodeViews = new();
    private readonly Dictionary<MapNode, Button> nodeButtons = new();

    private MapGraph data;
    private MapNode currentNode; // 플레이어가 현재 위치한 노드

    private void Awake()
    {
        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.DOFade(ZERO, DEFAULT_FADE_TIME)
            .OnComplete(()=> fadeCanvas.gameObject.SetActive(false));
        GenerateMap();
    }

    public void GenerateMap()
    {
        ClearMap();
        
        GameState gameState = GameManager.Instance.state;

        if (gameState.mapStatus == null)
        {
            // 맵 생성
            data = mapCreater.CreateMap();
            //gameState.mapStatus = data;
            gameState.mapStatus = SaveMapSaveData(data);
        }
        // 있을 경우 재사용
        else
        {
            data = LoadMapSaveData(gameState.mapStatus);
            currentNode = ToMapNode(data, GameManager.Instance.state.currentMapNodeId);
        }

        //data = gameState.mapStatus;
        /*if (GameManager.Instance.state.currentMapNode != null)
            currentNode = GameManager.Instance.state.currentMapNode;*/

        // 노드 UI 생성
        foreach (var floor in data.floors)
        {
            foreach (var node in floor)
            {
                CreateNodeView(floor, node);
            }
        }

        // 연결선 UI 생성 (모든 노드 좌표가 계산된 뒤에 그려야 함)
        foreach (var floor in data.floors)
        {
            foreach (var node in floor)
            {
                if (node.nextNodes == null) continue;

                foreach (var nextNode in node.nextNodes)
                {
                    DrawLine(nodeViews[node], nodeViews[nextNode]);
                }
            }
        }

        float contentWidth = (data.floors.Count - 1) * floorSpacing + horizontalPadding * 2f;

        mapContent.sizeDelta = new Vector2(contentWidth, mapContent.sizeDelta.y);

        // 초기 반영
        UpdateInteractable();

        FocusOnCurrentNode();
    }

    private void CreateNodeView(List<MapNode> floor, MapNode node)
    {
        Button prefab = GetPrefabForType(node.mapType);
        Button instance = Instantiate(prefab, mapContent);

        RectTransform rect = instance.GetComponent<RectTransform>();

        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);

        int slot = mapCreater.GetSlotForNode(floor, node);
        float x = node.y * floorSpacing + horizontalPadding;
        float y = (slot - 1) * slotSpacing + Random.Range(-positionJitter, positionJitter);
        rect.anchoredPosition = new Vector2(x, y);

        instance.onClick.AddListener(() => OnNodeClick(node));

        nodeViews[node] = rect;
        nodeButtons[node] = instance;
    }

    private Button GetPrefabForType(MapType type)
    {
        return type switch
        {
            MapType.Boss => bossBtn,
            MapType.Elite => eliteBtn,
            MapType.Shop => shopBtn,
            MapType.RestArea => restAreaBtn,
            _ => normalBtn,
        };
    }

    private void DrawLine(RectTransform from, RectTransform to)
    {
        Image lineInstance = Instantiate(mapLine, mapContent);

        // 선은 전부 버튼보다 아래로 향하도록
        lineInstance.transform.SetAsFirstSibling();

        RectTransform lineRect = lineInstance.rectTransform;
        lineRect.anchorMin = new Vector2(0f, 0.5f);
        lineRect.anchorMax = new Vector2(0f, 0.5f);
        lineRect.pivot = new Vector2(0f, 0.5f);

        Vector2 fromPos = from.anchoredPosition;
        Vector2 toPos = to.anchoredPosition;

        // 방향 벡터 계산
        Vector2 dir = (toPos - fromPos).normalized;

        float fromOffset = from.sizeDelta.x / 2f + lineGap;
        float toOffset = to.sizeDelta.x / 2f + lineGap;

        // 시작점은 기준값보다 계산값만큼 앞에서 지정
        Vector2 startPoint = fromPos + dir * fromOffset;

        // 종료점은 기준값보다 계산값만큼 뒤로 지정
        Vector2 endPoint = toPos - dir * toOffset;

        float distance = Mathf.Max(0f, Vector2.Distance(startPoint, endPoint));
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRect.anchoredPosition = startPoint;
        lineRect.sizeDelta = new Vector2(distance, lineThickness);
        lineRect.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private void OnNodeClick(MapNode node)
    {
        OnNodeClickBase(node).Forget();
    }

    /// <summary>
    /// 현재 노드의 값을 가져온 뒤, 해당 노드로 이동
    /// </summary>
    /// <param name="node"></param>
    private async UniTask OnNodeClickBase(MapNode node)
    {
        fadeCanvas.gameObject.SetActive(true);
        SceneType loadScene = SceneType.None;
        // 클릭 조건 검사
        if (currentNode == null && node.y != 0) return;
        if (currentNode != null && !currentNode.nextNodes.Contains(node)) return;

        currentNode = node;
        // 선택한 노드 저장

        GameManager.Instance.state.currentMapNodeId = ToNodeId(node);

        UpdateInteractable();

        // 항상 이번 적 목록 초기화
        GameManager.Instance.state.nextRoundEnemies.Clear();

        List<Enemy> zombieList = ResourceManager.Instance.EnemyZombieData;
        List<Enemy> skeletonList = ResourceManager.Instance.EnemySkeletonData;
        List<Enemy> eliteZombieList = ResourceManager.Instance.EnemyEliteZombieData;
        List<Enemy> eliteSkeletonList = ResourceManager.Instance.EnemyEliteSkeletonData;

        switch (node.mapType)
        {
            case MapType.Normal:
                // 스폰 로직은 차후 디테일하게 수정할 것
                GameManager.Instance.state.IsBoss = false;

                int countRandom = Random.Range(0, 100);
                // 스폰 명 수 확률 지정
                int enemyCount = 
                    countRandom < ENEMY_THREE_COUNT_PERCENT ? 3 
                    : countRandom < ENEMY_THREE_COUNT_PERCENT + ENEMY_TWO_COUNT_PERCENT ? 2 
                    : 1;


                for (int i = 0; i < enemyCount; i++)
                {
                    int random = Random.Range(0, 100);

                    if(random < SKELETON_SPAWN_WEIGHT)
                    {
                        Enemy enemyData = zombieList[Random.Range(0, zombieList.Count)];

                        GameManager.Instance.state.nextRoundEnemies.Add(enemyData);
                    }
                    else
                    {
                        Enemy enemyData = skeletonList[Random.Range(0, skeletonList.Count)];

                        GameManager.Instance.state.nextRoundEnemies.Add(enemyData);
                    }
                }

                loadScene = SceneType.Battle;
                break;
            case MapType.Elite:

                // 엘리트는 항상 2마리만 스폰
                int enemyeliteCount = MAX_ELITE_SPAWN_COUNT;

                for (int i = 0; i < enemyeliteCount; i++)
                {
                    int random = Random.Range(0, 100);
                    Enemy enemyData;
                    if (random < SKELETON_SPAWN_WEIGHT)
                        enemyData = eliteZombieList[Random.Range(0, eliteZombieList.Count)];
                    
                    else
                        enemyData = eliteSkeletonList[Random.Range(0, eliteSkeletonList.Count)];

                        
                   
                    GameManager.Instance.state.nextRoundEnemies.Add(enemyData);
                }

                loadScene = SceneType.Battle;
                break;
            case MapType.Boss:
                GameManager.Instance.state.IsBoss = true;
                // 보스는 항상 3마리 스폰
                int enemybossCount = MAX_ENEMY_COUNT;
                int nowRonud = GameManager.Instance.state.NowRound;
                for(int i = 0; i < enemybossCount; i++)
                {
                    if (i == ZERO)
                    {
                        Enemy enemyData = 
                            ResourceManager.Instance.EnemyBossData[nowRonud - 1];

                        GameManager.Instance.state.nextRoundEnemies.Add(enemyData);
                    }
                    else
                    {
                        int per = Random.Range(0, 100);

                        Enemy enemyData;

                        if (per < SKELETON_SPAWN_WEIGHT)
                            enemyData = zombieList[Random.Range(0, zombieList.Count)];
                        else
                            enemyData = skeletonList[Random.Range(0, skeletonList.Count)];
                        

                        GameManager.Instance.state.nextRoundEnemies.Add(enemyData);
                    }
                }

                loadScene = SceneType.Battle;
                break;
            case MapType.Shop:
                loadScene = SceneType.Shop;
                break;
            case MapType.RestArea:
                loadScene = SceneType.RestArea;
                break;
        }

        await fadeCanvas.DOFade(ONE, DEFAULT_FADE_TIME);

        SceneManager.LoadScene((int)loadScene);
    }

    /// <summary>
    /// 현재 노드 상태에 따라 노드 버튼의 상호작용 여부 변경
    /// </summary>
    private void UpdateInteractable()
    {
        foreach (var kvp in nodeButtons)
        {
            MapNode node = kvp.Key;
            Button btn = kvp.Value;

            bool interactable =
                (currentNode == null && node.y == 0) ||
                (currentNode != null && currentNode.nextNodes.Contains(node));

            btn.interactable = interactable;
        }
    }

    /// <summary>
    /// 혹시 모를 
    /// </summary>
    private void ClearMap()
    {
        foreach (Transform child in mapContent)
        {
            Destroy(child.gameObject);
        }
        nodeViews.Clear();
        nodeButtons.Clear();
        currentNode = null;
    }

    private void FocusOnCurrentNode()
    {
        if (mapLayout == null) return;

        if (currentNode == null)
        {
            mapLayout.horizontalNormalizedPosition = 0f;
            return;
        }

        float contentWidth = mapContent.sizeDelta.x;
        float viewportWidth = mapLayout.viewport.rect.width;

        if (contentWidth <= viewportWidth) return;

        float targetX = currentNode.y * floorSpacing + horizontalPadding;
        float normalized = (targetX - viewportWidth / 2f) / (contentWidth - viewportWidth);
        mapLayout.horizontalNormalizedPosition = Mathf.Clamp01(normalized);
    }
}