public static class Constants
{
    public static string ASSET_DATA_PATH = "Data/";
    public static string CARD_IMAGE_PATH = "IgnoreImage/CardImage/";

    public static string STATUS_EFFECT_DATA = "StatusEffectData";
    public static string PLAYER_CARD_DATA = "PlayerCardData";
    public static string ENEMY_CARD_DATA = "EnemyCardData";
    
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
    public static string SLOT_TAG = "Slot";
    public static string INVEN_TAG = "Inven";

    public static string HIT_COLOR = "CC2424";
    public static string ATTACK_COLOR = "CC2424";
    public static string SHIELD_COLOR = "16E9FC";
    public static string HEAL_COLOR = "81F65A";

    // 전투 연출 타이머 용 상수
    public static int COIN_FLIP_TIMER = 500;
    public static int COIN_NEXT_TIMER = 300;
    public static int BATTLE_END_DELAY = 1000;
    public static float MOVE_TIMER = 0.8f;

    // 패 버릴 때 필요한 상수
    public static float HAND_DROP_JUMP_POWER = 150f;
    public static float HAND_DROP_TURN_TIME = 0.2f;
    public static float HAND_DROP_TIME = 0.1f;
    public static float HAND_DROP_SCALE = 0.3f;
    public static float HAND_DROP_GAP = 0.05f;

    // 드로우할 때 타이머
    public static float DRAW_JUMP_POWER = 300f;
    public static float DRAW_TURN_TIME = 0.25f;
    public static float DRAW_TIME = 0.2f;
    public static float HAND_SORT_TIME = 0.015f;
    public static float DRAW_GAP = 0.2f;

    public static int REBOUND_SANITY_COST = 5;
    public static int MAX_SANITY = 95;
    public static int MIN_SANITY = 30;

    // 카드 아이디 관련
    public static int PLAYER_CARD_ID_START = 1000;
    public static int ENEMY_CARD_ID_START = 5000;

    //인벤토리 관련
    public static float INVEN_CARD_SCALE = 0.9f;
    public static float ROTATE_MIN_ANGlE = -5;
    public static float ROTATE_MAX_ANGlE = 5;

}
