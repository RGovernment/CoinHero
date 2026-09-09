public static class Constants
{
    public const string ASSET_DATA_PATH = "Data/";
    public const string CARD_IMAGE_PATH = "IgnoreImage/CardImage/";

    public const string STATUS_EFFECT_DATA = "StatusEffectData";
    public const string PLAYER_CARD_DATA = "PlayerCardData";
    public const string ENEMY_CARD_DATA = "EnemyCardData";
    public const string ENEMY_DATA = "EnemyData";
    public const string PLAYER_DATA = "PlayerData";
    public const string BOSS = "Boss";
    public const string ELITE = "Elite";
    public const string ZOMBIE = "Zombie";
    public const string SKELETON = "Skeleton";

    public const string VALUE = "Value";
    public const string DURATION = "Duration";
    public const string COIN_POINT = "CoinPoint";
    public const string COIN = "Coin";
    public const string PLUS = "Plus";
    public const string MINUS = "Minus";
    public const string MULTIPLY = "Multiply";
    public const string PLAYER_TAG = "Player";
    public const string ENEMY_TAG = "Enemy";
    public const string HAND_TAG = "Hand";
    public const string REWARD_TAG = "Reward";
    public const string SLOT_TAG = "Slot";
    public const string INVEN_TAG = "Inven";
    public const string HIT_COLOR = "CC2424";
    public const string ATTACK_COLOR = "CC2424";
    public const string SHIELD_COLOR = "16E9FC";
    public const string HEAL_COLOR = "81F65A";
    public const string GOLD_NOT_ENOUGH_COLOR = "F5D32D";

    // 씬 넘버
    public const int TITLE_SCENE = 0;
    public const int LOADING_SCENE = 1;
    public const int BATTLE_SCENE = 2;
    public const int MAP_SCENE = 3;
    public const int SHOP_SCENE = 4;

    // 단순 상수
    public const int ONE = 1;
    public const int ZERO = 0;

    // 전투 연출 타이머 용 상수
    public const int COIN_FLIP_TIMER = 500;
    public const int COIN_NEXT_TIMER = 300;
    public const int BATTLE_END_DELAY = 1000;
    public const float MOVE_TIMER = 0.8f;

    // 패 버릴 때 필요한 상수
    public static float HAND_DROP_JUMP_POWER = 30f;
    public const float HAND_DROP_TURN_TIME = 0.2f;
    public const float HAND_DROP_TIME = 0.15f;
    public const float HAND_DROP_SCALE = 0.3f;
    public const float HAND_DROP_GAP = 0.03f;

    // 드로우할 때 타이머
    public const float DRAW_JUMP_POWER = 150f;
    public const float DRAW_TURN_TIME = 0.25f;
    public const float DRAW_TIME = 0.2f;
    public const float HAND_SORT_TIME = 0.015f;
    public const float DRAW_GAP = 0.2f;

    // 전투 관련 상수
    public const int DEFAULT_SANITY_VALUE = 50;
    public const int SKELETON_SPAWN_WEIGHT = 80;
    public const int MAX_ENEMY_COUNT = 3;
    public const int ENEMY_HAND_MAX_COUNT = 3;
    public const int MAX_ELITE_SPAWN_COUNT = 2;
    public const int REBOUND_SANITY_COST = 5;
    public const int MAX_SANITY = 95;
    public const int MIN_SANITY = 30;

    // 카드 관련
    public const int PLAYER_CARD_ID_START = 1000;
    public const int ENEMY_CARD_ID_START = 5000;
    public const float CARD_EXPAND_TIMER = 0.15f;
    public const float CARD_EXPAND_SCALE = 1.25f;
    public const float CARD_DEFAULT_EXPAND_SCALE = 1.1f;
    public const float STAR_DEFAULT_EXPAND_SCALE = 3f;
    public const int CARD_MAX_UPGRADE_COUNT = 5;
    public const int COIN_NAX_COUNT = 5;

    //인벤토리 관련
    public const float INVEN_CARD_SCALE = 0.85f;
    public const float ROTATE_MIN_ANGlE = -4;
    public const float ROTATE_MAX_ANGlE = 4;

    //게임 관련
    public const int DEFAULT_MAX_CARD_REWARD_GOLD = 100;
    public const int REWARD_SORT_ORDER = 101;
    public const int DEFAULT_PLAYER_ROUND_CLEAR_MAX_HP_GAIN = 5;
    public const int DEFAULT_PAYER_ROUND_CLEAR_SANITY_GAIN = 5;
    public const string REWARD_SORT_LAYER_NAME = "StatusUI";
    public const float DEFAULT_FADE_TIME = 0.3f;
    public const int NORMAL_GOLD_REWARD_MIN = 20;
    public const int NORMAL_GOLD_REWARD_MAX = 51;
    public const int ELITE_GOLD_REWARD_MIN = 35;
    public const int ELITE_GOLD_REWARD_MAX = 81;
    public const int BOSS_GOLD_REWARD_MIN = 100;
    public const int BOSS_GOLD_REWARD_MAX = 151;

    public const int ENEMY_THREE_COUNT_PERCENT = 65;
    public const int ENEMY_TWO_COUNT_PERCENT = 20;
    public const int ENEMY_ONE_COUNT_PERCENT = 15;

    public const int SHOP_CARD_MIN_PRICE = 100;
    public const int SHOP_CARD_MAX_PRICE = 201;

    // 저장 관련 상수
    public const string SAVE_FILE_NAME = "savefile.dat";
    public const string OPTION_FILE_NAME = "option.dat";
    public const string SAVE_FILE_ROOT_NAME = "saves";

    // 사운드 관련
    public const string MASTER_PARAM = "MasterVolume";
    public const string SYSTEM_SFX_PARAM = "SystemSFXVolume";
    public const string GAME_SFX_PARAM = "GameSFXVolume";
    public const string BGM_PARAM = "BackgroundSFXVolume";

    // 맵 관련
    public const float NODE_LINE_BY_THREE_ABLE = 0.85f;
    public const float NODE_LINE_BY_TWO_ABLE = 0.55f;

    //상점, 보상 관련
    public const float CARD_GET_ANIMATION_TIME = 0.5f;
    public const float CARD_UPGRADE_ANIMATION_TIME = 0.2f;
    public const float CARD_STAR_ANIMATION_TIME = 0.5f;
}
