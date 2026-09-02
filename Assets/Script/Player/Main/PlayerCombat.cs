using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using static Constants;
using SF = UnityEngine.SerializeField;

public class PlayerCombat : MonoBehaviour, ICombat
{
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");
    public Character Character { get; set; }

    [SF] private Transform baseCharaObj;
    [SF] private Animator animator;
    [SF] private BattleCoinUI coinUI;
    [SF] private CombatAnimatorManager animatorManager;
    [SF] private StatusUI statUI;
    public Transform BaseCharaObj { get => baseCharaObj; set => baseCharaObj = value; }
    public CombatAnimatorManager AnimatorManager { get => animatorManager; set => animatorManager = value; }
    public BattleCoinUI CoinUI { get => coinUI; set => coinUI = value; }
    public Animator Animator { get => animator; set => animator = value; }

    private SpriteRenderer[] renders;
    private MaterialPropertyBlock hitMat;

    private void Awake()
    {
        // 임시 데이터, 플레이어별 시작 카드 프리셋을 만들어둘 것
        List<Card> cd = new();
        hitMat = new();
        foreach (var item in ResourceManager.Instance.CardData)
        {
            cd.Add(item.Value);
        }

        Character = new Player(10, "플레이어", 50, cd);
        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
        statUI.combat = this;
        statUI.Init(Character.HP, Character.SP, Character.Sanity);
        renders = animator.transform.GetComponentsInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        Character.OnHPHit += DamageSkinSpawn;
        Character.OnSPHit += SPDamageSkinSpawn;
        Character.OnHPHeal += HealSkinSpawn;
    }

    private void OnDisable()
    {
        Character.OnHPHit -= DamageSkinSpawn;
        Character.OnSPHit -= SPDamageSkinSpawn;
        Character.OnHPHeal -= HealSkinSpawn;
    }

    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
    }

    public bool[] CoinToss(Card card, int SanitySet = -1)
    {
        int coinCount = card.FinalCoin();

        bool[] result = new bool[coinCount];
        for (int i = 0; i < coinCount; i++)
        {
            result[i] = (SanitySet < 0 ? Character.Sanity : SanitySet) < Random.Range(0, 100);
        }

        return result;
    }
    
    public int TotalValueByWin(Card card, int APDiscount = 0)
    {
        // 승리 시 밸류 + 남은 코인 * 코인 위력 리턴
        return Mathf.Max(1, (card.FinalValue()  + card.Coin * card.FinalCoinPoint()) - APDiscount);
    }

    public int APDiscountByLose(Card card)
    {
        return card.Value + card.Coin;
    }

    private void DamageSkinSpawn(int damage, Character chara) 
    {
        HitColor().Forget();
        DamageSkinSpawner.Instance
            .DamageSkinSpawn(transform.position + new Vector3(0, 1f, 0), damage);
    }

    private void SPDamageSkinSpawn(int damage, Character chara)
    {
        HitColor(false).Forget();
        DamageSkinSpawner.Instance
            .SPDamageSkinSpawn(transform.position + new Vector3(0, 1f, 0), damage);
    }

    private void HealSkinSpawn(int heal, Character chara)
    {
        DamageSkinSpawner.Instance
            .HealSkinSpawn(transform.position + new Vector3(0, 1f, 0), heal);
    }

    private async UniTask HitColor(bool isHP = true)
    {
        if (isHP)
            hitMat.SetColor(ColorProperty, Color.darkRed);

        else if (ColorUtility.TryParseHtmlString($"#{SHIELD_COLOR}", out Color shield))
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
