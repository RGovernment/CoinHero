using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Constants;
using static Enums;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public Dictionary<int, Card> CardData { get; private set; }
    public Dictionary<int, Card> EnemyCardData { get; private set; }
    public Dictionary<int, Sprite> CardImageData { get; private set; }

    public Dictionary<int, StatusEffectData> EffectData { get; private set; }

    public List<Enemy> EnemyZombieData {  get; private set; }
    public List<Enemy> EnemyEliteZombieData { get; private set; }
    public List <Enemy> EnemyEliteSkeletonData { get; private set; }
    public List <Enemy> EnemySkeletonData { get; private set;  }
    public List<Enemy> EnemyBossData { get; private set; }
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
        EnemyEliteZombieData = new();
        EnemySkeletonData = new();
        EnemyEliteSkeletonData = new();
        EnemyBossData = new();
        PlayerData = new();

        ResourceLoad();
        CardImageLoad();
    }

    /// <summary>
    /// 변경되지 않는 상시 사용 Json 리소스 로드
    /// </summary>
    public void ResourceLoad()
    {
        string playerCardJson = "", enemyCardJson = "", statusEffectJson = "",
               enemyBossJson = "", enemyZombieJson = "", enemyEliteZombieJson = "",
               enemySkeletonJson = "", enemyEliteSkeletonJson = "", playerJson = "";

        try
        {
            playerCardJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + PLAYER_CARD_DATA).text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 플레이어 카드 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            enemyCardJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + ENEMY_CARD_DATA).text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 적 카드 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            statusEffectJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + STATUS_EFFECT_DATA).text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 상태 이상 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            enemyBossJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + $"{ENEMY_DATA}_{BOSS}").text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 보스 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            enemyZombieJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + $"{ENEMY_DATA}_{ZOMBIE}").text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 좀비 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            enemyEliteZombieJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + $"{ENEMY_DATA}_{ZOMBIE}_{ELITE}").text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 엘리트 좀비 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            enemySkeletonJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + $"{ENEMY_DATA}_{SKELETON}").text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 스켈레톤 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            enemyEliteSkeletonJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + $"{ENEMY_DATA}_{SKELETON}_{ELITE}").text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 엘리트 스켈레톤 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            playerJson = Resources.Load<TextAsset>(ASSET_DATA_PATH + PLAYER_DATA).text;
        }
        catch (Exception e)
        {
            Debug.LogError($"[DataLoad] 플레이어 스탯 데이터 파일 로드 실패: {e.Message}");
        }

        try
        {
            // 리스트 변환
            List<Card> cardList = JsonConvert.DeserializeObject<List<Card>>(playerCardJson);
            List<Card> enemyCardList = JsonConvert.DeserializeObject<List<Card>>(enemyCardJson);
            List<StatusEffectData> effectList = JsonConvert.DeserializeObject<List<StatusEffectData>>(statusEffectJson);

            PlayerData = JsonConvert.DeserializeObject<List<Player>>(playerJson);
            EnemyZombieData = JsonConvert.DeserializeObject<List<Enemy>>(enemyZombieJson);
            EnemyEliteZombieData = JsonConvert.DeserializeObject<List<Enemy>>(enemyEliteZombieJson);
            EnemySkeletonData = JsonConvert.DeserializeObject<List<Enemy>>(enemySkeletonJson);
            EnemyEliteSkeletonData = JsonConvert.DeserializeObject<List<Enemy>>(enemyEliteSkeletonJson);
            EnemyBossData = JsonConvert.DeserializeObject<List<Enemy>>(enemyBossJson);

            // 딕셔너리 가공
            CardData = cardList?.ToDictionary(x => x.Id) ?? new Dictionary<int, Card>();
            EnemyCardData = enemyCardList?.ToDictionary(x => x.Id) ?? new Dictionary<int, Card>();
            EffectData = effectList?.ToDictionary(x => x.Id) ?? new Dictionary<int, StatusEffectData>();
            EffectDataByType = effectList?.ToDictionary(x => x.Type) ?? new Dictionary<EffectType, StatusEffectData>();
        }
        catch (JsonException jsonEx)
        {
            // JSON 문법(쉼표 누락, 중괄호 미닫힘 등) 오류 발생 시
            Debug.LogError($"[JSON Parsing] JSON 데이터 파싱 중 문법 오류가 발생했습니다: {jsonEx.Message}");
        }
        catch (Exception e)
        {
            // 이외 런타임 에러
            Debug.LogError($"[DataProcessing] 데이터 변환 및 딕셔너리 구성 중 오류 발생: {e.Message}");
        }

    }

    private void CardImageLoad()
    {
        try
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
        catch
        {
            Debug.LogError("카드 데이터에 문제가 있습니다.");
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
