using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
using SF = UnityEngine.SerializeField;
using static Constants;

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

    [Header("사운드")]
    [SF] private AudioSource battleUISound;
    public AudioClip atkSound;

    [Header("프리팹")]
    public StateMachine state;
    public Dictionary<BattleStateType, IState> stateGroup;

    private Queue<Card> nowPlayerCards;
    private Queue<Card> nowEnemyCards;

    public int enemyActionOrderCount = 0;

    private void Awake()
    {
        Instance = this;
        nowPlayerCards = new();
        nowEnemyCards = new();
        enemyCombat = new List<EnemyCombat>();
    }

    private void Start()
    {
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
            [BattleStateType.RoundEnd] = new RoundEndState(Instance)
        };

        state.ChangeState(stateGroup[BattleStateType.RoundStart]);
        state.ChangeState(stateGroup[BattleStateType.TurnStart]);
    }

    private void Update()
    {
        state.Stay();
    }

    public async UniTaskVoid DrawPhaseDelay()
    {
        // 드로우 페이즈 애니메이션 도입 전까지 임시 딜레이
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
        state.ChangeState(stateGroup[BattleStateType.BattleStart]);
    }
    public void EnemyRemove(Character chara)
    {
        //enemyCombat.Remove(combat. combat);
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

    public int GetEnemyCombatCount()
    {
        return enemyCombat.Count;
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
}
