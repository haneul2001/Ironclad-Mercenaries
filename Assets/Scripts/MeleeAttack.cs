using UnityEngine;

// UnitAttack을 상속받음
public class MeleeAttack : UnitAttack
{
    // 부모의 빈칸을 채움: 근접은 즉시 데미지
    protected override void PerformAttack(EnemyHealth target)
    {
        target.TakeDamage(attackDamage);
        Debug.Log(gameObject.name + " (근접) → 즉시 타격!");
    }
}