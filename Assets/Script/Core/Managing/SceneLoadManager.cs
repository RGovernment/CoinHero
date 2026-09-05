using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Constants;

using SF = UnityEngine.SerializeField;


public class SceneLoadManager : MonoBehaviour
{

    private static readonly int _1MoveHash = Animator.StringToHash("1_Move");

    [SF] private GameObject LoadingGroup;
    [SF] private Image loadingBar;
    [SF] private TextMeshProUGUI loadingValueText;
    [SF] private CanvasGroup fade;
    [SF] private Color startColor;
    [SF] private Color middleColor;
    [SF] private Color endColor;
    [SF] private Animator charaAnimator;

    private readonly float fadeDuration = 1f; // 페이드 인/아웃 시간
    private readonly float minLoadingTime = 2.0f; // 최소 로딩 시간 보장

    void Start()
    {
        LoadSceneAsyncWithDelay().Forget();
        charaAnimator.SetBool(_1MoveHash, true);
        loadingBar.fillAmount = ZERO;
        loadingValueText.text = $"{ZERO}%";
    }

    private async UniTask LoadSceneAsyncWithDelay()
    {
        fade.alpha = 1;
        fade.gameObject.SetActive(true);
        // 페이드 인 (검정 → 투명)
        await fade.DOFade(0, fadeDuration).ToUniTask();
        fade.gameObject.SetActive(false);

        // 비동기 씬 로드 시작
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)GameManager.Instance.nextScene);
        asyncLoad.allowSceneActivation = false;

        float targetProgress = 0;
        float currentDisplayProgress = 0;
        float elapsed = 0f;

        while (asyncLoad.progress < 0.9f || elapsed < minLoadingTime)
        {
            await UniTask.DelayFrame(ONE);
            elapsed += Time.deltaTime;

            float realProgress = asyncLoad.progress >= 0.9f ? 1f : asyncLoad.progress;
            float timeProgress = Mathf.Clamp01(elapsed / minLoadingTime);

            targetProgress = Mathf.Min(realProgress, timeProgress);
            currentDisplayProgress =
                Mathf.MoveTowards(currentDisplayProgress, targetProgress, Time.deltaTime * 2f);

            if (loadingBar != null)
            {
                loadingValueText.text = $"{Mathf.Round(targetProgress * 100)}%";
                loadingBar.fillAmount = targetProgress;
            }
        }

        // 로딩바 100% 채우기
        if (loadingBar != null)
        {
            loadingValueText.text = "100%";
            loadingBar.fillAmount = ONE;
        }


        // 로딩 완료 후 페이드 아웃 (투명 → 검정)
        fade.gameObject.SetActive(true);

        await fade.DOFade(1, fadeDuration).ToUniTask();
        charaAnimator.SetBool("1_Move", false);
        // 씬 활성화
        asyncLoad.allowSceneActivation = true;
    }
}
