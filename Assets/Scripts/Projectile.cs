using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;

    private EnemyHealth target;  // 날아갈 목표
    private int damage;

    // 발사할 때 목표와 데미지를 전달받음
    public void Setup(EnemyHealth newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    void Update()
    {
        // 목표가 이미 죽어 사라졌으면 화살도 제거
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 목표를 향해 이동
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 목표에 충분히 가까워지면 명중 처리
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 0.3f)
        {
            target.TakeDamage(damage);
            Destroy(gameObject);  // 명중 후 화살 제거
        }
    }
}