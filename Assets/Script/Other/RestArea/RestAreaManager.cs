using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Enums;
using static Constants;

using SF = UnityEngine.SerializeField;

public class RestAreaManager : MonoBehaviour
{
    [Header("UI 관련")]
    [SF] private CanvasGroup fadeCanvas;
    [SF] private Button exitBtn;

    private void Start()
    {
        RestAreaIn();
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
}
