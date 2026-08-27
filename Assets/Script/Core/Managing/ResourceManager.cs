using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Resources;
using System.Collections.Generic;

using UnityEngine;
using static Constants;
using System.Linq;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    public Dictionary<int, Card> CardData { get; private set; }

    public Dictionary<int, StatusEffectData> EffectData { get; private set; }
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
        ResourceLoad();
    }

    /// <summary>
    /// 변경되지 않는 상시 사용 Json 리소스 로드
    /// </summary>
    public void ResourceLoad()
    {
        // 플레이어 카드 및 기본 효과 데이터 로드
        string playerCardJson 
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + PLAYER_CARD_DATA).text;
        string statusEffectJson
            = Resources.Load<TextAsset>(ASSET_DATA_PATH + STATUS_EFFECT_DATA).text;
        List<Card> cardList 
            = JsonConvert.DeserializeObject<List<Card>>(playerCardJson);

        CardData = cardList.ToDictionary(x => x.Id);

        List<StatusEffectData> effectList 
            = JsonConvert.DeserializeObject<List<StatusEffectData>>(statusEffectJson);

        EffectData = effectList.ToDictionary(x => x.Id);
    }
}
