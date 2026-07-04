using UnityEngine;

// abstract = 이 클래스 자체로는 못 쓰고, 상속받아서만 사용
public abstract class UnitAttack : MonoBehaviour
{
    [Header("공통 공격 스탯")]
    public float attackRange = 3f;
    public int attackDamage = 10;
    public float attackInterval = 1f;

    private float attackTimer = 0f;


    [Header("빌드 스킬 (해금/확률)")]
    public bool skill02Unlocked = false;   // 2단계 스킬 해금 여부
    public bool skill03Unlocked = false;   // 3단계 스킬 해금 여부
    public float skill02Chance = 0.3f;     // 2단계 발동 확률 (30%)
    public float skill03Chance = 0.1f;     // 3단계 발동 확률 (10%)

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            EnemyHealth target = FindNearestEnemy();
            if (target != null)
            {
                PerformAttack(target);
                attackTimer = 0f;
            }
        }
    }

    // 사거리 안 가장 가까운 적 찾기 (공통 로직)
    protected virtual EnemyHealth FindNearestEnemy()
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        EnemyHealth nearest = null;
        float minDistance = attackRange;

        foreach (EnemyHealth enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        return nearest;
    }

    // 자식이 반드시 구현해야 하는 "공격 방법"
    protected abstract void PerformAttack(EnemyHealth target);

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    // 공격 주기를 바꾸고, 관련 처리(애니메이션 속도 등)를 갱신
    public virtual void SetAttackInterval(float newInterval)
    {
        attackInterval = newInterval;
        OnAttackStatsChanged();
    }

    protected virtual void OnAttackStatsChanged() { }

    // 확률로 공격 단계 결정 (강한 스킬부터 독립 체크)
    protected int RollAttackLevel()
    {
        if (skill03Unlocked && Random.value < skill03Chance)
            return 3;
        if (skill02Unlocked && Random.value < skill02Chance)
            return 2;
        return 1;
    }
}