using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
using SF = UnityEngine.SerializeField;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; set; }
    [SF] private HandManager handManager;
    [SF] private PlayerCombat playerCombat;
    [SF] private List<EnemyCombat> enemyCombat;
    private StateMachine state;
    private Dictionary<BattleStateType, IState> stateGroup;


    private void Awake()
    {
        Instance = this;
    }

    private async UniTaskVoid Start()
    {
        enemyCombat = new List<EnemyCombat>();
        state = new();

        stateGroup = new Dictionary<BattleStateType, IState>()
        {
            [BattleStateType.RoundStart] = new RoundStartState(this),
            [BattleStateType.TurnStart]= new TurnStartState(this),
            [BattleStateType.DrawPhase] = new DrawPhaseState(this),
            [BattleStateType.PlayerChoosePhase] = new PlayerChoosePhaseState(this),
            [BattleStateType.PlayerComplete] = new PlayerCompleteState(this),
            [BattleStateType.EnemyChoosePhase]= new EnemyChoosePhaseState(this),
            [BattleStateType.BattlePhase] = new BattlePhaseState(this),
            [BattleStateType.BattleEnd] = new BattleEndState(this),
            [BattleStateType.TurnEnd] = new TurnEndState(this),
            [BattleStateType.RoundEnd] = new RoundEndState(this)
        };
        
        state.ChangeState(stateGroup[BattleStateType.RoundStart]);
        
        //테스트용
        await UniTask.WaitUntil(() => playerCombat != null, 
            cancellationToken: this.GetCancellationTokenOnDestroy());
        DrawTest(playerCombat.Player);
    }

    private void Update()
    {
        state.Stay();
    }

    public void DrawTest(Character player)
    {
        handManager.CreateAllCard(player.CardList, player);
        handManager.ShuffleAndSettingHand();
        handManager.UpdateHandPos();
    }

    public HandManager GetHandManager()
    {
        return handManager;
    }

    public void RegisterPlayer(PlayerCombat player)
    {
        playerCombat = player;
    }

    public void RegisterEnemy(EnemyCombat enemy)
    {
        enemyCombat.Add(enemy);
    }
}
