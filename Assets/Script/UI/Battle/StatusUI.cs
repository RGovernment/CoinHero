using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;
using static Constants;
using SF = UnityEngine.SerializeField;

public class StatusUI : MonoBehaviour
{
    public ICombat combat;

    [Header("이미지")]
    [SF] private Image HPImage;
    [SF] private Image SPImage;
    [SF] private Image SanityImage;
    [SF] private Image ArrowImage;

    [Header("텍스트")]
    [SF] private TextMeshProUGUI HPText;
    [SF] private TextMeshProUGUI SPText;
    [SF] private TextMeshProUGUI SanityText;

    private Tween hpTween;
    private Tween spTween;
    private Tween sanityTween;

    private Tween arrowTween;

    private void OnEnable()
    {
        combat.Character.OnSanityChanged += SetSanityUI;
        combat.Character.OnHPChanged += SetHPUI;
        combat.Character.OnSPChanged += SetSPUI;
    }

    private void OnDisable()
    {
        combat.Character.OnSanityChanged -= SetSanityUI;
        combat.Character.OnHPChanged -= SetHPUI;
        combat.Character.OnSPChanged -= SetSPUI;
    }

    /// <summary>
    /// 연동 세팅
    /// </summary>
    /// <param name="HP"></param>
    /// <param name="SP"></param>
    /// <param name="Sanity"></param>
    public void Init(int HP, int SP, int Sanity)
    {
        SetHPUI(0, HP);
        SetSPUI(0, SP);
        SetSanityUI(0, Sanity);
    }

    /// <summary>
    /// HP 변경시 작동하는 HP UI 변경 함수
    /// </summary>
    /// <param name="now"></param>
    /// <param name="set"></param>
    public void SetHPUI(int now, int set)
    {
        if (hpTween != null && hpTween.IsActive())
            hpTween.Kill();


        float value = Mathf.InverseLerp(0, combat.Character.MaxHP, set);

        HPImage.DOFillAmount(value, 0.3f);

        int nowValue = now;
        HPText.text = nowValue.ToString();

        hpTween = DOTween.To(() => nowValue, 
            x => nowValue = x, set, 0.3f)
            .OnUpdate(() =>
            {
                HPText.text = nowValue.ToString();
            })
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// SP 변경시 작동하는 SP UI 변경 함수
    /// </summary>
    /// <param name="now"></param>
    /// <param name="set"></param>
    public void SetSPUI(int now, int set)
    {
        if (spTween != null && spTween.IsActive())
            spTween.Kill();

        if (set > 0)
        {
            SPImage.gameObject.SetActive(true);
            SPText.gameObject.SetActive(true);
        }

        int nowValue = now;
        SPText.text = nowValue.ToString();

        DOTween.To(() => nowValue,
            x => nowValue = x, set, 0.3f)
            .OnUpdate(() =>
            {
                SPText.text = $"+{nowValue}";
            })
            .OnComplete(() => 
            { 
                if (set == 0)
                {
                    SPImage.gameObject.SetActive(false);
                    SPText.gameObject.SetActive(false);
                }   
            })
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// Sanity 변경시 작동하는 Sanity UI 변경 함수
    /// </summary>
    /// <param name="now"></param>
    /// <param name="set"></param>
    public void SetSanityUI(int now, int set)
    {
        if (sanityTween != null && sanityTween.IsActive())
            sanityTween.Kill();


        float value = Mathf.InverseLerp(MIN_SANITY, MAX_SANITY, set);

        SanityImage.DOFillAmount(value, 0.3f);

        int nowValue = now;
        SanityText.text = nowValue.ToString();

        DOTween.To(() => nowValue,
            x => nowValue = x, set, 0.3f)
            .OnUpdate(() =>
            {
                SanityText.text = nowValue.ToString();
            })
            .SetEase(Ease.OutQuad);
    }

    /// <summary>
    /// 캐릭터 상단 화살표 활성화
    /// </summary>
    public void ArrowImageActive()
    {
        Vector2 ancher = ArrowImage.rectTransform.anchoredPosition;

        if (combat is PlayerCombat)
        {
            ArrowImage.rectTransform.anchoredPosition = new Vector2(1, ancher.y);
            if(ColorUtility.TryParseHtmlString("#4181F0", out Color color))
                ArrowImage.color = color;

        }
        else
        {
            ArrowImage.rectTransform.anchoredPosition = new Vector2(-1, ancher.y);
            if (ColorUtility.TryParseHtmlString("#D41616", out Color color))
                ArrowImage.color = color;
        }
        ArrowImage.gameObject.SetActive(true);
        ArrowImage.rectTransform
            .DOAnchorPosY(1.2f, 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear);
    }

    /// <summary>
    /// 캐릭터 상단 화살표 비활성화
    /// </summary>
    public void ArrowImageDeActive()
    {
        Vector2 ancher = ArrowImage.rectTransform.anchoredPosition;
        arrowTween.Kill();
        ArrowImage.gameObject.SetActive(false);
        ArrowImage.rectTransform.anchoredPosition = new Vector2(ancher.x, 1.1f);
    }

}
