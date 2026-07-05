using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;

    [Header("데미지 텍스트")]
    public GameObject damageTextPrefab;                  // 데미지 텍스트 프리팹
    public Vector3 textOffset = new Vector3(0, 2f, 0);   // 몬스터 위로 살짝

    // ── 취약 디버프 ──
    // 받는 데미지 배율. 1.0 = 100%(기본), 1.1 = 110%(취약 10%)
    private float damageTakenMultiplier = 1f;

     void Start()
    {
        // SetMaxHealth로 미리 설정되지 않았으면 프리팹 기본값으로 초기화
        if (currentHealth <= 0)
            currentHealth = maxHealth;
    }

     // 스폰 시 외부(스포너)에서 이 적의 최대 체력을 설정 (Day 난이도용)
    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = newMax;
    }
    // 취약 디버프 설정 (한 번만 걸림, 더 높은 값이 오면 갱신)
    public void AddVulnerable(float percent)
    {
        float newMultiplier = 1f + percent;
        if (newMultiplier > damageTakenMultiplier)
            damageTakenMultiplier = newMultiplier;
    }

    // 외부(유닛)에서 호출해 데미지를 입힘
    public void TakeDamage(int damage)
    {
        // 취약 배율 적용
        int finalDamage = Mathf.RoundToInt(damage * damageTakenMultiplier);

        currentHealth -= finalDamage;

        // 데미지 텍스트 표시 (실제 들어간 데미지로)
        ShowDamageText(finalDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null) return;

        // 몬스터 위치 + 오프셋에 텍스트 생성 (프리팹 회전 유지)
        Vector3 spawnPos = transform.position + textOffset;
        GameObject textObj = Instantiate(damageTextPrefab, spawnPos, damageTextPrefab.transform.rotation);

        // 데미지 값 설정
        DamageText dt = textObj.GetComponent<DamageText>();
        if (dt != null)
        {
            dt.SetDamage(damage);
        }
    }

    void Die()
    {
        // 적이 죽으면 제거
        Destroy(gameObject);
    }
}