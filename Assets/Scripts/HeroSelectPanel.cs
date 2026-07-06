using UnityEngine;
using UnityEngine.UI;

// 게임 시작 시 주인공(플레이어가 강화할 유닛)을 고르는 창.
// 세 버튼(궁수/전사/마법사) 중 하나 누르면 hero 지정 후 전투 시작.
public class HeroSelectPanel : MonoBehaviour
{
    [Header("직업별 선택 버튼")]
    public Button archerButton;
    public Button warriorButton;
    public Button mageButton;

    void Start()
    {
        if (archerButton != null)
            archerButton.onClick.AddListener(() => Choose(UpgradeManager.Instance.archer));
        if (warriorButton != null)
            warriorButton.onClick.AddListener(() => Choose(UpgradeManager.Instance.warrior));
        if (mageButton != null)
            mageButton.onClick.AddListener(() => Choose(UpgradeManager.Instance.mage));
    }

    private void Choose(UnitAttack chosen)
    {
        if (UpgradeManager.Instance == null || chosen == null) return;

        // 주인공 지정
        UpgradeManager.Instance.hero = chosen;
        Debug.Log($"주인공 선택: {chosen.name}");

        // 체력바 밑 초상화 갱신
        if (HeroPortrait.Instance != null)
            HeroPortrait.Instance.Apply();

        // 전투 시작
        if (GameManager.Instance != null)
            GameManager.Instance.StartFirstDay();
    }
}