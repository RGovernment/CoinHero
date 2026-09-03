using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Constants;
using static Enums;

using SF = UnityEngine.SerializeField;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; set; }

    [Header("패널 관련")]
    [SF] private GameObject StartPanel;
    [SF] private GameObject EndPanel;
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

    [Header("사운드")]
    [SF] private AudioSource battleUISound;
    public AudioClip atkSound;

    [Header("프리팹")]
    public StateMachine state;
    public Dictionary<BattleStateType, IState> stateGroup;

    private Queue<Card> nowPlayerCards;
    private Queue<Card> nowEnemyCards;

    public Vector3 enemyBeforePos;
    public int enemyActionOrderCount = 0;
    public bool EnemyDeadTurn;
    public CancellationTokenSource battlePhaseToken;

    private void Awake()
    {
        Instance = this;
        nowPlayerCards = new();
        nowEnemyCards = new();
        enemyCombat = new List<EnemyCombat>();
    }

    private void Start()
    {
        StartPanel.SetActive(true);
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
    }

    private void Update()
    {
        state.Stay();
    }

    public async UniTaskVoid DrawPhaseDelay()
    {
        await UniTask.DelayFrame(1);
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
        PlayerZone.SetCardToZone(data).Forget();
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

    public AudioSource GetBattleUISound()
    {
        return battleUISound;
    }

    public void RegisterPlayer(PlayerCombat player)
    {
        playerCombat = player;
    }

    public void RegisterEnemy(EnemyCombat enemy)
    {
        enemyCombat.Add(enemy);
    }

    public void TurnStart()
    {
        StartPanel.SetActive(false);

        state.ChangeState(stateGroup[BattleStateType.TurnStart]);
    }

    public void RoundEnd(bool isWin)
    {
        EndPanel.SetActive(true);
        endPanelText.text = isWin ? "승리!" : "패배"; 
    }

    /// <summary>
    /// 임시
    /// </summary>
    public void RonudNext()
    {
        // 임시로 타이틀로 돌아감
        SceneManager.LoadScene(0);
    }
}
