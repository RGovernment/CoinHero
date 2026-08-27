using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class CardWrite : MonoBehaviour
{
    public List<CardWriteOnly> data;
    public List<StatusEffectData> effects;
    
    public void JsonWrite()
    {
        Debug.Log(JsonConvert.SerializeObject(data, Formatting.Indented));
    }

    public void JsonEffectWrite()
    {
        Debug.Log(JsonConvert.SerializeObject(effects, Formatting.Indented));
    }
}
