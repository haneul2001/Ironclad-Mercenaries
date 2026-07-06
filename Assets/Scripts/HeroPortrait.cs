using UnityEngine;
using UnityEngine.UI;

// 이 Image에 현재 주인공의 초상화를 표시.
public class HeroPortrait : MonoBehaviour
{
    public static HeroPortrait Instance;   // 체력바 밑 초상화용 (하나면 싱글톤으로 접근)

    public Image targetImage;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Apply();
    }

    void Start()
    {
        Apply();
    }

    // 외부(주인공 선택 시)에서 호출해 갱신
  public void Apply()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();

        if (targetImage == null)
        {
            Debug.Log("targetImage가 null! (Image 컴포넌트 못 찾음)");
            return;
        }
        if (UpgradeManager.Instance == null) return;

        UnitAttack hero = UpgradeManager.Instance.hero;
        if (hero != null && hero.portrait != null)
        {
            targetImage.sprite = hero.portrait;
            Debug.Log($"초상화 적용: {hero.portrait.name} → {targetImage.name}");
        }
        else
        {
            Debug.Log("hero 또는 portrait가 null");
        }
    }
}