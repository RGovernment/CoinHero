using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Enums;

using HIn = UnityEngine.HideInInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance {  get; private set; }

    public GameState state;

    public OptionState optionData;

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
            optionData = new OptionState()
            {
                SoundData = new()
            };
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Start()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBGM(SceneType.Title);
        SaveManager.Instance.OptionLoad();
    }

    public void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneMusicChanged;
    }

    public void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneMusicChanged;
    }

    public void UpdateState(GameState newState)
    {
        Player data = newState.playerData;
        state = new()
        {
            playerData = new Player(
                data.Id, data.Name, data.MaxHP, data.HP, data.ClassType,
                new(data.CardList)),
            NowRound = newState.NowRound,
            IsBoss = newState.IsBoss,
            IsBattle = newState.IsBattle,
            IsTutorialCompleted = newState.IsTutorialCompleted,
            nextRoundEnemies = new(newState.nextRoundEnemies),
            mapStatus = new MapGraphData()
            {
                nodes = newState.mapStatus.nodes,
            },
            currentMapNodeId = newState.currentMapNodeId,
            gold = newState.gold
        };
    }

    public void UpdateState(OptionState newState)
    {
        optionData = new()
        {
            SoundData = new(newState.SoundData)
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
