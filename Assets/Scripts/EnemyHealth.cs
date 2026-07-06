using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;
    private bool healthInitialized = false;

    [Header("데미지 텍스트")]
    public GameObject damageTextPrefab;
    public Vector3 textOffset = new Vector3(0, 2f, 0);

    private float damageTakenMultiplier = 1f;

    void Start()
    {
        if (!healthInitialized)
            currentHealth = maxHealth;
    }

    public void SetMaxHealth(int newMax)
    {
        maxHealth = newMax;
        currentHealth = newMax;
        healthInitialized = true;
    }

    public float HealthRatio
    {
        get { return maxHealth > 0 ? (float)currentHealth / maxHealth : 0f; }
    }

    public bool IsDead
    {
        get { return currentHealth <= 0; }
    }

    public void AddVulnerable(float percent)
    {
        float newMultiplier = 1f + percent;
        if (newMultiplier > damageTakenMultiplier)
            damageTakenMultiplier = newMultiplier;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.RoundToInt(damage * damageTakenMultiplier);
        currentHealth -= finalDamage;

        ShowDamageText(finalDamage);

        if (currentHealth <= 0)
            Die();
    }

    private void ShowDamageText(int damage)
    {
        if (damageTextPrefab == null) return;

        Vector3 spawnPos = transform.position + textOffset;
        GameObject textObj = Instantiate(damageTextPrefab, spawnPos, damageTextPrefab.transform.rotation);

        DamageText dt = textObj.GetComponent<DamageText>();
        if (dt != null)
            dt.SetDamage(damage);
    }

    void Die()
    {
        Destroy(gameObject);
    }
}