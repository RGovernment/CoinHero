using NUnit.Framework;
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
            state = new();
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

    public void PlayerSetting(Character chara)
    {
        //로드 대신 임시 지정
        // 차후 로드되면 현재의 배틀 매니저 > 게임 매니저로 되어있는 역순 호출을
        // 게임 매니저 > 배틀 매니저로의 로드로 정리할 것
        state.playerData = new Player(chara.Id, chara.Name, chara.MaxHP, chara.CardList);
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
