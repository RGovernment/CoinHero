using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Enums;
using static Constants;

using SF = UnityEngine.SerializeField;
using Cysharp.Threading.Tasks;

public class TitleManager : MonoBehaviour
{
    [SF] private Button startBtn;
    [SF] private Button closeBtn;
    [SF] private CanvasGroup titlePanel;

    private void Start()
    {
        titlePanel.alpha = ZERO;
        titlePanel.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        GameManager.Instance.nextScene = SceneType.Battle;
        // 임시로 즉시 이동
        SceneNext().Forget();
    }

    public async UniTask SceneNext()
    {
        titlePanel.gameObject.SetActive(true);
        await titlePanel.DOFade(ONE, DEFAULT_FADE_TIME);
        SceneManager.LoadScene((int)SceneType.Loading);
    }

    public async UniTask PanelFadeOut()
    {
        await titlePanel.DOFade(ZERO, DEFAULT_FADE_TIME);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
