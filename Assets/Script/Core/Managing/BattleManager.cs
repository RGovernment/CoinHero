using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Constants;
using static Enums;

using SF = UnityEngine.SerializeField;

public class BattleManager : MonoBehaviour
{
    [Serializable]
    public struct PlayerData
    {
        public PlayerClassType type;
        public PlayerCombat combat;
    }
    [Serializable]
    public struct EnemyData 
    {
        public EnemyClassType type;
        public EnemyCombat combat;
    }


    public static BattleManager Instance { get; set; }

    [Header("패널 관련")]
    [SF] private RewardManager rewardManager;
    [SF] private CanvasGroup StartPanel;
    [SF] private CanvasGroup EndPanel;
    [SF] private TextMeshProUGUI endPanelText;

    [Header("덱/패 관련")]
    [SF] private HandManager handManager;
    [SF] private EnemyHandManager enemyHandManager;

    [Header("전투 관련")]
    [SF] private PlayerCombat playerCombat;
    [SF] private List<EnemyCombat> enemyCombat;
    [SF] private SelectZone PlayerZone;
    [SF] private SelectZone EnemyZone;

    [Header("스폰 위치")]
    public Transform playerSpawnPoint;
    public Transform playerBattlePoint;
    public Transform[] enemySpawnPoint;
    public Transform enemyBattlePoint;

    [Header("프리팹")]
    [SF] private List<PlayerData> playerPrefabs;
    [SF] private List<EnemyData> enemyPrefabs;
    public StateMachine state;
    public Dictionary<BattleStateType, IState> stateGroup;

    private Queue<Card> nowPlayerCards;
    private Queue<Card> nowEnemyCards;

    public Vector3 enemyBeforePos;
    public int enemyActionOrderCount = 0;
    public bool EnemyDeadTurn;
    public CancellationTokenSource battlePhaseToken;
    public int totalEnemy;
    private Dictionary<EnemyClassType, EnemyData> enemyPrefabDict;

    private static int nextEnemyInstanceId = 50;
    private void Awake()
    {
        enemyPrefabDict = new Dictionary<EnemyClassType, EnemyData>();
        foreach (var p in enemyPrefabs)
        {
            if (!enemyPrefabDict.ContainsKey(p.type))
                enemyPrefabDict.Add(p.type, p);
        }

        Instance = this;
        nowPlayerCards = new();
        nowEnemyCards = new();
        enemyCombat = new List<EnemyCombat>();
        
        SpawnCharacter();
        GameManager.Instance.state.IsBattle = true;
    }

    private void Start()
    {
        StartPanel.alpha = ONE;
        StartPanel.gameObject.SetActive(true);
        state = new();

        stateGroup = new Dictionary<BattleStateType, IState>()
        {
            [BattleStateType.RoundStart] = new RoundStartState(Instance),
            [BattleStateType.TurnStart]= new TurnStartState(Instance),
            [BattleStateType.DrawPhase] = new DrawPhaseState(Instance),
            [BattleStateType.PlayerChoosePhase] = new PlayerChoosePhaseState(Instance),
            [BattleStateType.BattleStart] = new BattleStartState(Instance),
            [BattleStateType.BattlePhase] = new BattlePhaseState(Instance),
            [BattleStateType.BattleEnd] = new BattleEndState(Instance),
            [BattleStateType.TurnEnd] = new TurnEndState(Instance),
            [BattleStateType.RoundEnd] = new RoundEndState(Instance),
            [BattleStateType.DeadDelay] = new DeadDelayState(Instance)
        };

        state.ChangeState(stateGroup[BattleStateType.RoundStart]);
        TurnStart().Forget();
    }

    private void SpawnCharacter()
    {
        // 1. 플레이어 스폰
        SpawnPlayer();

        // 2. 적 스폰
        SpawnEnemies();
    }

    /// <summary>
    /// 플레이어 스폰 함수
    /// </summary>
    private void SpawnPlayer()
    {
        if (GameManager.Instance == null || 
            GameManager.Instance.state.playerData == null) return;
        Player saveData =  GameManager.Instance.state.playerData;

        PlayerData targetPrefab = playerPrefabs.Find(x => x.type == saveData.ClassType);
        if (targetPrefab.combat == null)
        {
            Debug.LogError($"플레이어 탐색 안됨: {saveData.Name}");
            return;
        }

        List<Card> card = new();

        if (saveData.CardList.Count <= 0)
            card = InitCharacterCards(saveData);
        else
            card = saveData.CardList;

        Player playerData =
            new(
                saveData.Id,
                saveData.Name,
                saveData.MaxHP,
                saveData.HP,
                saveData.ClassType,
                card
                );

        // 생성 및 데이터 주입
        PlayerCombat combat = Instantiate(targetPrefab.combat, Vector3.zero, Quaternion.identity);
        combat.Init(playerData);

        playerCombat = combat;
        playerCombat.gameObject.SetActive(true);
        
    }

    /// <summary>
    /// 적 스폰 함수
    /// </summary>
    private void SpawnEnemies()
    {
        List<Enemy> nextEnemies = GameManager.Instance.state.nextRoundEnemies;
        if (nextEnemies == null || nextEnemies.Count == 0) return;

        for (int i = 0; i < nextEnemies.Count; i++)
        {
            var enemyData = nextEnemies[i];

            if (enemyPrefabDict.TryGetValue(enemyData.ClassType, out var prefabData))
            {
                EnemyCombat combat = Instantiate(prefabData.combat, Vector3.zero, Quaternion.identity);
                
                nextEnemyInstanceId++;
                // 적 카드 세팅
                Enemy enemySet = new (
                    nextEnemyInstanceId,
                    enemyData.Name,
                    enemyData.MaxHP,
                    enemyData.RoundValue,
                    enemyData.Type,
                    enemyData.ClassType,
                    InitCharacterCards(enemyData)
                    );

                combat.Init(enemySet);

                combat.gameObject.SetActive(true);
                enemyCombat.Add(combat);
            }
            else
                Debug.LogWarning($"타입에 해당하는 적 프리팹 없음: {enemyData.ClassType}");
            
        }
    }

    /// <summary>
    /// 캐릭터 초기 생성 시 카드 초기화 함수
    /// </summary>
    /// <param name="character"></param>
    private List<Card> InitCharacterCards(Character character)
    {
        List<Card> cd = new();
        foreach (var cardId in character.StartCardList)
        {
            cd.Add(ResourceManager.Instance.GetCardData(cardId));
        }

        return cd;
    }

    private void Update()
    {
        state.Stay();
    }

    public async UniTaskVoid DrawPhaseDelay()
    {
        await UniTask.DelayFrame(ONE);

        state.ChangeState(stateGroup[BattleStateType.DrawPhase]);
    }

    private void OnEnable()
    {
        PlayerZone.OnSelectCard += handManager.UpdateHandPos;
        PlayerZone.OnCancelCard += handManager.HandActive;
        PlayerZone.OnCancelCard += handManager.UpdateHandPos;
        PlayerZone.OnSelectCardComplete += BattleStatusSet;
    }
    

    private void OnDisable()
    {
        PlayerZone.OnSelectCard -= handManager.UpdateHandPos;
        PlayerZone.OnCancelCard -= handManager.HandActive;
        PlayerZone.OnCancelCard -= handManager.UpdateHandPos;
        PlayerZone.OnSelectCardComplete -= BattleStatusSet;
    }

    public void SelectCard(CardData data)
    {
        PlayerZone.SetCardToZone(data, playerCombat.Character).Forget();
    }

    public void BattleStatusSet()
    {
        BatttleZoneMove().Forget();
    }

    public async UniTask BatttleZoneMove()
    {
        Sequence seq = DOTween.Sequence();
        PlayerCombat player = playerCombat;
        EnemyCombat enemy = enemyCombat[GetEnemyCombatOrderCount()];
        enemyBeforePos = enemy.transform.position;
        player.AnimatorManager.OnMove();
        enemy.AnimatorManager.OnMove();
        // 이동 연출
        await
            seq
            .Join(
                player.transform.DOMove(
                    playerBattlePoint.position, MOVE_TIMER)
            )
            .Join(
                enemy.transform.DOMove(
                    enemyBattlePoint.position, MOVE_TIMER)
            ).ToUniTask();
        

        player.AnimatorManager.OnIdle();
        enemy.AnimatorManager.OnIdle();

        state.ChangeState(stateGroup[BattleStateType.BattleStart]);
    }

    public void PlayerDead(Character chara)
    {
        playerCombat.AnimatorManager.OnDead().Forget();
        playerCombat.Character.OnDead -= PlayerDead;
        state.ChangeState(stateGroup[BattleStateType.RoundEnd]);
    }

    public HandManager GetHandManager()
    {
        return handManager;
    }

    public EnemyHandManager GetEnemyHandManager()
    {
        return enemyHandManager;
    }

    public List<EnemyCombat> GetEnemyCombat()
    {
        return enemyCombat;
    }

    public int GetEnemyCombatOrderCount()
    {
        if (enemyCombat.Count == 0)
        {
            Debug.LogError("GetEnemyCombatOrderCount() 호출 시점에 살아있는 적이 없습니다.");
            return -1;
        }
        return enemyActionOrderCount % enemyCombat.Count;
    }

    public PlayerCombat GetPlayerCombat()
    {
        return playerCombat;
    }

    public SelectZone GetPlayerZone()
    {
        return PlayerZone;
    }

    public SelectZone GetEnemyZone()
    {
        return EnemyZone;
    }           

    public void SetNowPlayerCards(Queue<Card> cards)
    {
        nowPlayerCards = cards;
    }

    public void SetNowEnemyCards(Queue<Card> cards)
    {
        nowEnemyCards = cards;
    }

    public Queue<Card> GetNowPlayerCards()
    {
        return nowPlayerCards;
    }

    public Queue<Card> GetNowEnemyCards()
    {
        return nowEnemyCards;
    }

    public async UniTask TurnStart()
    {
        await StartPanel.DOFade(ZERO, DEFAULT_FADE_TIME);
        StartPanel.gameObject.SetActive(false);
        state.ChangeState(stateGroup[BattleStateType.TurnStart]);
    }

    public async UniTask RoundEnd(bool isWin)
    {
        if (isWin) RonudNext();
        else
        {
            EndPanel.alpha = ZERO;
            EndPanel.gameObject.SetActive(true);
            await EndPanel.DOFade(ONE, DEFAULT_FADE_TIME);
        }
    }

    /// <summary>
    /// 라운드 종료시 플레이어가 생존했으면 호출
    /// </summary>
    public void RonudNext()
    {
        rewardManager.RewardSetting(totalEnemy);
    }
}
