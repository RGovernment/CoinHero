using UnityEngine;

public class AttackDownEffect : StatusEffect
{
    public AttackDownEffect(StatusEffectData data, int value, int duration) : base(data, value, duration)
    {
    }

    public override int OnModifyAttackValue(int baseDamage)
    {
        return baseDamage - Value;
    }
}
