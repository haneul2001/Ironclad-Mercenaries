using UnityEngine;

// abstract = 이 클래스 자체로는 못 쓰고, 상속받아서만 사용
public abstract class UnitAttack : MonoBehaviour
{
    [Header("공통 공격 스탯")]
    public float attackRange = 3f;
    public int attackDamage = 10;
    public float attackInterval = 1f;

    private float attackTimer = 0f;

    void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            EnemyHealth target = FindNearestEnemy();
            if (target != null)
            {
                PerformAttack(target);  // ← 실제 공격은 자식이 정의
                attackTimer = 0f;
            }
        }
    }

    // 사거리 안 가장 가까운 적 찾기 (공통 로직)
    protected EnemyHealth FindNearestEnemy()
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
}