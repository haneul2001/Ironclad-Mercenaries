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

    // ── 이 화살이 거는 디버프 (궁수 강화 시 실려 옴) ──
    private float vulnerableOnHit = 0f;   // 취약 부여량
    private float slowOnHit = 0f;         // 둔화 부여량

    // 발사: 방향과 데미지 (기존 호출부 호환 유지 - 전사 검기 등)
    public void Setup(Vector3 fireDirection, int newDamage)
    {
        Setup(fireDirection, newDamage, 0f, 0f);
    }

    // 발사: 방향·데미지 + 디버프 수치 (궁수용)
    public void Setup(Vector3 fireDirection, int newDamage, float vulnerable, float slow)
    {
        direction = fireDirection.normalized;
        damage = newDamage;
        vulnerableOnHit = vulnerable;
        slowOnHit = slow;

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

        // 디버프 먼저 걸고(취약이 이번 타격부터 반영되도록), 그 다음 데미지
        if (vulnerableOnHit > 0f)
            enemy.AddVulnerable(vulnerableOnHit);

        if (slowOnHit > 0f)
        {
            EnemyMovement move = enemy.GetComponentInParent<EnemyMovement>();
            if (move == null) move = enemy.GetComponent<EnemyMovement>();
            if (move != null) move.AddSlow(slowOnHit);
        }

        enemy.TakeDamage(damage);

        if (!piercing)   // 관통 아니면 사라짐
            Destroy(gameObject);
        // 관통이면 안 사라지고 계속 감
    }
}