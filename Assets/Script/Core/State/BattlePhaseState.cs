using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static Constants;
using static Enums;

public class BattlePhaseState : IState
{

    private BattleManager manager;

    public BattlePhaseState(BattleManager manager)
    {
        this.manager = manager;
    }

    public void OnEnd()
    {
        manager.battlePhaseToken.Dispose();
    }

    public void OnStart()
    {
        manager.battlePhaseToken = new CancellationTokenSource();
        CancellationToken token = manager.battlePhaseToken.Token;
        Debug.Log("BattlePhaseState Start");
        ExecuteBattleLoopAsync(token).Forget();
    }

    public void OnStay()
    {
    }

    private async UniTask ExecuteBattleLoopAsync(CancellationToken cts)
    {
        // 적이 사망 한 경우 카드 풀을 제거함
        if (manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()].Character.IsDead &&
            manager.GetNowEnemyCards().Count > 0)
            manager.GetNowEnemyCards().Clear();

        // 공격 가능 상태 판별
        bool playerAble = manager.GetNowPlayerCards().TryDequeue(out Card playerCard);
        bool enemyAble = manager.GetNowEnemyCards().TryDequeue(out Card enemyCard);
        
        // 적이 사망하여 일방공격 상태일 경우, 아이템을 제외한 모든 카드 무효
        if (playerAble && manager.EnemyDeadTurn && playerCard.Type == CardType.Item)
        {
            await OneWayAction(manager.GetPlayerCombat(), playerCard, null, cts);

            manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleEnd]);
            return;
        }
        else if(manager.EnemyDeadTurn)
        {
            manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleEnd]);
            return;
        }

        EnemyCombat selectEnemy = manager.GetEnemyCombat()[manager.GetEnemyCombatOrderCount()];

        int ItemCounterId = ResourceManager.Instance
            .EffectDataByType[EffectType.ItemCounter].Id;

        bool playerHasCounter = enemyCard?.Type == CardType.Item &&
                       (playerCard?.Effect?.Any(x => x.EffectId == ItemCounterId) ?? false);

        bool enemyHasCounter = playerCard?.Type == CardType.Item &&
                               (enemyCard?.Effect?.Any(x => x.EffectId == ItemCounterId) ?? false);

                // 적이나 아군이 아이템 무효화 효과를 쓸 경우
        if (playerHasCounter)
        {
            await OneWayAction(manager.GetPlayerCombat(), playerCard, selectEnemy, cts);
        }
        // 적이나 아군이 아이템 무효화 효과를 쓸 경우
        else if (enemyHasCounter)
        {
            await OneWayAction(selectEnemy, enemyCard, manager.GetPlayerCombat(), cts);
        }
        // 한쪽이 카드를 사용하지 않았을 경우 / 일방 행동
        else if (!playerAble || !enemyAble)
        {
            ICombat attacker = playerAble ? manager.GetPlayerCombat() : selectEnemy;
            ICombat defender = playerAble ? selectEnemy : manager.GetPlayerCombat();
            var card = playerAble ? playerCard : enemyCard;

            await OneWayAction(attacker, card, defender, cts);
        }
        // 합
        else
            await ClashAction(manager.GetPlayerCombat(), playerCard, selectEnemy, enemyCard, cts);
        
        // 행동 종료 후 1회 전투 종료
        manager.state.ChangeState(manager.stateGroup[BattleStateType.BattleEnd]);
    }

    /// <summary>
    /// 일방 공격의 경우 사용됨 
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="attackerCard"></param>
    /// <param name="defender"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async UniTask OneWayAction(ICombat attacker, Card attackerCard, ICombat defender, CancellationToken cts)
    {
        if(!attacker.Character.IsDead)
            await BattleActionLogic(attacker, defender, attackerCard, null, CrashType.OneWay, cts);

        attacker.CoinUI.Release();
    }

    /// <summary>
    /// 합 할 경우 사용됨
    /// </summary>
    /// <param name="player"></param>
    /// <param name="playerCard"></param>
    /// <param name="enemy"></param>
    /// <param name="enemyCard"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async UniTask ClashAction(PlayerCombat player, Card playerCard, EnemyCombat enemy, Card enemyCard, CancellationToken cts)
    {
        // 해당 카드가 합이 가능한 카드일 경우
        if ((playerCard.Type == CardType.Weapon || playerCard.Type == CardType.Armor) &&
            (enemyCard.Type == CardType.Weapon || enemyCard.Type == CardType.Armor))
        {
            Card winCard = null, loseCard = null;
            ICombat winCombat = null, loseCombat = null;

            player.CoinUI.CoinSet(playerCard);
            enemy.CoinUI.CoinSet(enemyCard);

            // 코인 토스 후 승리 결정
            while (true)
            {
                bool[] playerCoins = player.CoinToss(playerCard, player.Character.Sanity);
                bool[] enemyCoins = enemy.CoinToss(enemyCard, enemy.Character.Sanity);

                int coinCount = Mathf.Max(playerCoins.Length, enemyCoins.Length);

                player.CoinUI.CoinFlip();
                enemy.CoinUI.CoinFlip();

                await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);
                // 코인 표시
                for (int i = 0; i < coinCount; i++)
                {
                    if (i < playerCoins.Length) player.CoinUI.CoinStop(playerCoins[i]);
                    if (i < enemyCoins.Length) enemy.CoinUI.CoinStop(enemyCoins[i]);

                    await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                }

                int playerResult = playerCard.CalcCoinValue(playerCoins);
                int enemyResult = enemyCard.CalcCoinValue(enemyCoins);

                manager.GetBattleUISound().clip = manager.atkSound;
                manager.GetBattleUISound().Play();

                // 위력이 동등했을 경우 재굴림
                if (playerResult == enemyResult)
                {
                    player.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                    enemy.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                    player.AnimatorManager.OnAttack();
                    enemy.AnimatorManager.OnAttack();

                    await UniTask.WhenAll(
                        player.AnimatorManager.animationTriggerAttacker.Task.AttachExternalCancellation(cts),
                        enemy.AnimatorManager.animationTriggerAttacker.Task.AttachExternalCancellation(cts)
                    );

                    continue;
                }
                // 플레이어가 우세했을 경우 적 코인 파괴
                else if (playerResult > enemyResult)
                {
                    player.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                    enemy.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                    enemyCard.Coin--;
                    player.AnimatorManager.OnAttack();
                    
                    await player.AnimatorManager.animationTriggerAttacker.Task.AttachExternalCancellation(cts);

                    enemy.AnimatorManager.OnOther();

                    await UniTask.WhenAll(
                        enemy.AnimatorManager.animationTriggerDefender.Task.AttachExternalCancellation(cts),
                        enemy.CoinUI.CoinBroken(cts)
                    );

                    if (enemyCard.Coin <= 0)
                    {
                        winCard = playerCard; loseCard = enemyCard;
                        winCombat = player; loseCombat = enemy;
                        break;
                    }
                }
                // 적이 우세했을 경우 플레이어 코인 파괴
                else
                {
                    player.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                    enemy.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();

                    playerCard.Coin--;

                    enemy.AnimatorManager.OnAttack();
                    await enemy.AnimatorManager.animationTriggerAttacker.Task
                        .AttachExternalCancellation(cts);
                    player.AnimatorManager.OnOther();
                    await UniTask.WhenAll(
                        player.AnimatorManager.animationTriggerDefender.Task
                        .AttachExternalCancellation(cts),
                        player.CoinUI.CoinBroken(cts)
                    );

                    if (playerCard.Coin <= 0)
                    {
                        winCard = enemyCard; loseCard = playerCard;
                        winCombat = enemy; loseCombat = player;
                        break;
                    }
                }
            }

            await BattleActionLogic(winCombat, loseCombat, winCard, loseCard, CrashType.Crash, cts);
        }
        // 플레이어와 적이 모두 아이템을 사용했을 경우
        if (playerCard.Type == CardType.Item && enemyCard.Type == CardType.Item)
        {
            // 플레이어 우선 사용
            await BattleActionLogic
                (player, enemy, playerCard, enemyCard, CrashType.OneWay, cts);
            cts.ThrowIfCancellationRequested();
            if (!enemy.Character.IsDead)
                await BattleActionLogic
                (enemy, player, enemyCard, playerCard, CrashType.OneWay, cts);
            else
                manager.EnemyDeadTurn = true;
        }
        // 한쪽만 아이템을 사용했을 경우
        else if (playerCard.Type == CardType.Item || enemyCard.Type == CardType.Item)
        {
            // 아이템을 사용한 쪽 우선 행동
            if (playerCard.Type == CardType.Item)
            {
                await BattleActionLogic
                    (player, enemy, playerCard, enemyCard, CrashType.OneWay, cts);
                cts.ThrowIfCancellationRequested();
                if (!enemy.Character.IsDead)
                    await BattleActionLogic
                        (enemy, player, enemyCard, playerCard, CrashType.OneWay, cts);
                else
                    manager.EnemyDeadTurn = true;
            }
            else
            {
                await BattleActionLogic
                    (enemy, player, enemyCard, playerCard, CrashType.OneWay, cts);
                cts.ThrowIfCancellationRequested();
                if (!player.Character.IsDead)
                    await BattleActionLogic
                        (player, enemy, playerCard, enemyCard, CrashType.OneWay, cts);
            }
        }
    }

    /// <summary>
    /// 공격 및 아이템 사용 처리
    /// </summary>
    /// <param name="user"></param>
    /// <param name="target"></param>
    /// <param name="card"></param>
    /// <param name="targetCard"></param>
    /// <param name="flag"></param>
    /// <param name="cts"></param>
    /// <returns></returns>
    public async UniTask BattleActionLogic(ICombat user, ICombat target, Card card, Card targetCard, CrashType flag,
        CancellationToken cts)
    {
        /// 일방 공격은 여기서 UI 생성
        if (flag == CrashType.OneWay)
            user.CoinUI.CoinSet(card);
        

        switch (card.Type)
        {
            // 공격자 카드 타입이 무기일 때
            case CardType.Weapon:
                user.AnimatorManager.animationTriggerAttacker = new UniTaskCompletionSource();
                target.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                int result = 0;
                int APDiscount = 0;
                bool isAPOn = false;
                if (flag == CrashType.Crash && targetCard != null && targetCard.Type == CardType.Armor)
                {
                    APDiscount = target.APDiscountByLose(targetCard);
                    isAPOn = true;
                }

                // 일방 공격일 경우
                if (flag == CrashType.OneWay)
                {
                    bool[] userCoins = target.CoinToss(card, user.Character.Sanity);

                    int coinCount = userCoins.Length;

                    user.CoinUI.CoinFlip();

                    await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);

                    for (int i = 0; i < coinCount; i++)
                    {
                        user.CoinUI.CoinStop(userCoins[i]);
                        await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                    }

                    result = card.CalcCoinValue(userCoins);
                }
                else
                    result = user.TotalValueByWin(card, APDiscount);

                user.AnimatorManager.OnAttack();

                await user.AnimatorManager.animationTriggerAttacker.Task.AttachExternalCancellation(cts);
                target.AnimatorManager.OnDamage();
                target.Character.TakeDamage(result, user.Character);

                if (isAPOn) DamageSkinSpawner.Instance.TextSpawn(
                    target.BaseCharaObj.transform.position,
                    $"<color=#{SHIELD_COLOR}>피해 {APDiscount} 경감됨!</color>");

                await target.AnimatorManager.animationTriggerDefender.Task.AttachExternalCancellation(cts);
                if(target.Character.IsDead)
                    manager.EnemyDeadTurn = true;
                break;
                // 방어일 경우
            case CardType.Armor:
                user.AnimatorManager.animationTriggerDefender = new UniTaskCompletionSource();
                int SP = 0;
                int armorResult = 0;

                // 일방 공격일 경우
                if (flag == CrashType.OneWay)
                {
                    bool[] userCoins = target.CoinToss(card, user.Character.Sanity);

                    int coinCount = userCoins.Length;

                    user.CoinUI.CoinFlip();

                    await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);

                    for (int i = 0; i < coinCount; i++)
                    {
                        user.CoinUI.CoinStop(userCoins[i]);
                        await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                    }

                    armorResult = card.CalcCoinValue(userCoins);
                }
                else
                    armorResult = user.TotalValueByWin(card, 0);

                SP = flag == CrashType.Crash ? armorResult : armorResult / 2;
                user.Character.TakeShieldPoint(SP);

                if(flag == CrashType.Crash)
                {
                    int reboundDamage = target.Character.TakeRebound(SP);
                    DamageSkinSpawner.Instance.TextSpawn(
                    target.BaseCharaObj.transform.position,
                    $"<color=#{ATTACK_COLOR}>정신력 {reboundDamage}감소!</color>");
                }
                    

                user.AnimatorManager.OnCustom();

                if (SP > 0) DamageSkinSpawner.Instance.TextSpawn(
                    user.BaseCharaObj.transform.position,
                    $"<color=#{SHIELD_COLOR}>실드 증가 {SP}</color>");

                await user.AnimatorManager.animationTriggerDefender.Task.AttachExternalCancellation(cts);
                
                break;
                // 아이템을 사용했을 경우
            case CardType.Item:
                // 일방 사용일 경우
                if(targetCard == null)
                {
                    foreach (var item in card.Effect)
                    {
                        StatusEffectData data = ResourceManager.Instance.EffectData[item.EffectId];

                        await InstantEffectCk(data.Type, user, target, card,cts);
                    }
                }
                else
                {   //적 카드 상태 검사 하는 로직 추가해야함 
                    foreach (var item in card.Effect)
                    {
                        StatusEffectData data = ResourceManager.Instance.EffectData[item.EffectId];

                        await InstantEffectCk(data.Type, user, target, card, targetCard, cts);
                    }
                }
                break;
        }
        if (flag == CrashType.OneWay)
        {
            Debug.Log("초기화됨");
            user.CoinUI.Release();
        }
        // 자연스러운 전환을 위한 고정값 딜레이
        await UniTask.Delay(BATTLE_END_DELAY);
    }

    public async  UniTask InstantEffectCk(EffectType type, ICombat user, ICombat target,
         Card card, CancellationToken cts)
    {
        user.AnimatorManager.animationTriggerItem = new UniTaskCompletionSource();
        // 타겟이 필요한 행동은 타겟이 null일 경우에 대한 조건 체크 넣을 것
        switch (type)
        {
            case EffectType.InstantDamage :
                if (target == null) break;

                bool[] userCoins = target.CoinToss(card, user.Character.Sanity);

                int coinCount = userCoins.Length;

                user.CoinUI.CoinFlip();

                await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);

                for (int i = 0; i < coinCount; i++)
                {
                    user.CoinUI.CoinStop(userCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                }
                user.AnimatorManager.OnCustom();

                int result = card.CalcCoinValue(userCoins);
                await user.AnimatorManager.animationTriggerItem.Task.AttachExternalCancellation(cts);
                target.Character.TakeDamage(result, user.Character);
                break;
            case EffectType.InstantHeal :
                bool[] healCoins = user.CoinToss(card, user.Character.Sanity);
                int healCoinCount = healCoins.Length;
                
                user.CoinUI.CoinFlip();
                await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);
                
                for (int i = 0; i < healCoinCount; i++)
                {
                    Debug.Log("회복 실행됨");
                    user.CoinUI.CoinStop(healCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                }
                user.AnimatorManager.OnCustom();

                int healResult = card.CalcCoinValue(healCoins);

                await user.AnimatorManager.animationTriggerItem.Task.AttachExternalCancellation(cts);
                
                user.Character.TakeHeal(healResult, user.Character);
                break;
        }
        
    }

    public async UniTask InstantEffectCk(EffectType type, ICombat user, ICombat target,
     Card card, Card targetCard, CancellationToken cts)
    {
        user.AnimatorManager.animationTriggerItem = new UniTaskCompletionSource();

        switch (type)
        {
            case EffectType.InstantDamage:
                if (target == null) break;


                bool[] userCoins = target.CoinToss(card, user.Character.Sanity);

                int coinCount = userCoins.Length;

                user.CoinUI.CoinFlip();

                await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);

                for (int i = 0; i < coinCount; i++)
                {
                    user.CoinUI.CoinStop(userCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                }
                user.AnimatorManager.OnCustom();

                int result = card.CalcCoinValue(userCoins);
                await user.AnimatorManager.animationTriggerItem.Task.AttachExternalCancellation(cts);
                target.Character.TakeDamage(result, user.Character);
                break;
            case EffectType.InstantHeal:
                bool[] healCoins = user.CoinToss(card, user.Character.Sanity);
                int healCoinCount = healCoins.Length;
                Debug.Log(healCoinCount);

                user.CoinUI.CoinFlip();
                await UniTask.Delay(COIN_FLIP_TIMER, cancellationToken: cts);

                for (int i = 0; i < healCoinCount; i++)
                {
                    Debug.Log("회복 실행됨");
                    user.CoinUI.CoinStop(healCoins[i]);
                    await UniTask.Delay(COIN_NEXT_TIMER, cancellationToken: cts);
                }
                user.AnimatorManager.OnCustom();

                int healResult = card.CalcCoinValue(healCoins);

                await user.AnimatorManager.animationTriggerItem.Task.AttachExternalCancellation(cts);

                user.Character.TakeHeal(healResult, user.Character);
                break;
        }

    }
}
