using UnityEngine;

public class RangedAttack : UnitAttack
{
    [Header("원거리 전용")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("애니메이션")]
    public float attackAnimLength = 1f;   // Attack01 원래 길이 (초)

    private Animator anim;
    private EnemyHealth currentTarget;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        // 애니메이션 속도를 공격 주기(attackInterval)에 맞춤
        if (anim != null && attackInterval > 0f)
        {
            anim.speed = attackAnimLength / attackInterval;
        }
    }

    protected override void PerformAttack(EnemyHealth target)
    {
        currentTarget = target;
        if (anim != null)
        {
            anim.SetTrigger("Attack01");
        }
    }

    public void FireArrow()
    {
        if (currentTarget == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject arrow = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Projectile proj = arrow.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Setup(currentTarget, attackDamage);
        }
    }
    protected override void OnAttackStatsChanged()
{
    // 공속 바뀌면 애니메이션 속도도 다시 계산
    if (anim != null && attackInterval > 0f)
    {
        anim.speed = attackAnimLength / attackInterval;
    }
}
}