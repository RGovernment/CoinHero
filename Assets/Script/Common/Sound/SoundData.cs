using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using static Enums;

public class SoundData : MonoBehaviour
{
    [JsonConverter(typeof(StringEnumConverter))]
    public SoundMixerType type;
    public float dB;
}
