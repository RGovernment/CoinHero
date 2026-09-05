using System;
using UnityEngine;
using static Enums;
using SF = UnityEngine.SerializeField;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public struct ButtonSoundData
    {
        public ButtonSoundType type;
        public AudioClip audioClip;
    }

    [Serializable]
    public struct BackgroundSoundData
    {
        public SceneType type;
        public AudioClip audioClip;
    }


    [SF] private AudioSource systemAudioSource;
    [SF] private ButtonSoundData[] audioClips;
    [SF] private BackgroundSoundData[] backgroundAudioClips;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
