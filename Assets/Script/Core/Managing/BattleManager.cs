using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
using SF = UnityEngine.SerializeField;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; set; }

    [Header("덱/패 관련")]
    [SF] private HandManager handManager;
    [SF] private EnemyHandManager enemyHandManager;

    [Header("전투 관련")]
    [SF] private PlayerCombat playerCombat;
    [SF] private List<EnemyCombat> enemyCombat;
    [SF] private SelectZone PlayerZone;
    [SF] private SelectZone EnemyZone;

    [Header("프리팹")]
    [SF] private BattleCoinUI coinUI;
    private StateMachine state;
    private Dictionary<BattleStateType, IState> stateGroup;

    private Queue<Card> nowPlayerCards;
    private Queue<Card> nowEnemyCards;

    private void Awake()
    {
        Instance = this;
        nowPlayerCards = new();
        nowEnemyCards = new();
    }

    private async UniTaskVoid Start()
    {
        enemyCombat = new List<EnemyCombat>();
        state = new();

        stateGroup = new Dictionary<BattleStateType, IState>()
        {
            [BattleStateType.RoundStart] = new RoundStartState(this),
            [BattleStateType.TurnStart]= new TurnStartState(this),
            [BattleStateType.DrawPhase] = new DrawPhaseState(this),
            [BattleStateType.PlayerChoosePhase] = new PlayerChoosePhaseState(this),
            [BattleStateType.PlayerComplete] = new PlayerCompleteState(this),
            [BattleStateType.EnemyChoosePhase]= new EnemyChoosePhaseState(this),
            [BattleStateType.BattlePhase] = new BattlePhaseState(this),
            [BattleStateType.BattleEnd] = new BattleEndState(this),
            [BattleStateType.TurnEnd] = new TurnEndState(this),
            [BattleStateType.RoundEnd] = new RoundEndState(this)
        };
        
        state.ChangeState(stateGroup[BattleStateType.RoundStart]);
        
        //테스트용
        await UniTask.WaitUntil(() => playerCombat != null, 
            cancellationToken: this.GetCancellationTokenOnDestroy());
        PlayerZone.CardZoneOpen();
        EnemyZone.CardZoneOpen();
        DrawTest(playerCombat.Player);

    }

    private void Update()
    {
        state.Stay();
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

    public void DrawTest(Character player)
    {
        handManager.CreateAllCard(player.CardList, player);
        handManager.ShuffleAndSettingHand();
        handManager.HandActive();
        handManager.UpdateHandPos();
        EnemyCardOpen();
    }

    public void EnemyCardOpen()
    {
        List<Card> dummy = enemyHandManager.CardSelect(enemyCombat[0].Enemy.CardList);
        foreach (var item in dummy)
        {
            nowEnemyCards.Enqueue(item);
            BehindCardData bd = enemyHandManager.CardCreate(item);
            bd.SetTypeIcon();
            EnemyZone.SetCardToEnemyZone(bd);
        }

        
    }

    public void SelectCard(CardData data)
    {
        PlayerZone.SetCardToZone(data).Forget();
    }

    public void BattleStatusSet()
    {
        nowPlayerCards = PlayerZone.GetCardList();
        nowEnemyCards = EnemyZone.GetCardList();
    }

    public HandManager GetHandManager()
    {
        return handManager;
    }

    public void RegisterPlayer(PlayerCombat player)
    {
        playerCombat = player;
    }

    public void RegisterEnemy(EnemyCombat enemy)
    {
        enemyCombat.Add(enemy);
    }
}
