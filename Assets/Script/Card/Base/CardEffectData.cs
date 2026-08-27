using System;
using UnityEngine;

[Serializable]
public class CardEffectData
{
    public int EffectId;

    public int Value;

    // 지속 턴 수가 -1일 경우 영구
    public int Duration;
}
