using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;

    [Header("회전 보정")]
    public Vector3 rotationOffset = Vector3.zero;   // 화살 모델 방향 보정 (필요시)

    private EnemyHealth target;
    private int damage;

    public void Setup(EnemyHealth newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 목표 방향 계산
        Vector3 direction = (target.transform.position - transform.position).normalized;

        // ★ 화살이 목표 방향을 바라보게 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
        }

        // 목표를 향해 이동
        transform.position += direction * speed * Time.deltaTime;

        // 명중 처리
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 0.3f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}