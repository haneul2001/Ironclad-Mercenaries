using UnityEngine;

public class RangedAttack : UnitAttack
{
    [Header("원거리 전용")]
    public GameObject projectilePrefab;  // 발사할 화살 프리팹
    public Transform firePoint;          // 화살이 나가는 위치(없으면 자기 위치)

    // 부모의 빈칸을 채움: 원거리는 화살 발사
    protected override void PerformAttack(EnemyHealth target)
    {
        // 발사 위치 결정
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

        // 화살 생성
        GameObject arrow = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // 화살에게 목표와 데미지를 알려줌
        Projectile proj = arrow.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Setup(target, attackDamage);
        }

        Debug.Log(gameObject.name + " (원거리) → 화살 발사!");
    }
}