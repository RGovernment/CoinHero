using Newtonsoft.Json;
using System.Collections.Generic;

using UnityEngine;
using static Constants;
using static Enums;
using System.Linq;
using Newtonsoft.Json.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public Dictionary<int, Card> CardData { get; private set; }
    public Dictionary<int, Card> EnemyCardData { get; private set; }
    public Dictionary<int, Sprite> CardImageData { get; private set; }

    public Dictionary<int, StatusEffectData> EffectData { get; private set; }

    public List<Enemy> EnemyZombieData {  get; private set; }
    public List<Player> PlayerData {  get; private set; }

    public Dictionary<EffectType, StatusEffectData> EffectDataByType { get; private set; }
    public Dictionary<SystemSoundType, AudioClip> SystemSoundData { get; private set; }
    public Dictionary<SceneType, AudioClip> BackgroundSoundData { get; private set; }

    public Dictionary<BattleSoundType, AudioClip[]> BattleSoundData { get; private set; }



    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
             Destroy(gameObject);
        }

        CardData = new();
        EffectData = new();
        EnemyCardData = new();
        CardImageData = new();
        EffectDataByType = new();
        SystemSoundData = new();
        BackgroundSoundData = new();
        BattleSoundData = new();
        EnemyZombieData = new();
        PlayerData = new();
        ResourceLoad();
        CardImageLoad();
    }

    /// <summary>
    /// 변경되지 않는 상시 사용 Json 리소스 로드
    /// </summary>
    public void ResourceLoad()
    {
        // 플레이어 카드 및 기본 효과 데이터 로드
        string playerCardJson 
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + PLAYER_CARD_DATA).text;
        string enemyCardJson
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + ENEMY_CARD_DATA).text;
        string statusEffectJson
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + STATUS_EFFECT_DATA).text;
        string enemyZombieJson
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + $"{ENEMY_DATA}_{ZOMBIE}").text;
        string playerJson
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + PLAYER_DATA).text;

        List<Card> cardList 
            = JsonConvert.DeserializeObject<List<Card>>(playerCardJson);
        List<Card> enemyCardList
            = JsonConvert.DeserializeObject<List<Card>>(enemyCardJson);
        List<StatusEffectData> effectList 
            = JsonConvert.DeserializeObject<List<StatusEffectData>>(statusEffectJson);

        PlayerData = JsonConvert.DeserializeObject<List<Player>>(playerJson);
        EnemyZombieData = JsonConvert.DeserializeObject<List<Enemy>>(enemyZombieJson);

        CardData = cardList.ToDictionary(x => x.Id);
        EnemyCardData = enemyCardList.ToDictionary(x => x.Id);
        EffectData = effectList.ToDictionary(x => x.Id);
        EffectDataByType = effectList.ToDictionary(x => x.Type);
    }

    private void CardImageLoad()
    {
        foreach (var item in CardData)
        {
            int id = item.Value.Id;
            Sprite sprite = Resources.Load<Sprite>(CARD_IMAGE_PATH + $"Card_{id}");
            CardImageData[id] = sprite;
        }

        foreach (var item in EnemyCardData)
        {
            int id = item.Value.Id;
            Sprite sprite = Resources.Load<Sprite>(CARD_IMAGE_PATH + $"Card_{id}");
            CardImageData[id] = sprite;
        }
    }

    public Card GetCardData(int id)
    {
        if(id >= PLAYER_CARD_ID_START && id < ENEMY_CARD_ID_START)
            return CardData[id];
        
        else
            return EnemyCardData[id];
        
    }
}
