using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;   // 이 시간 지나면 자동 소멸

    [Header("회전 보정")]
    public Vector3 rotationOffset = Vector3.zero;

    private Vector3 direction;   // 날아갈 방향 (발사 시 고정)
    private int damage;

    // 발사: 방향과 데미지를 받음
    public void Setup(Vector3 fireDirection, int newDamage)
    {
        direction = fireDirection.normalized;
        damage = newDamage;

        // 발사 방향을 바라보게 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
        }

        // 수명 후 자동 소멸
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 방향으로 직진
        transform.position += direction * speed * Time.deltaTime;
    }

    // 적과 충돌 시 데미지
   void OnTriggerEnter(Collider other)
    {
        Debug.Log("화살이 닿음: " + other.name);   // ← 뭐에 닿는지

        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null)
        {
            Debug.Log("적 발견! 데미지: " + enemy.name);   // ← 적 찾았는지
            enemy.TakeDamage(damage);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("EnemyHealth 못 찾음: " + other.name);   // ← 못 찾음
        }
    }
}