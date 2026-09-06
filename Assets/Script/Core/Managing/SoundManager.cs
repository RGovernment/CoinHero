using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using static Constants;
using static Enums;
using SF = UnityEngine.SerializeField;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public struct SystemSoundData
    {
        public SystemSoundType type;
        public AudioClip audioClip;
    }

    [Serializable]
    public struct BackgroundSoundData
    {
        public SceneType type;
        public AudioClip audioClip;
    }

    [Serializable]
    public struct BattleSoundData
    {
        public BattleSoundType type;
        public AudioClip[] audioClip;
    }

    [SF] private AudioMixer mainMixer;
    [SF] private AudioSource systemAudioSource;
    [SF] private AudioSource bgmSource;
    [SF] private SystemSoundData[] audioClips;
    [SF] private BackgroundSoundData[] backgroundAudioClips;
    [SF] private BattleSoundData[] battleClips;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SoundRegister();
    }

    private void SoundRegister()
    {
        Dictionary<SystemSoundType, AudioClip> dict = ResourceManager.Instance.SystemSoundData;
        Dictionary<SceneType, AudioClip> dict2 = ResourceManager.Instance.BackgroundSoundData;

        foreach (var data in audioClips)
        {
            if (data.audioClip == null) continue;

            dict[data.type] = data.audioClip;
        }
        foreach (var data in backgroundAudioClips)
        {
            
            if(data.audioClip == null) continue;

            dict2[data.type] = data.audioClip;
        }
        foreach (var data in battleClips)
        {
            if (data.audioClip == null || data.audioClip.Length == 0) continue;
            ResourceManager.Instance.BattleSoundData[data.type] = data.audioClip;
        }
    }

    /// <summary>
    /// 전체 볼륨 조정
    /// </summary>
    /// <param name="linearValue"></param>
    public void SetMasterVolume(float value) => SetMixerVolume(MASTER_PARAM, value);

    /// <summary>
    /// 시스템 사운드 볼륨 조정
    /// </summary>
    /// <param name="linearValue"></param>
    public void SetSystemSFXVolume(float value) => SetMixerVolume(SYSTEM_SFX_PARAM, value);
    /// <summary>
    /// 게임 효과음 볼륨 조정
    /// </summary>
    /// <param name="linearValue"></param>
    public void SetGameSFXVolume(float value) => SetMixerVolume(GAME_SFX_PARAM, value);

    /// <summary>
    /// 배경음 볼륨 조정
    /// </summary>
    /// <param name="value"></param>
    public void SetBGMVolume(float value) => SetMixerVolume(BGM_PARAM, value);

    private void SetMixerVolume(string parameterName, float value)
    {
        value = Mathf.Clamp(value, 0.0001f, 1f);

        float dB = Mathf.Log10(value) * 20f;
        mainMixer.SetFloat(parameterName, dB);
    }

    public void PlayBGM(SceneType type, bool loop = true)
    {
        if(!ResourceManager.Instance.BackgroundSoundData.TryGetValue(type, out AudioClip clip)) return;
        if (clip == null) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    /// <summary>
    /// 버튼 클릭, 카드 클릭 등 시스템 사운드 재생
    /// </summary>
    public void PlaySystemSFX(SystemSoundType type)
    {
        if(!ResourceManager.Instance.SystemSoundData.TryGetValue(type, out AudioClip clip)) return;
        if (clip == null) return;

        systemAudioSource.PlayOneShot(clip);
    }

    public void PlayBattleSFX(BattleSoundType type)
    {
        AudioClip[] clips = ResourceManager.Instance.BattleSoundData[type];

        int index = UnityEngine.Random.Range(0, clips.Length);

        if(clips[index] == null) return;

        systemAudioSource.PlayOneShot(clips[index]);
    }
}