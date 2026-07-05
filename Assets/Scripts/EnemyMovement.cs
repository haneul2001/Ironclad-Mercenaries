using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 2f;

    [Header("공격")]
    public int attackDamage = 10;      // 이 적의 공격력
    public float attackInterval = 1f;  // 이 적의 공격 간격(초)

    private bool isAttacking = false;

    // ── 둔화 디버프 ──
    // 이동속도 배율. 1.0 = 100%, 0.9 = 90%(10% 감소). 최소 0.2까지만.
    private float slowMultiplier = 1f;

    void Update()
    {
        if (!isAttacking)
        {
            transform.position += Vector3.back * moveSpeed * slowMultiplier * Time.deltaTime;
        }
    }

    // 둔화 디버프 설정 (한 번만 걸림, 더 강한 둔화가 오면 갱신)
    public void AddSlow(float percent)
    {
        float newMultiplier = 1f - percent;
        newMultiplier = Mathf.Max(0.2f, newMultiplier);   // 완전 정지 방지
        if (newMultiplier < slowMultiplier)                // 더 느린(작은) 값이면 갱신
            slowMultiplier = newMultiplier;
    }

    // 성벽에 닿으면 호출됨: 멈추고 공격 시작
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("뭔가 닿음: " + other.name);
        if (other.CompareTag("Core"))
        {
            CoreHealth core = other.GetComponent<CoreHealth>();
            if (core != null)
            {
                isAttacking = true;
                // 성벽을 향해 주기적으로 공격
                InvokeRepeating(nameof(AttackCore), 0f, attackInterval);
                // 어떤 성벽을 때릴지 기억해둠
                targetCore = core;
            }
        }
    }

    private CoreHealth targetCore;

    // 주기적으로 성벽을 공격
    void AttackCore()
    {
        if (targetCore != null)
        {
            targetCore.TakeDamage(attackDamage);
        }
    }
}