using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ICombat
{
    public Character Character { get; set; }
    public BattleCoinUI CoinUI { get; set; }
    public CombatAnimatorManager AnimatorManager { get; set; }
    public Animator Animator { get; set; }

    /// <summary>
    /// 코인 출력, 세 코인 전부 
    /// </summary>
    /// <returns>코인 값</returns>
    public bool[] CoinToss(Card card, int SanitySet = -1);
    public int TotalValueByWin(Card card, int APDiscount = 0);
    public int APDiscountByLose(Card card);

}
