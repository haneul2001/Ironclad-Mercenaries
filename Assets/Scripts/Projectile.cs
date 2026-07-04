using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 2f;   // 이 시간 지나면 자동 소멸
    public bool piercing = false;   // 관통 여부


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
        EnemyHealth enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy == null) return;

        enemy.TakeDamage(damage);

        if (!piercing)   // 관통 아니면 사라짐
            Destroy(gameObject);
        // 관통이면 안 사라지고 계속 감
    }
}