using UnityEngine;
using UnityEngine.UI;

// 보스 전용 체력바. 보스 스폰 시 켜지고 보스 체력을 추적해 표시.
public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance;

    public GameObject barRoot;      // 체력바 UI 전체 (켜고 끔)
    public Image fillImage;         // 채움 바 (fillAmount 조절)

    private EnemyHealth target;

    void Awake()
    {
        Instance = this;
        if (barRoot != null) barRoot.SetActive(false);   // 평소엔 숨김
    }

    // 보스 스폰 시 호출 → 체력바 켜고 이 보스 추적
    public void ShowBoss(EnemyHealth boss)
    {
        target = boss;
        if (barRoot != null) barRoot.SetActive(true);
        UpdateBar();
    }

    void Update()
    {
        if (target == null) return;

        // 보스가 죽었거나 사라졌으면 숨김
        if (target == null || target.IsDead)
        {
            Hide();
            return;
        }

        UpdateBar();
    }

    private void UpdateBar()
    {
        if (fillImage != null && target != null)
            fillImage.fillAmount = target.HealthRatio;
    }

    public void Hide()
    {
        target = null;
        if (barRoot != null) barRoot.SetActive(false);
    }
}