using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Constants;
using static Enums;
using SF = UnityEngine.SerializeField;

public class OptionManager : MonoBehaviour
{
    public static OptionManager Instance {  get; private set; }

    [SF]private CanvasGroup OptionGroup;
    [SF] private Slider MasterSlider;
    [SF] private Slider BGMSlider;
    [SF] private Slider SystemSlider;
    [SF] private Slider GameSlider;
    [SF] private TextMeshProUGUI masterText;
    [SF] private TextMeshProUGUI bgmText;
    [SF] private TextMeshProUGUI systemText;
    [SF] private TextMeshProUGUI gameText;
    [SF] private Button closeBtn;
    [SF] private Button gameExitBtn;


    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SoundSliderSetting();
        MasterSlider.onValueChanged
            .AddListener(x => { SetMasterVolume(SoundMixerType.Master, x); });
        BGMSlider.onValueChanged
            .AddListener(x => { SetMasterVolume(SoundMixerType.BGM, x); });
        GameSlider.onValueChanged
            .AddListener(x => { SetMasterVolume(SoundMixerType.Game, x); });
        SystemSlider.onValueChanged
            .AddListener(x => { SetMasterVolume(SoundMixerType.System, x); });
    }

    private void SoundSliderSetting()
    {
        if(GameManager.Instance.optionData.SoundData
            .TryGetValue(SoundMixerType.Master, out float masterVolume))
            SoundManager.Instance.SetMasterVolume(masterVolume);

        if (GameManager.Instance.optionData.SoundData
            .TryGetValue(SoundMixerType.System, out float systemVolume))
            SoundManager.Instance.SetSystemSFXVolume(systemVolume);

        if (GameManager.Instance.optionData.SoundData
            .TryGetValue(SoundMixerType.Game, out float gameVolume))
            SoundManager.Instance.SetGameSFXVolume(gameVolume);

        if (GameManager.Instance.optionData.SoundData
            .TryGetValue(SoundMixerType.BGM, out float bgmVolume))
            SoundManager.Instance.SetBGMVolume(bgmVolume);

        float masterCk = SoundManager.Instance.GetMixerVolume(SoundMixerType.Master);
        float systemCk = SoundManager.Instance.GetMixerVolume(SoundMixerType.System);
        float gameCk = SoundManager.Instance.GetMixerVolume(SoundMixerType.Game);
        float bgmCk = SoundManager.Instance.GetMixerVolume(SoundMixerType.BGM);

        MasterSlider.value = masterCk;
        SystemSlider.value = systemCk;
        GameSlider.value = gameCk;
        BGMSlider.value = bgmCk;

        masterText.text = Mathf.RoundToInt(masterCk * 100).ToString();
        systemText.text = Mathf.RoundToInt(systemCk * 100).ToString();
        gameText.text = Mathf.RoundToInt(gameCk * 100).ToString();
        bgmText.text = Mathf.RoundToInt(bgmCk * 100).ToString();
    }

    public void SetMasterVolume(SoundMixerType type, float value)
    {
        switch (type)
        {
            case SoundMixerType.Master:
                SoundManager.Instance.SetMasterVolume(value);
                masterText.text = Mathf.RoundToInt(value * 100).ToString();
                break;
            case SoundMixerType.System:
                SoundManager.Instance.SetSystemSFXVolume(value);
                systemText.text = Mathf.RoundToInt(value * 100).ToString();
                break;
            case SoundMixerType.BGM:
                SoundManager.Instance.SetBGMVolume(value);
                bgmText.text = Mathf.RoundToInt(value * 100).ToString();
                break;
            case SoundMixerType.Game:
                SoundManager.Instance.SetGameSFXVolume(value);
                gameText.text = Mathf.RoundToInt(value * 100).ToString();
                break;
        }
    }

    public void OptionPanelOpen()
    {
        OptionGroup.alpha = 0;
        OptionGroup.gameObject.SetActive(true);
        OptionGroup.DOFade(ONE, DEFAULT_FADE_TIME);
    }


    public void OptionPanelClose()
    {
        
        Dictionary<SoundMixerType, float> data = new()
        {
            [SoundMixerType.Master] = SoundManager.Instance.GetMixerVolume(SoundMixerType.Master),
            [SoundMixerType.System] = SoundManager.Instance.GetMixerVolume(SoundMixerType.System),
            [SoundMixerType.Game] = SoundManager.Instance.GetMixerVolume(SoundMixerType.Game),
            [SoundMixerType.BGM] = SoundManager.Instance.GetMixerVolume(SoundMixerType.BGM)
        };
        GameManager.Instance.optionData.SoundData = data;

        OptionGroup.DOFade(ZERO, DEFAULT_FADE_TIME)
            .OnComplete(() => OptionGroup.gameObject.SetActive(false));
        
        SaveManager.Instance.OptionSave().Forget();
    }

    public void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
