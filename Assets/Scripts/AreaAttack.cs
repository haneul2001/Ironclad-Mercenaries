using UnityEngine;

public class AreaAttack : UnitAttack
{
    [Header("광역 전용")]
    public float explosionRadius = 1.5f;
    public GameObject explosionEffect;
    public float effectBaseRadius = 1f; // 이펙트 원본 Scale이 1f일 때 반경

    [Header("애니메이션")]
    public float attackAnimLength = 1f;   // Attack01 원래 길이

    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        // 애니메이션 속도를 공격 주기에 맞춤 (궁수랑 동일)
        if (anim != null && attackInterval > 0f)
        {
            anim.speed = attackAnimLength / attackInterval;
        }
    }
    protected override void PerformAttack(EnemyHealth target)
        {
            // 공격 애니메이션 재생
            if (anim != null)
            {
                anim.SetTrigger("Attack01");
            }

            // 폭발 중심 = 타겟 위치
            Vector3 explosionCenter = target.transform.position;

            // 폭발 이펙트 생성 (한 번만!)
            if (explosionEffect != null)
            {
                GameObject fx = Instantiate(explosionEffect, explosionCenter, Quaternion.identity);

                // 이펙트 크기를 폭발 반경에 맞춤
                float scaleFactor = explosionRadius / effectBaseRadius;
                fx.transform.localScale = Vector3.one * scaleFactor;

                Destroy(fx, 2f);   // 2초 후 제거
            }

            // 반경 안 적에게 데미지
            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            foreach (EnemyHealth enemy in enemies)
            {
                float distance = Vector3.Distance(explosionCenter, enemy.transform.position);
                if (distance <= explosionRadius)
                {
                    enemy.TakeDamage(attackDamage);
                }
            }
        }
        

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}