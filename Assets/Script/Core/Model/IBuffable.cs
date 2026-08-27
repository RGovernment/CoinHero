using UnityEngine;

public interface IBuffable
{
    /// <summary>
    /// 효과 부여
    /// </summary>
    /// <param name="effect"></param>
    public void TakeEffect(StatusEffect effect);

    /// <summary>
    /// 효과 제거
    /// </summary>
    public void RemoveEffect();
}
