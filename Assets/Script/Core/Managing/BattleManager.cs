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

        DrawPhaseDelay().Forget();
    }

    private void Update()
    {
        state.Stay();
    }

    private async UniTask DrawPhaseDelay()
    {
        // 드로우 페이즈 애니메이션 도입 전까지 임시 딜레이
        await UniTask.Delay(500);
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

/*    /// <summary>
    /// 아이템이나 일방 공격/방어를 했을 경우의 처리
    /// </summary>
    public async UniTask OneWayAction(Character attacker, Card attackerCard, Character defender)
    {
        await UniTask.Delay(0);
    }

    public async UniTask ClashAction(PlayerCombat player, Card playerCard, EnemyCombat enemy, Card enemyCard)
    {
        if ((
            playerCard.Type == CardType.Weapon ||
            playerCard.Type == CardType.Armor
            ) && (
            enemyCard.Type == CardType.Weapon ||
            enemyCard.Type == CardType.Armor
            ))
        {
            Card winCard = null;
            Card loseCard = null;
            ICombat winCombat = null;
            ICombat loseCombat = null;

            // 합 진행 전 UI 및 코인 세팅
            player.CoinUI.CoinSet(player.transform, playerCard);
            enemy.CoinUI.CoinSet(player.transform, enemyCard);

            // --- 이하 코인 결과에 따른 처리 ---
            while (true)
            {
                bool[] playerCoins = player.CoinToss(playerCard);
                bool[] enemyCoins = enemy.CoinToss(enemyCard);

                int coinCount = Math.Max(playerCoins.Length, enemyCoins.Length);

                player.CoinUI.CoinFlip();
                enemy.CoinUI.CoinFlip();

                // 코인 결과를 보여주기 전 딜레이
                await UniTask.Delay(COIN_NEXT_TIMER);

                // 코인 결과에 따른 UI 처리
                for (int i = 0; i < coinCount; i++)
                {
                    bool playerEnd = i < playerCoins.Length;
                    bool enemyEnd = i < enemyCoins.Length;

                    if (playerEnd)
                    {
                        // 플레이어 코인 스탑
                        player.CoinUI.CoinStop(playerCoins[i]);
                    }

                    if (enemyEnd)
                    {
                        // 적 코인 스탑
                        enemy.CoinUI.CoinStop(enemyCoins[i]);
                    }

                    // 코인을 순차적으로 보여주기 위해 다음 코인까지 딜레이
                    await UniTask.Delay(COIN_NEXT_TIMER);
                }

                // 플레이어 카드 코인 결과 계산
                int playerResult = playerCard.CalcCoinValue(playerCoins);

                // 적 카드 코인 결과 계산
                int enemyResult = enemyCard.CalcCoinValue(enemyCoins);

                // 쌍방 공격일 경우에만 결과 비교, 한쪽이 아이템이면 무시
                if ((
                    playerCard.Type == CardType.Weapon ||
                    playerCard.Type == CardType.Armor)
                    &&
                    (
                    enemyCard.Type == CardType.Weapon ||
                    enemyCard.Type == CardType.Armor
                    ))
                {
                    battleUISound.clip = atkSound;
                    battleUISound.Play();
                    // 무승부 처리
                    if (playerResult == enemyResult)
                    {
                        await UniTask.Delay(COIN_NEXT_TIMER);
                        continue;
                    }
                    else if (playerResult > enemyResult)
                    {
                        enemyCard.Coin--;
                        await enemy.CoinUI.CoinBroken();

                        // 적 코인이 0이 되면 더 이상 진행하지 않고 종료
                        if (enemyCard.Coin <= 0)
                        {
                            winCard = playerCard;
                            loseCard = enemyCard;
                            winCombat = player;
                            loseCombat = enemy;
                            break;
                        }

                    }
                    else if (playerResult < enemyResult)
                    {
                        playerCard.Coin--;
                        await player.CoinUI.CoinBroken();

                        // 플레이어 코인이 0이 되면 더 이상 진행하지 않고 종료
                        if (playerCard.Coin <= 0)
                        {
                            winCard = enemyCard;
                            winCombat = enemy;
                            loseCard = playerCard;
                            loseCombat = player;
                            break;
                        }
                    }
                }
            }

            BattleActionLogic(winCombat, loseCombat, winCard, loseCard, CrashType.Crash);
        }

        // 스페셜 카드는 합을 진행하지 않으므로 여기서 계산 배제
        if (playerCard.Type == CardType.Item && enemyCard.Type == CardType.Item)
        {
            // 둘다 아이템이므로 플레이어 먼저 효과를 처리 하고, 이후 적 효과 처리 로직 작동
            // 조건 처리 후 BattleActionLogic 작동, 항상 플레이어 먼저

            BattleActionLogic(player, enemy, playerCard, enemyCard, CrashType.OneWay);
            BattleActionLogic(enemy, player, enemyCard, playerCard, CrashType.OneWay);
        }

        if (playerCard.Type == CardType.Item || enemyCard.Type == CardType.Item)
        {
            // 한쪽이 아이템이면 아이템 효과를 먼저 처리하고, 이후 상대 일방 공격 진행

            if(playerCard.Type == CardType.Item)
            {
                BattleActionLogic(player, enemy, playerCard, enemyCard, CrashType.OneWay);
                BattleActionLogic(enemy, player, enemyCard, playerCard, CrashType.OneWay);
            }
            else
            {
                BattleActionLogic(enemy, player, enemyCard, playerCard, CrashType.OneWay);
                BattleActionLogic(player, enemy, playerCard, enemyCard, CrashType.OneWay);
            }
        }
    }

    public void BattleActionLogic(ICombat user, ICombat target, Card card, Card targetCard, CrashType flag)
    {
        switch(card.Type)
        {
            case CardType.Weapon:
                
                int APDiscount = 0;

                // 방어에 의한 감쇄가 발생할 경우, 감쇄량 계산
                if (flag == CrashType.Crash && targetCard.Type == CardType.Armor)
                    APDiscount = target.APDiscountByLose(targetCard);
                // 여기서 대미지 스킨 출력 명령 할 것, 대미지(방어구로 인한 경감시 경감됨! 표시 추가)
                int damage = user.TotalValueByWin(card, APDiscount);

                target.Character.TakeDamage(damage, user.Character);
                break;
            case CardType.Armor:

                if(flag == CrashType.Crash && targetCard.Type == CardType.Weapon)
                {
                    int SP = target.TotalValueByWin(targetCard, 0);

                    user.Character.TakeShieldPoint(SP);
                    target.Character.TakeRebound(SP);
                }
                else
                {
                    int SP = target.TotalValueByWin(targetCard, 0) / 2;
                    user.Character.TakeShieldPoint(SP);
                }

                break;
            case CardType.Item:
                
                break;
        }

        BattleTypeCheck().Forget();
    }*/

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
