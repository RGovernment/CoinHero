using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Enums;

using HIn = UnityEngine.HideInInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }

    public GameState state;

    [HIn] public SceneType nextScene;
    [HIn] public SceneType nowScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            state = new()
            {
                nextRoundEnemies = new()
            };
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneMusicChanged;
    }

    public void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneMusicChanged;
    }

    public void Start()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SceneType.Title);
    }

    public void UpdateState(GameState newState)
    {
        state = new()
        {
            playerData = newState.playerData,
            NowRound = newState.NowRound,
            IsBoss = newState.IsBoss,
            gold = newState.gold
        };
    }

    private void SceneMusicChanged(Scene arg0, Scene arg1)
    {
        if (SoundManager.Instance == null) return;
        SoundManager.Instance.StopBGM();

        int index = arg1.buildIndex;
        SoundManager.Instance.PlayBGM((SceneType)index);
    }
}
