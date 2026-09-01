using Cysharp.Threading.Tasks;
using UnityEngine;
using static Enums;
using static Constants;
using System.Threading;
public class BattlePhaseState : IState
{

    private BattleManager manager;
    public BattlePhaseState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
    }

    public void OnStart()
    {
        Debug.Log("BattlePhaseState Start");
        ExecuteBattleLoopAsync().Forget();
    }

    public void OnStay()
    {
    }

    private async UniTask ExecuteBattleLoopAsync()
    {
        bool playerAble = manager.GetNowPlayerCards().TryDequeue(out Card playerCard);
        bool enemyAble = manager.GetNowEnemyCards().TryDequeue(out Card enemyCard);

        EnemyCombat selectEnemy = manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()];

        if (!playerAble || !enemyAble)
        {
            Character attacker = playerAble ? manager.GetPlayerCombat().Character : selectEnemy.Character;
            Character defender = playerAble ? selectEnemy.Character : manager.GetPlayerCombat().Character;
            var card = playerAble ? playerCard : enemyCard;

            await OneWayAction(attacker, card, defender);
        }
        else
        {
            await ClashAction(manager.GetPlayerCombat(), playerCard, selectEnemy, enemyCard);
        }
        

        manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleEnd]);
    }

    public async UniTask OneWayAction(Character attacker, Card attackerCard, Character defender)
    {
        await UniTask.Delay(0);
    }

    public async UniTask ClashAction(PlayerCombat player, Card playerCard, EnemyCombat enemy, Card enemyCard)
    {
        if ((playerCard.Type == CardType.Weapon || playerCard.Type == CardType.Armor) &&
            (enemyCard.Type == CardType.Weapon || enemyCard.Type == CardType.Armor))
        {
            Card winCard = null, loseCard = null;
            ICombat winCombat = null, loseCombat = null;

            player.CoinUI.CoinSet(playerCard);
            enemy.CoinUI.CoinSet(enemyCard);

            while (true)
            {
                bool[] playerCoins = player.CoinToss(playerCard);
                bool[] enemyCoins = enemy.CoinToss(enemyCard);

                int coinCount = Mathf.Max(playerCoins.Length, enemyCoins.Length);

                player.CoinUI.CoinFlip();
                enemy.CoinUI.CoinFlip();

                await UniTask.Delay(COIN_FLIP_TIMER);

                for (int i = 0; i < coinCount; i++)
                {
                    if (i < playerCoins.Length) player.CoinUI.CoinStop(playerCoins[i]);
                    if (i < enemyCoins.Length) enemy.CoinUI.CoinStop(enemyCoins[i]);

                    await UniTask.Delay(COIN_NEXT_TIMER);
                }

                int playerResult = playerCard.CalcCoinValue(playerCoins);
                int enemyResult = enemyCard.CalcCoinValue(enemyCoins);

                manager.GetBattleUISound().clip = manager.atkSound;
                manager.GetBattleUISound().Play();

                if (playerResult == enemyResult)
                {
                    player.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                    enemy.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                    player.AnimatorManager.OnAttack();
                    enemy.AnimatorManager.OnAttack();

                    await UniTask.WhenAll(
                        player.AnimatorManager.animationTriggerAttacker.Task,
                        enemy.AnimatorManager.animationTriggerAttacker.Task
                    );

                    continue;
                }
                else if (playerResult > enemyResult)
                {
                    player.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                    enemy.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                    enemyCard.Coin--;
                    player.AnimatorManager.OnAttack();
                    
                    await player.AnimatorManager.animationTriggerAttacker.Task;

                    enemy.AnimatorManager.OnOther();

                    await UniTask.WhenAll(
                        enemy.AnimatorManager.animationTriggerDefender.Task,
                        enemy.CoinUI.CoinBroken()
                    );

                    if (enemyCard.Coin <= 0)
                    {
                        winCard = playerCard; loseCard = enemyCard;
                        winCombat = player; loseCombat = enemy;
                        break;
                    }
                }
                else
                {
                    player.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                    enemy.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();

                    playerCard.Coin--;

                    enemy.AnimatorManager.OnAttack();
                    await enemy.AnimatorManager.animationTriggerAttacker.Task;
                    player.AnimatorManager.OnOther();
                    await UniTask.WhenAll(
                        player.AnimatorManager.animationTriggerDefender.Task,
                        player.CoinUI.CoinBroken()
                    );

                    if (playerCard.Coin <= 0)
                    {
                        winCard = enemyCard; loseCard = playerCard;
                        winCombat = enemy; loseCombat = player;
                        break;
                    }
                }
            }

            await BattleActionLogic(winCombat, loseCombat, winCard, loseCard, CrashType.Crash);
        }

        if (playerCard.Type == CardType.Item && enemyCard.Type == CardType.Item)
        {
            await BattleActionLogic(player, enemy, playerCard, enemyCard, CrashType.OneWay);
            await BattleActionLogic(enemy, player, enemyCard, playerCard, CrashType.OneWay);
        }
        else if (playerCard.Type == CardType.Item || enemyCard.Type == CardType.Item)
        {
            if (playerCard.Type == CardType.Item)
            {
                await BattleActionLogic(player, enemy, playerCard, enemyCard, CrashType.OneWay);
                await BattleActionLogic(enemy, player, enemyCard, playerCard, CrashType.OneWay);
            }
            else
            {
                await BattleActionLogic(enemy, player, enemyCard, playerCard, CrashType.OneWay);
                await BattleActionLogic(player, enemy, playerCard, enemyCard, CrashType.OneWay);
            }
        }
    }

    public async UniTask BattleActionLogic(ICombat user, ICombat target, Card card, Card targetCard, CrashType flag)
    {
        switch (card.Type)
        {
            case CardType.Weapon:
                int APDiscount = 0;
                if (flag == CrashType.Crash && targetCard.Type == CardType.Armor)
                {
                    APDiscount = target.APDiscountByLose(targetCard);
                    
                }

                int damage = user.TotalValueByWin(card, APDiscount);
                
                user.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                target.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();

                user.AnimatorManager.OnAttack();
                await user.AnimatorManager.animationTriggerAttacker.Task;
                target.Character.TakeDamage(damage, user.Character);
                target.AnimatorManager.OnDamage();
                await target.AnimatorManager.animationTriggerDefender.Task;
                
                break;

            case CardType.Armor:
                if (flag == CrashType.Crash && targetCard.Type == CardType.Weapon)
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

                user.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                user.AnimatorManager.OnCustom();

                await user.AnimatorManager.animationTriggerDefender.Task;
                
                break;

            case CardType.Item:
                break;
        }
        // 자연스러운 전환을 위한 고정값 딜레이
        await UniTask.Delay(BATTLE_END_DELAY);
    }
}
