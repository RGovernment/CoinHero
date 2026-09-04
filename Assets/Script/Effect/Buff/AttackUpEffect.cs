public class AttackUpEffect : StatusEffect
{
    public AttackUpEffect(StatusEffectData data, int value, int duration) : base(data, value, duration)
    {

    }

    public override int OnModifyAttackValue(int baseDamage)
    {
        return baseDamage + Value;
    }
}
