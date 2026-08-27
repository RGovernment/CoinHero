using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
using SF = UnityEngine.SerializeField;

public class BattleManager : MonoBehaviour
{
    [SF] private HandManager handManager;
    private StateMachine state;
    private Dictionary<BattleStateType, IState> stateGroup;

    private Player player;

    private void Start()
    {
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
        List<Card> cd = new();
        foreach (var item in ResourceManager.Instance.CardData)
        {
            cd.Add(item.Value);
        }
        player = new (10, 50, cd);

        DrawTest(player);
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
}
