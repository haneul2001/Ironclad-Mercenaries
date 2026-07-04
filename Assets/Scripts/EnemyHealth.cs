using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;

    [Header("데미지 텍스트")]
    public GameObject damageTextPrefab;                  // 데미지 텍스트 프리팹
    public Vector3 textOffset = new Vector3(0, 2f, 0);   // 몬스터 위로 살짝

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 외부(유닛)에서 호출해 데미지를 입힘
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // 데미지 텍스트 표시
        ShowDamageText(damage);

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