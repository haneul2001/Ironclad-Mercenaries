using UnityEngine;

// UnitAttack을 상속받음
public class MeleeAttack : UnitAttack
{
    [Header("근접 광역 (사각 범위)")]
    public float rangeX = 2f;                   // 좌우 폭
    public float rangeZ = 3f;                   // 앞뒤 길이
    public GameObject slashEffect;              // 검기 이펙트 (선택)
    public float effectBaseScale = 1f;          // 이펙트 원본 크기 배율 (조정용)

    [Header("애니메이션")]
    public float attackAnimLength = 1f;         // Attack01 원래 길이

    private Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();

        // 애니메이션 속도를 공격 주기에 맞춤 (궁수·마법사와 동일)
        if (anim != null && attackInterval > 0f)
        {
            anim.speed = attackAnimLength / attackInterval;
        }
    }
    protected override EnemyHealth FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float minDistance = float.MaxValue;

        foreach (EnemyHealth enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - transform.position;
            // 사각 범위 안에 있는 적만 대상
            if (Mathf.Abs(diff.x) <= rangeX / 2f && Mathf.Abs(diff.z) <= rangeZ / 2f)
            {
                float distance = diff.magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }
        }
        return nearest;
    }
    // 부모의 빈칸을 채움: 근접 사각 범위 광역 데미지
    protected override void PerformAttack(EnemyHealth target)
    {
        // 공격 애니메이션 재생
        if (anim != null)
        {
            anim.SetTrigger("Attack01");
        }

        // 검기 이펙트 생성 (있을 때만)
      if (slashEffect != null)
        {
            // Y축으로 180도 돌려서 생성 (반대 방향 고치기)
            GameObject fx = Instantiate(slashEffect, transform.position, Quaternion.Euler(0, 180, 0));
            fx.transform.localScale = Vector3.one * effectBaseScale;
            Destroy(fx, 2f);
        }

        // 사각 범위 안의 모든 적에게 데미지
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            Vector3 diff = enemy.transform.position - transform.position;
            // 좌우(X)와 앞뒤(Z)가 각각 범위 안이면 사각형 안
            if (Mathf.Abs(diff.x) <= rangeX / 2f && Mathf.Abs(diff.z) <= rangeZ / 2f)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    // 사각 타격 범위를 Scene 뷰에 표시 (개발용)
    protected override void OnDrawGizmosSelected()
    {
        // base.OnDrawGizmosSelected();   // 부모 사거리 원(청록)
        Gizmos.color = Color.yellow;
        // 사각 범위 (높이 1은 표시용)
        Gizmos.DrawWireCube(transform.position, new Vector3(rangeX, 1f, rangeZ));
    }
}