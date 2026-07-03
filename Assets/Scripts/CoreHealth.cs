using UnityEngine;

public class CoreHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // 외부(적)에서 호출해 데미지를 입힘
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("성벽 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            GameManager.Instance.OnCoreDestoryed();
        }
    }
}