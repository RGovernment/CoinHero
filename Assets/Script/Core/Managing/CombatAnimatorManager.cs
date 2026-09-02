using Cysharp.Threading.Tasks;
using UnityEngine;
using SF = UnityEngine.SerializeField;

public class CombatAnimatorManager : MonoBehaviour
{
    private static readonly int _7CustomHash = Animator.StringToHash("7_Custom");
    private static readonly int _6OtherHash = Animator.StringToHash("6_Other");
    private static readonly int _5DebuffHash = Animator.StringToHash("5_Debuff");
    private static readonly int _4DeathHash = Animator.StringToHash("4_Death");
    private static readonly int _3DamageHash = Animator.StringToHash("3_Damaged");
    private static readonly int _2AttackHash = Animator.StringToHash("2_Attack");
    private static readonly int _1MoveHash = Animator.StringToHash("1_Move");

    public ICombat Combat;
    private Animator animator;

    public UniTaskCompletionSource animationTriggerAttacker;
    public UniTaskCompletionSource animationTriggerDefender;
    public UniTaskCompletionSource animationTriggerItem;
    private UniTaskCompletionSource deadTrigger;

    public void Start()
    {
        animator = Combat.Animator;
    }

    public void OnMove()
    {
        animator.SetBool(_1MoveHash, true);
    }

    public void OnIdle()
    {
        animator.SetBool(_1MoveHash, false);
        animator.SetBool(_5DebuffHash, false);
    }

    public void OnAttack()
    {
        animator.SetTrigger(_2AttackHash);
    }

    public void OnDamage()
    {
        animator.SetTrigger(_3DamageHash);
    }

    public async UniTask OnDead()
    {
        deadTrigger = new();
        animator.SetBool(_4DeathHash, true);
        await deadTrigger.Task;
    }

    public void OnOther()
    {
        animator.SetTrigger(_6OtherHash);
    }

    public void OnCustom()
    {
        animator.SetTrigger(_7CustomHash);
    }

    public void OnAttacker()
    {
        animationTriggerAttacker?.TrySetResult();
    }

    public void OnDefender()
    {
        animationTriggerDefender?.TrySetResult();
    }

    public void OnItem()
    {
        animationTriggerItem?.TrySetResult();
    }

    public void OnDeadEnd()
    {
        deadTrigger.TrySetResult();
    }
}
