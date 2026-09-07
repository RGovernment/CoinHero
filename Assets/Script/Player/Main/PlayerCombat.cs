using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : CombatBase
{
    public override void Init(Character chara)
    {
        Character = chara;
        hitMat = new();
        CoinUI.gameObject.SetActive(false);
        animatorManager.Combat = this;
        statUI.combat = this;
        statUI.Init(Character.HP, Character.SP, Character.Sanity);
        renders = animator.transform.GetComponentsInChildren<SpriteRenderer>();
        
        BattleManager.Instance.RegisterPlayer(this);
    }
}
