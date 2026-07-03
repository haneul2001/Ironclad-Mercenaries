using UnityEngine;
using UnityEngine.UI;   // Image(체력바) 쓰려면 필요

public class CoreHealth : MonoBehaviour
{
    [Header("체력")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI")]
    public Image healthBarFill;   // 체력바 채움 이미지 (Inspector에서 연결)

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();   // 시작 시 꽉 찬 상태로
    }

    // 외부(적)에서 호출해 데미지를 입힘
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("성벽 체력: " + currentHealth);

        UpdateHealthBar();   // 데미지 받을 때마다 바 갱신

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateHealthBar();
            GameManager.Instance.OnCoreDestroyed();   // 패배 알림
        }
    }

    // 체력바를 현재 체력 비율(0~1)로 갱신
    void UpdateHealthBar()
    {
        if (healthBarFill != null)
        {
            // (float) 꼭 붙이기! 안 그러면 정수 나눗셈이라 0 또는 1만 나옴
            healthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
    }
}