using UnityEngine;

public interface ICombat
{
    public BattleCoinUI CoinUI { get; set; }
    /// <summary>
    /// 코인 출력, 세 코인 전부 
    /// </summary>
    /// <returns>코인 값</returns>
    public bool[] CoinToss(Card card);
    
}
