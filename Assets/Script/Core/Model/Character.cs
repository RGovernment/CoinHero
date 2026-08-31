using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public abstract class Character : IDamageable, IBuffable
{
    public int Id { get; private set; }
    public int HP { get; private set; }
    public int MaxHP { get; private set; }
    public int SP { get; private set; }
    public int Sanity { get; private set; }

    public bool IsDead { get => HP <= 0; }
    public List<Card> CardList;

    // 인게임에서만 사용
    [JsonIgnore]
    public List<StatusEffect> StatusEffectList;

    /// <summary>
    /// SP가 피해를 입을때 작동하는 함수
    /// <para>int : 입은 피해 </para>
    /// <para>Character : 피해를 입힌 캐릭터 객체</para>
    /// </summary>
    public event Action<int, Character> OnSPHit;
    
    /// <summary>
    /// HP가 피해를 입을때 작동하는 함수
    /// <para>int : 입은 피해 </para>
    /// <para>Character : 피해를 입힌 캐릭터 객체</para>
    /// </summary>
    public event Action<int, Character> OnHPHit;

    /// <summary>
    /// SP가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 실드 포인트</para>
    /// <para>int2 : 변경된 실드 포인트</para>
    /// </summary>
    public event Action<int, int> OnSPChanged;

    /// <summary>
    /// HP가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 체력</para>
    /// <para>int2 : 변경된 체력</para>
    /// </summary>
    public event Action<int, int> OnHPChanged;

    /// <summary>
    /// MaxHP가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 체력</para>
    /// <para>int2 : 변경된 체력</para>
    /// </summary>
    public event Action<int, int> OnMaxHPChanged;

    /// <summary>
    /// Sanity가 변경될 때 작동하는 함수
    /// <para>int1 : 현재 정신력</para>
    /// <para>int2 : 변경된 정신력</para>
    /// </summary>
    public event Action<int, int> OnSanityChanged;

    /// <summary>
    /// 캐릭터가 사망할 때 작동하는 함수
    /// <para> Character : 사망한 캐릭터 </para>
    /// </summary>
    public event Action<Character> OnDead;

    public Character(int id, int maxHp, List<Card> data)
    {
        Id = id;
        MaxHP = maxHp;
        HP = MaxHP;
        CardList = data;
        Sanity = 50;
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

            OnSPHit?.Invoke(damage, attacker);
            OnSPChanged?.Invoke(nowAP, SP);
            return;
        }
        else if (SP <= damage)
        {
            SP = 0;
            OnSPHit?.Invoke(SP, attacker);
            OnSPChanged?.Invoke(SP, 0);

            damage -= SP;

        }

        int nowHP = HP;
        HP = Mathf.Clamp(HP - damage, 0, MaxHP);
        OnHPHit?.Invoke(damage, attacker);
        OnHPChanged?.Invoke(nowHP, HP);
        if(IsDead)
            OnDead?.Invoke(this);
    }

    /// <summary>
    /// 상대가 방어에 성공했을 경우 반동 데미지를 받는 함수
    /// </summary>
    public void TakeRebound(int AP)
    {
        int reboundDamage = 0;

        if (AP < 10)
            reboundDamage = REBOUND_SANITY_COST;
        else
            reboundDamage = REBOUND_SANITY_COST + (AP / 10);
        
        int nowSanity = Sanity;

        Sanity = Mathf.Clamp(Sanity - reboundDamage, MIN_SANITY, MAX_SANITY);

        OnSanityChanged?.Invoke(nowSanity, Sanity);
    }

    public void TakeShieldPoint(int getSP)
    {
        int nowSP = SP;
        SP += getSP;
        
        OnSPChanged?.Invoke(nowSP, SP);
    }

    public void SetMaxHP(int changeMaxHp)
    {
        MaxHP = changeMaxHp;
        OnMaxHPChanged?.Invoke(MaxHP, changeMaxHp);
    }

    public void SetSanity(int changeSanity)
    {
        Sanity = Mathf.Clamp(changeSanity, MIN_SANITY, MAX_SANITY);
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
