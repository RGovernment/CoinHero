using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : IDamageable, IBuffable
{
    public int Id { get; private set; }
    public int HP { get; private set; }
    public int MaxHP { get; private set; }
    public int SP { get; private set; }
    public List<Card> CardList;

    // 인게임에서만 사용
    [JsonIgnore]
    public List<StatusEffect> StatusEffectList;

    /// <summary>
    /// AP가 피해를 입을때 작동하는 함수
    /// <para>int : 입은 피해 </para>
    /// <para>Character : 피해를 입힌 캐릭터 객체</para>
    /// </summary>
    public event Action<int, Character> OnAPHit;
    
    /// <summary>
    /// HP가 피해를 입을때 작동하는 함수
    /// <para>int : 입은 피해 </para>
    /// <para>Character : 피해를 입힌 캐릭터 객체</para>
    /// </summary>
    public event Action<int, Character> OnHPHit;

    /// <summary>
    /// AP가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 아머 포인트</para>
    /// <para>int2 : 변경된 아머 포인트</para>
    /// </summary>
    public event Action<int, int> OnAPChanged;

    /// <summary>
    /// HP가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 체력</para>
    /// <para>int2 : 최대 체력</para>
    /// </summary>
    public event Action<int, int> OnHPChanged;

    /// <summary>
    /// HP가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 체력</para>
    /// <para>int2 : 변경된 체력</para>
    /// </summary>
    public event Action<int, int> OnMaxHPChanged;

    public Character(int id, int maxHp, List<Card> data)
    {
        Id = id;
        MaxHP = maxHp;
        HP = MaxHP;
        CardList = data;
        StatusEffectList = new();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="attacker"></param>
    public void TakeDamage(int damage, Character attacker)
    {
        // 피해 증가 디버프에 대한 계산
        int finalDamage = damage;
        foreach (var item in StatusEffectList)
        {
            finalDamage = item.OnModifyTakeDamage(finalDamage);
            
        }
        damage = finalDamage;

        if (SP > 0 && SP > damage)
        {
            int nowAP = SP;
            SP = Mathf.Max(0, SP - damage);

            OnAPHit(damage, attacker);
            OnAPChanged(nowAP, SP);
            return;
        }
        else if (SP <= damage)
        {
            SP = 0;
            OnAPHit(SP, attacker);
            OnAPChanged(SP, 0);

            damage -= SP;

        }

        int nowHP = HP;
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        OnHPHit?.Invoke(damage, attacker);
        OnHPChanged?.Invoke(HP, MaxHP);
    }

    public void SetMaxHP(int changeMaxHp)
    {
        MaxHP = changeMaxHp;
        OnHPChanged?.Invoke(HP, MaxHP);
    }

    public void TakeEffect(StatusEffect effect)
    {
        StatusEffectList.Add(effect);
    }

    public void RemoveEffect()
    {
        //
    }
}
