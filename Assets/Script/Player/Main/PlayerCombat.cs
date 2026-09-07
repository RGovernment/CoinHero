using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CombatBase
{

    private void Awake()
    {
        hitMat = new();

        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
        statUI.combat = this;
        statUI.Init(Character.HP, Character.SP, Character.Sanity);
        renders = animator.transform.GetComponentsInChildren<SpriteRenderer>();
    }
    private void Start()
    {
        BattleManager.Instance.RegisterPlayer(this);
    }
}
