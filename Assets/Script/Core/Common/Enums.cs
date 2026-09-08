public static class Enums
{
    public enum BattleStateType { 
        RoundStart,
        TurnStart, 
        DrawPhase, 
        PlayerChoosePhase, 
        BattleStart,
        BattlePhase, 
        BattleEnd, 
        TurnEnd , 
        RoundEnd ,
        DeadDelay
    }

    /// <summary>
    /// 카드 타입, 무기, 방어구, 아이템, 특수
    /// 아이템과 특수 타입은 Value + coin의 값이 Effect의 효과에 사용되므로
    /// 두 타입은 반드시 최소 1개 이상의 Effect를 가져야함, 
    /// 단 Effect 자체가 효과를 가진 경우 쓰지 않음
    /// </summary>
    public enum CardType { Weapon, Armor, Item, Special }

    /// <summary>
    /// 발동 시점
    ///<para>OnPlay : 전투 진행 중</para>
    ///<para>OnTurnStart : 전투 시작 시</para>
    ///<para>OnTurnEnd : 전투 종료 시</para>
    ///<para>OnSpecial : Special 슬롯에 장착 즉시 발동</para>
    ///<para>Passive : 사용 후 상시 발동</para>
    /// </summary>
    public enum CardTrigger { OnPlay, OnTurnStart, OnTurnEnd, OnSpecial, Passive }

    /// <summary>
    /// 발동 위치
    /// <para>Slot : 슬롯에서 발동</para>
    /// <para>Special : 특수 카드존에서 발동</para>
    /// </summary>
    public enum CardZone { Slot, Special }

    /// <summary>
    /// <para>Caster : 본인</para>
    /// <para>TargetEnemy : 타겟한 적</para>
    /// <para>AllEnemies : 적 전체</para>
    /// <para>All : 본인 포함 전체</para>
    /// <para>Slot : 슬롯에 영향</para>
    /// </summary>
    public enum TargetType { Caster, TargetEnemy, AllEnemies, All , Slot }

    /// <summary>
    /// normal : 일반 맵
    /// Boss : 보스 맵
    /// Shop : 상점 맵
    /// Rest Area : 휴식 맵
    /// </summary>
    public enum MapType { Normal, Boss, Shop, RestArea }

    /// <summary>
    /// Normal : 일반
    /// Elete : 엘리트
    /// Boss : 보스
    /// </summary>
    public enum EnemyType { Normal, Elete, Boss}
    /// <summary>
    /// 적 세부 타입
    /// Zombie : 적 타입이 좀비임
    /// </summary>
    public enum EnemyClassType { Zombie, ZombieBoss }

    public enum PlayerClassType { Warrior }
    /// <summary>
    /// 스탯 종류 
    /// </summary>
    public enum StatType { Value, CoinPoint, Coin }
    

    /// <summary>
    /// 합 유형
    /// Crash : 합
    /// OneWay : 일방
    /// </summary>
    public enum CrashType { Crash, OneWay }

    /// <summary>
    /// 씬 유형
    /// Title : 메인 타이틀
    /// Loading : 로딩 화면
    /// Battle : 전투 화면
    /// Map : 지도 화면
    /// Shop : 상점 화면
    /// None : 없음
    /// </summary>
    public enum SceneType { Title, Loading, Battle, Map, Shop, None };

    public enum SystemSoundType { Click, Hover, CardClick, CardHover, Inven, Reward , RewardBtn, None }

    public enum BattleSoundType { Crash, Attack, Use, None }

    // Json 파일로의 저장시 참고용
    public enum EffectType { ValueUp, ValueDown, 
        InstantHeal, InstantDamage, 
        ExtraSlot, Burn, ItemCounter,
        AttackUpEffect, AttackDownEffect, Crack, Silence
    }

    public enum SoundMixerType
    {
        Master,
        BGM,
        System,
        Game
    }
}
