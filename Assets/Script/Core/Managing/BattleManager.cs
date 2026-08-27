using System.Collections.Generic;
using UnityEngine;
using static Enums;

public class BattleManager : MonoBehaviour
{
    private StateMachine state;
    private Dictionary<BattleStateType, IState> stateGroup;

    void Start()
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
    }

    // Update is called once per frame
    void Update()
    {
        state.Stay();
    }
}
