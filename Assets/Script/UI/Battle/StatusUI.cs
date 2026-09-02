using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using SF = UnityEngine.SerializeField;

public class StatusUI : MonoBehaviour
{
    public ICombat combat;

    [Header("이미지")]
    [SF] private Image HPImage;
    [SF] private Image SPImage;
    [SF] private Image SanityImage;

    [Header("텍스트")]
    [SF] private TextMeshProUGUI HPText;
    [SF] private TextMeshProUGUI SPText;
    [SF] private TextMeshProUGUI SanityText;

    private Tween hpTween;
    private Tween spTween;
    private Tween sanityTween;

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

    public void Init(int HP, int SP, int Sanity)
    {
        SetHPUI(0, HP);
        SetSPUI(0, SP);
        SetSanityUI(0, Sanity);
    }

    public void SetHPUI(int now, int set)
    {
        float value = Mathf.InverseLerp(0, combat.Character.MaxHP, set);

        HPImage.DOFillAmount(value, 0.3f);

        int nowValue = now;
        HPText.text = nowValue.ToString();

        DOTween.To(() => nowValue, 
            x => nowValue = x, set, 0.3f)
            .OnUpdate(() =>
            {
                HPText.text = nowValue.ToString();
            })
            .SetEase(Ease.OutQuad);
    }

    public void SetSPUI(int now, int set)
    {
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

    public void SetSanityUI(int now, int set)
    {
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

}
