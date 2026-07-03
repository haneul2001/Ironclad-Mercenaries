using UnityEngine;

public class AreaAttack : UnitAttack
{
    [Header("광역 전용")]
    public float explosionRadius = 1.5f;   // 폭발 반경 (설정 가능)
    public GameObject explosionEffect;     // 폭발 이펙트(선택, 없으면 비워둠)

    // 부모가 찾아준 target(가장 가까운 적) 위치에 광역기를 터뜨림
    protected override void PerformAttack(EnemyHealth target)
    {
        // 타겟의 위치 = 광역기가 터지는 중심점
        Vector3 explosionCenter = target.transform.position;

        // (선택) 폭발 이펙트를 그 위치에 생성
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, explosionCenter, Quaternion.identity);
        }

        // 폭발 중심 반경 안의 모든 적에게 데미지
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        foreach (EnemyHealth enemy in enemies)
        {
            float distance = Vector3.Distance(explosionCenter, enemy.transform.position);
            if (distance <= explosionRadius)
            {
                enemy.TakeDamage(attackDamage);
            }
        }

        Debug.Log(gameObject.name + " (마법사) → " + explosionCenter + " 지점에 광역기!");
    }

    // 폭발 반경을 Scene 뷰에 표시 (개발용)
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();  // ← 부모 기즈모(사거리 청록 원)를 먼저 그림
        // 그 위에 폭발 반경을 빨강으로 덧그림
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}