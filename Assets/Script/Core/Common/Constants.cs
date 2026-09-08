public static class Constants
{
    public static string ASSET_DATA_PATH = "Data/";
    public static string CARD_IMAGE_PATH = "IgnoreImage/CardImage/";

    public static string STATUS_EFFECT_DATA = "StatusEffectData";
    public static string PLAYER_CARD_DATA = "PlayerCardData";
    public static string ENEMY_CARD_DATA = "EnemyCardData";
    public static string ENEMY_DATA = "EnemyData";
    public static string PLAYER_DATA = "PlayerData";
    public static string BOSS = "Boss";
    public static string ELITE = "Elite";
    public static string ZOMBIE = "Zombie";
    public static string SKELETON = "Skeleton";

    public static string VALUE = "Value";
    public static string DURATION = "Duration";
    public static string COIN_POINT = "CoinPoint";
    public static string COIN = "Coin";
    public static string PLUS = "Plus";
    public static string MINUS = "Minus";
    public static string MULTIPLY = "Multiply";
    public static string PLAYER_TAG = "Player";
    public static string ENEMY_TAG = "Enemy";
    public static string HAND_TAG = "Hand";
    public static string REWARD_TAG = "Reward";
    public static string SLOT_TAG = "Slot";
    public static string INVEN_TAG = "Inven";

    public static string HIT_COLOR = "CC2424";
    public static string ATTACK_COLOR = "CC2424";
    public static string SHIELD_COLOR = "16E9FC";
    public static string HEAL_COLOR = "81F65A";

    // 씬 넘버
    public static int TITLE_SCENE = 0;
    public static int LOADING_SCENE = 1;
    public static int BATTLE_SCENE = 2;
    public static int MAP_SCENE = 3;
    public static int SHOP_SCENE = 4;

    // 단순 상수
    public static int ONE = 1;
    public static int ZERO = 0;

    // 전투 연출 타이머 용 상수
    public static int COIN_FLIP_TIMER = 500;
    public static int COIN_NEXT_TIMER = 300;
    public static int BATTLE_END_DELAY = 1000;
    public static float MOVE_TIMER = 0.8f;

    // 패 버릴 때 필요한 상수
    public static float HAND_DROP_JUMP_POWER = 30f;
    public static float HAND_DROP_TURN_TIME = 0.2f;
    public static float HAND_DROP_TIME = 0.15f;
    public static float HAND_DROP_SCALE = 0.3f;
    public static float HAND_DROP_GAP = 0.03f;

    // 드로우할 때 타이머
    public static float DRAW_JUMP_POWER = 150f;
    public static float DRAW_TURN_TIME = 0.25f;
    public static float DRAW_TIME = 0.2f;
    public static float HAND_SORT_TIME = 0.015f;
    public static float DRAW_GAP = 0.2f;

    // 전투 관련 상수
    public static int MAX_ENEMY_COUNT = 3;
    public static int REBOUND_SANITY_COST = 5;
    public static int MAX_SANITY = 95;
    public static int MIN_SANITY = 30;

    // 카드 관련
    public static int PLAYER_CARD_ID_START = 1000;
    public static int ENEMY_CARD_ID_START = 5000;
    public static float CARD_EXPAND_TIMER = 0.15f;
    public static float CARD_EXPAND_SCALE = 1.25f;
    public static float CARD_DEFAULT_EXPAND_SCALE = 1.1f;
    public static float STAR_DEFAULT_EXPAND_SCALE = 3f;
    public static int CARD_MAX_UPGRADE_COUNT = 5;

    //인벤토리 관련
    public static float INVEN_CARD_SCALE = 0.85f;
    public static float ROTATE_MIN_ANGlE = -4;
    public static float ROTATE_MAX_ANGlE = 4;

    //게임 관련
    public static int DEFAULT_MAX_CARD_REWARD_GOLD = 100;
    public static int REWARD_SORT_ORDER = 101;
    public static int DEFAULT_PLAYER_ROUND_CLEAR_MAX_HP_GAIN = 5;
    public static int DEFAULT_PAYER_ROUND_CLEAR_SANITY_GAIN = 5;
    public static string REWARD_SORT_LAYER_NAME = "StatusUI";
    public static float DEFAULT_FADE_TIME = 0.3f;
    public static int NORMAL_GOLD_REWARD_MIN = 20;
    public static int NORMAL_GOLD_REWARD_MAX = 51;
    public static int ELITE_GOLD_REWARD_MIN = 35;
    public static int ELITE_GOLD_REWARD_MAX = 81;
    public static int BOSS_GOLD_REWARD_MIN = 100;
    public static int BOSS_GOLD_REWARD_MAX = 151;

    public static int ENEMY_THREE_COUNT_PERCENT = 65;
    public static int ENEMY_TWO_COUNT_PERCENT = 20;
    public static int ENEMY_ONE_COUNT_PERCENT = 15;

    public static int SHOP_CARD_MIN_PRICE = 100;
    public static int SHOP_CARD_MAX_PRICE = 201;

    // 저장 관련 상수
    public static string SAVE_FILE_NAME = "savefile.dat";
    public static string OPTION_FILE_NAME = "option.dat";
    public static string SAVE_FILE_ROOT_NAME = "saves";

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
