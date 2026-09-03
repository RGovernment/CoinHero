using Cysharp.Threading.Tasks;
using UnityEngine;
using static Constants;
using SF = UnityEngine.SerializeField;

public abstract class CombatBase : MonoBehaviour, ICombat
{
    protected static readonly int ColorProperty = Shader.PropertyToID("_Color");
    public Character Character { get; set; }
    [SF] protected Transform baseCharaObj;
    [SF] protected Animator animator;
    [SF] protected BattleCoinUI coinUI;
    [SF] protected CombatAnimatorManager animatorManager;
    [SF] protected StatusUI statUI;
    public Transform BaseCharaObj { get => baseCharaObj; set => baseCharaObj = value; }
    public CombatAnimatorManager AnimatorManager { get => animatorManager; set => animatorManager = value; }
    public BattleCoinUI CoinUI { get => coinUI; set => coinUI = value; }
    public Animator Animator { get => animator; set => animator = value; }
    public StatusUI StatUI { get => statUI; }

    protected SpriteRenderer[] renders;
    protected MaterialPropertyBlock hitMat;

    protected virtual void OnEnable()
    {
        Character.OnHPHit += DamageSkinSpawn;
        Character.OnSPHit += SPDamageSkinSpawn;
        Character.OnHPHeal += HealSkinSpawn;
    }

    protected virtual void OnDisable()
    {
        Character.OnHPHit -= DamageSkinSpawn;
        Character.OnSPHit -= SPDamageSkinSpawn;
        Character.OnHPHeal -= HealSkinSpawn;
    }

    public async UniTask RemoveDelay(Character chara)
    {
        await AnimatorManager.OnDead();
        AnimatorManager.OnDefender();
    }

    public bool[] CoinToss(Card card, int SanitySet = -1)
    {
        int coinCount = card.FinalCoin();
        int sanity = SanitySet < 0 ? Character.Sanity : SanitySet;

        bool[] result = new bool[coinCount];
        for (int i = 0; i < coinCount; i++)
        {
            result[i] = Random.Range(0, 100) < sanity;
        }

        return result;
    }

    public int TotalValueByWin(Card card, int APDiscount = 0)
    {
        // 승리 시 밸류 + 남은 코인 * 코인 위력 리턴
        return Mathf.Max(1, (card.FinalValue() + card.Coin * card.FinalCoinPoint()) - APDiscount);
    }
    public abstract int APDiscountByLose(Card card);

    protected virtual void DamageSkinSpawn(int damage, Character chara)
    {
        HitColor().Forget();
        DamageSkinSpawner.Instance.DamageSkinSpawn(transform.position + new Vector3(0, 1f, 0), damage);
    }

    protected virtual void SPDamageSkinSpawn(int damage, Character chara)
    {
        HitColor(false).Forget();
        DamageSkinSpawner.Instance
            .SPDamageSkinSpawn(transform.position + new Vector3(0, 1f, 0), damage);
    }

    protected virtual void HealSkinSpawn(int heal, Character chara)
    {
        DamageSkinSpawner.Instance
            .HealSkinSpawn(transform.position + new Vector3(0, 1f, 0), heal);
    }

    protected virtual async UniTask HitColor(bool isHP = true)
    {
        if (isHP && ColorUtility.TryParseHtmlString($"#{HIT_COLOR}", out Color hit))
            hitMat.SetColor(ColorProperty, hit);
        else if (!isHP && ColorUtility.TryParseHtmlString($"#{SHIELD_COLOR}", out Color shield))
            hitMat.SetColor(ColorProperty, shield);

        for (int i = 0; i < renders.Length; i++)
        {
            if (renders[i] != null)
                renders[i].SetPropertyBlock(hitMat);
        }

        await UniTask.Delay(100);

        hitMat.Clear();

        for (int i = 0; i < renders.Length; i++)
        {
            if (renders[i] != null)
                renders[i].SetPropertyBlock(hitMat);
        }
    }
}
