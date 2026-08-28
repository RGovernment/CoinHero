using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Player player { get; set; }

    private void Awake()
    {

    }

    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
    }
}
