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
    [SF] private AudioClip atkSound;

    [Header("프리팹")]
    [SF] 
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
        DrawTest(playerCombat.Character);

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
        List<Card> dummy = enemyHandManager.CardSelect(enemyCombat[0].Character.CardList);
        
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
        // 적은 처음에 결정됨

        PlayerZone.gameObject.SetActive(false);
        EnemyZone.gameObject.SetActive(false);
        PlayerZone.BtnClose();
        BattleTypeCheck().Forget();
    }

    public async UniTaskVoid BattleTypeCheck()
    {
        
        bool playerAble = nowPlayerCards.TryDequeue(out Card playerCard);
        bool enemyAble = nowEnemyCards.TryDequeue(out Card enemyCard);

        // 양쪽 다 사용이 불가능하다면 턴 종료
        if (!playerAble && !enemyAble)
        {
            return;
        }

        if (!playerAble || !enemyAble)
        {
            Character attacker = playerAble ? playerCombat.Character : enemyCombat[0].Character;
            Character defender = playerAble ? enemyCombat[0].Character : playerCombat.Character;
            var card = playerAble ? playerCard : enemyCard;

            await OneWayAction(attacker, card, defender);
        }
        else if (playerAble && enemyAble)
        {
            await ClashAction(playerCombat, playerCard, enemyCombat[0], enemyCard);
        }

        // 다음 스킬로 전환 시키기
    }

    /// <summary>
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
                await UniTask.Delay(CoinFlipTimer);

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
                    await UniTask.Delay(CoinNextTimer);
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
                        await UniTask.Delay(CoinNextTimer);
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
        int damage = 0;

        switch(card.Type)
        {
            case CardType.Weapon:
                target.Character.TakeDamage(damage, user.Character);
                break;
            case CardType.Armor:
                target.Character.TakeDamage(damage, user.Character);
                break;
            case CardType.Item:
                
                break;
        }
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
