using Cysharp.Threading.Tasks;
using NUnit.Framework.Internal;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Constants;
using static Enums;
using static UnityEngine.UIElements.UxmlAttributeDescription;
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

        int ItemCounterId = ResourceManager.Instance
            .EffectDataByType[EffectType.ItemCounter].Id;

        bool playerHasCounter = enemyCard.Type == CardType.Item &&
                       (playerCard.Effect?.Any(x => x.EffectId == ItemCounterId) ?? false);

        bool enemyHasCounter = playerCard.Type == CardType.Item &&
                               (enemyCard.Effect?.Any(x => x.EffectId == ItemCounterId) ?? false);

        if (!playerAble || !enemyAble)
        {
            ICombat attacker = playerAble ? manager.GetPlayerCombat() : selectEnemy;
            ICombat defender = playerAble ? selectEnemy : manager.GetPlayerCombat();
            var card = playerAble ? playerCard : enemyCard;

            await OneWayAction(attacker, card, defender);
        }
        else if (playerHasCounter)
        {
            await OneWayAction(manager.GetPlayerCombat(), playerCard, selectEnemy);
        }
        else if (enemyHasCounter)
        {
            await OneWayAction(selectEnemy, enemyCard, manager.GetPlayerCombat());
        }
        else
        {
            await ClashAction(manager.GetPlayerCombat(), playerCard, selectEnemy, enemyCard);
        }
        

        manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleEnd]);
    }

    public async UniTask OneWayAction(ICombat attacker, Card attackerCard, ICombat defender)
    {
        await BattleActionLogic(attacker, defender, attackerCard, null, CrashType.OneWay);

        attacker.CoinUI.Release();
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
                bool[] playerCoins = player.CoinToss(playerCard, player.Character.Sanity);
                bool[] enemyCoins = enemy.CoinToss(enemyCard, enemy.Character.Sanity);

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
        if (flag == CrashType.OneWay) user.CoinUI.CoinSet(card); 

        switch (card.Type)
        {
            case CardType.Weapon:
                user.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                target.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();

                // 카드가 없는 일방 공격의 경우
                if (targetCard == null)
                {
                    int oneWayDamage = user.TotalValueByWin(card);

                    user.AnimatorManager.OnAttack();
                    await user.AnimatorManager.animationTriggerAttacker.Task;
                    target.Character.TakeDamage(oneWayDamage, user.Character);
                    target.AnimatorManager.OnDamage();
                    await target.AnimatorManager.animationTriggerDefender.Task;

                    break;
                }

                int APDiscount = 0;
                bool isAPOn = false;
                if (flag == CrashType.Crash && targetCard.Type == CardType.Armor)
                {
                    APDiscount = target.APDiscountByLose(targetCard);
                    isAPOn = true;
                }

                int damage = user.TotalValueByWin(card, APDiscount);



                user.AnimatorManager.OnAttack();
                await user.AnimatorManager.animationTriggerAttacker.Task;
                target.Character.TakeDamage(damage, user.Character);
                target.AnimatorManager.OnDamage();
                if (isAPOn) DamageSkinSpawner.Instance.TextSpawn(
                    target.BaseCharaObj.transform.position,
                    $"<color=#{SHIELD_COLOR}>{APDiscount} 방어됨!</color>");
                await target.AnimatorManager.animationTriggerDefender.Task;

                break;

            case CardType.Armor:
                user.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                int SP = 0;
                // 카드 없는 일방 방어의 경우
                if (targetCard == null)
                {
                    SP = target.TotalValueByWin(card, 0) / 2;
                    user.Character.TakeShieldPoint(SP);

                    break;
                }
                else
                {
                    if (flag == CrashType.Crash && targetCard.Type == CardType.Weapon)
                    {
                        SP = target.TotalValueByWin(card, 0);
                        user.Character.TakeShieldPoint(SP);
                        target.Character.TakeRebound(SP);
                    }
                    else
                    {
                        SP = target.TotalValueByWin(card, 0) / 2;
                        user.Character.TakeShieldPoint(SP);
                    }
                }
                
                user.AnimatorManager.OnCustom();
                if (SP > 0) DamageSkinSpawner.Instance.TextSpawn(
                    user.BaseCharaObj.transform.position,
                    $"<color=#{SHIELD_COLOR}>실드 증가 {SP}</color>");

                await user.AnimatorManager.animationTriggerDefender.Task;
                
                break;

            case CardType.Item:
                // 일방 사용일 경우
                if(targetCard == null)
                {
                    foreach (var item in card.Effect)
                    {
                        StatusEffectData data = ResourceManager.Instance.EffectData[item.EffectId];

                        await InstantEffectCk(data.Type, user, target, card);
                    }
                }
                else
                {   //적 카드 상태 검사 하는 로직 추가해야함 
                    foreach (var item in card.Effect)
                    {
                        StatusEffectData data = ResourceManager.Instance.EffectData[item.EffectId];

                        await InstantEffectCk(data.Type, user, target, card, targetCard);
                    }
                }
                break;
        }

        // 자연스러운 전환을 위한 고정값 딜레이
        await UniTask.Delay(BATTLE_END_DELAY);
    }

    public async  UniTask InstantEffectCk(EffectType type, ICombat user, ICombat target,
         Card card)
    {
        user.AnimatorManager.animationTriggerItem = new UniTaskCompletionSource();
        
        switch (type)
        {
            case EffectType.InstantDamage :
                bool[] userCoins = target.CoinToss(card, user.Character.Sanity);

                int coinCount = userCoins.Length;

                user.CoinUI.CoinFlip();

                await UniTask.Delay(COIN_FLIP_TIMER);

                for (int i = 0; i < coinCount; i++)
                {
                    user.CoinUI.CoinStop(userCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER);
                }
                user.AnimatorManager.OnCustom();

                int result = card.CalcCoinValue(userCoins);
                await user.AnimatorManager.animationTriggerItem.Task;
                target.Character.TakeDamage(result, user.Character);
                break;
            case EffectType.InstantHeal :
                bool[] healCoins = target.CoinToss(card, user.Character.Sanity);
                int healCoinCount = healCoins.Length;
                int heal = 0;

                user.CoinUI.CoinFlip();
                await UniTask.Delay(COIN_FLIP_TIMER);

                for (int i = 0; i < healCoinCount; i++)
                {
                    user.CoinUI.CoinStop(healCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER);
                }
                user.AnimatorManager.OnCustom();

                int healResult = card.CalcCoinValue(healCoins);

                await user.AnimatorManager.animationTriggerItem.Task;
                
                user.Character.TakeHeal(heal, user.Character);
                break;
        }
        
    }

    public async UniTask InstantEffectCk(EffectType type, ICombat user, ICombat target,
     Card card, Card targetCard)
    {
        user.AnimatorManager.animationTriggerItem = new UniTaskCompletionSource();

        switch (type)
        {
            case EffectType.InstantDamage:
                bool[] userCoins = target.CoinToss(card, user.Character.Sanity);

                int coinCount = userCoins.Length;

                user.CoinUI.CoinFlip();

                await UniTask.Delay(COIN_FLIP_TIMER);

                for (int i = 0; i < coinCount; i++)
                {
                    user.CoinUI.CoinStop(userCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER);
                }
                user.AnimatorManager.OnCustom();

                int result = card.CalcCoinValue(userCoins);
                await user.AnimatorManager.animationTriggerItem.Task;
                target.Character.TakeDamage(result, user.Character);
                break;
            case EffectType.InstantHeal:
                bool[] healCoins = target.CoinToss(card, user.Character.Sanity);
                int healCoinCount = healCoins.Length;

                user.CoinUI.CoinFlip();
                await UniTask.Delay(COIN_FLIP_TIMER);

                for (int i = 0; i < healCoinCount; i++)
                {
                    user.CoinUI.CoinStop(healCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER);
                }
                user.AnimatorManager.OnCustom();

                int healResult = card.CalcCoinValue(healCoins);

                await user.AnimatorManager.animationTriggerItem.Task;

                user.Character.TakeHeal(healResult, user.Character);
                break;
        }

    }
}
