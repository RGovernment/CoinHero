using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;
using static Enums;

using SF = UnityEngine.SerializeField;

public class RestAreaManager : MonoBehaviour
{
    [Header("UI 관련")]
    [SF] private CanvasGroup fadeCanvas;
    [SF] private Button restBtn;
    [SF] private Button exitBtn;
    [SF] private Image HPImage;
    [SF] private TextMeshProUGUI HPText;
    [SF] private TextMeshProUGUI maxHPText;

    private int nowHp;
    private int maxHp;

    private Tween hpTween;
    private Tween maxHpTween;

    private Player data;

    private void Start()
    {
        RestAreaIn();
        data = GameManager.Instance.state.playerData;

        data.OnHPChanged += HPUIChange;
        data.OnMaxHPChanged += MaxHPChange;

        maxHPText.text = data.MaxHP.ToString();
        HPText.text = data.HP.ToString();
        maxHp = data.MaxHP;
        nowHp = data.HP;
        HPImage.fillAmount = Mathf.InverseLerp(0, maxHp, nowHp);
    }

    public void MaxHPChange(int now, int set) 
    {
        if (maxHpTween != null && maxHpTween.IsActive())
            maxHpTween.Kill();

        int nowValue = now;
        maxHPText.text = nowValue.ToString();

        maxHpTween = DOTween.To(() => nowValue,
            x => nowValue = x, set, 0.3f)
            .OnUpdate(() =>
            {
                maxHPText.text = nowValue.ToString();
            })
            .SetEase(Ease.OutQuad);
    }

    public void HPUIChange(int now, int set)
    {
        if (hpTween != null && hpTween.IsActive())
            hpTween.Kill();


        float value = Mathf.InverseLerp(0, maxHp, set);

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

    public void UseRest()
    {
        restBtn.interactable = false;

        Player data = GameManager.Instance.state.playerData;

        if (data.HP < data.MaxHP)
        {
            data.SetHP(data.MaxHP);
            nowHp = data.HP;
        }
        else
        {
            data.SetMaxHP(data.MaxHP + DEFAULT_PLAYER_ROUND_CLEAR_MAX_HP_GAIN);
            data.SetHP(data.MaxHP);
            maxHp = data.MaxHP;
            nowHp = data.HP;
        }

        SaveManager.Instance.Save().Forget();
    }

    private void RestAreaIn()
    {
        fadeCanvas.alpha = 0;
        fadeCanvas.gameObject.SetActive(true);
        fadeCanvas.DOFade(ONE, DEFAULT_FADE_TIME).OnComplete(() =>
        {
            fadeCanvas.gameObject.SetActive(false);
        });
    }

    public void RestAreaExitBtn()
    {
        exitBtn.interactable = false;
        RestAreaExit().Forget();
    }

    private async UniTask RestAreaExit()
    {
        fadeCanvas.alpha = 1;
        fadeCanvas.gameObject.SetActive(true);
        await fadeCanvas.DOFade(ONE, DEFAULT_FADE_TIME).ToUniTask();

        SceneManager.LoadScene((int)SceneType.Map);
    }

    private void OnDestroy()
    {
        data.OnHPChanged -= HPUIChange;
        data.OnMaxHPChanged -= MaxHPChange;
    }
}
