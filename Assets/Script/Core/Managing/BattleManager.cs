using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using static Constants;
using static Enums;
using static Utility;
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

    public CancellationToken battlePhaseToken;

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

    public void EnemyRemove(Character chara)
    {
        BattleStateType stateType = state.GetStateType();
        state.ChangeState(stateGroup[BattleStateType.DeadDelay]);
        RemoveDelay(stateType, chara).Forget();
    }

    public async UniTask RemoveDelay(BattleStateType before, Character chara)
    {
        // 사망 모션 작동 후 처리

        int index = enemyCombat.FindIndex(x => x.Character.Id == chara.Id);

        await enemyCombat[index].AnimatorManager.OnDead();

        EnemyCombat temp = enemyCombat[index];
        temp.Character.OnDead -= EnemyRemove;
        enemyCombat.RemoveAt(index);
        Destroy(temp.gameObject);

        if (before == BattleStateType.TurnStart)
            state.ChangeState(stateGroup[BattleStateType.DrawPhase]);
        
            
        
        if (before == BattleStateType.TurnEnd)
        {
            if (enemyCombat.Count <= 0)
                state.ChangeState(stateGroup[BattleStateType.RoundEnd]);
            else
                state.ChangeState(stateGroup[BattleStateType.TurnEnd]);
            
        }
            
        if (before == BattleStateType.BattlePhase)
        {
            nowEnemyCards.Clear();
            EnemyDeadTurn = true;
            state.ChangeState(stateGroup[BattleStateType.BattleEnd]);
        }
            
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
        state.ChangeState(stateGroup[BattleStateType.TurnStart]);
    }
}
