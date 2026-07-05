using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 능력치 강화 패널. 주인공(hero)에게만 적용.
// 강화 버튼 2개(공격력/공속) + 현재 스탯 요약 표시.
//
// 화면 라벨 ↔ 코드 대응 (가독성 위해 화면은 기본/스킬1/스킬2로 표기):
//   화면 "스킬1" = 코드 어택2 (skill02Chance, GetDisplayDamage(2))
//   화면 "스킬2" = 코드 어택3 (skill03Chance, GetDisplayDamage(3))
//   화면 "공격력" = 코드 어택1 기본 (GetDisplayDamage(1))
public class BaseStatPanel : MonoBehaviour
{
    [Header("공격력 강화")]
    public Button attackButton;
    public TMP_Text attackButtonText;
    public UpgradeData attackData;

    [Header("공속 강화")]
    public Button speedButton;
    public TMP_Text speedButtonText;
    public UpgradeData speedData;

    [Header("현재 스탯 표시 (선택)")]
    public TMP_Text skill2ChanceText;   // 화면 "스킬1 확률" = 어택2 확률
    public TMP_Text skill3ChanceText;   // 화면 "스킬2 확률" = 어택3 확률
    public TMP_Text skill1DmgText;      // 화면 "공격력" = 어택1 기본
    public TMP_Text skill2DmgText;      // 화면 "스킬1 공격력" = 어택2
    public TMP_Text skill3DmgText;      // 화면 "스킬2 공격력" = 어택3
    public TMP_Text cooldownText;       // 공격 쿨타임

    void OnEnable()
    {
        Refresh();
    }

    void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(OnClickAttack);
        if (speedButton != null)
            speedButton.onClick.AddListener(OnClickSpeed);

        Refresh();
    }

    private UnitAttack Hero()
    {
        return UpgradeManager.Instance != null ? UpgradeManager.Instance.hero : null;
    }

    void OnClickAttack() { Buy(attackData); }
    void OnClickSpeed() { Buy(speedData); }

    private void Buy(UpgradeData data)
    {
        UnitAttack hero = Hero();
        if (hero == null || data == null) return;

        UpgradeManager.Instance.TryPurchaseBaseStat(hero, data);
        Refresh();   // 강화 후 버튼·스탯 다 갱신
    }

    public void Refresh()
    {
        UnitAttack hero = Hero();
        if (hero == null) return;

        // 버튼 갱신
        UpdateButton(attackButton, attackButtonText, attackData, hero, "기본 공격력 강화");
        UpdateButton(speedButton, speedButtonText, speedData, hero, "공격 쿨타임 감소");

        // 스탯 요약 갱신 (화면 라벨 기준)
        if (skill2ChanceText != null)   // 화면 "스킬1 확률" = 어택2 확률
            skill2ChanceText.text = $"{hero.skill02Chance * 100f:F0}%";
        if (skill3ChanceText != null)   // 화면 "스킬2 확률" = 어택3 확률
            skill3ChanceText.text = $"{hero.skill03Chance * 100f:F0}%";

        if (skill1DmgText != null)      // 화면 "공격력" = 어택1 기본
            skill1DmgText.text = $"{hero.GetDisplayDamage(1)}";
        if (skill2DmgText != null)      // 화면 "스킬1 공격력" = 어택2
            skill2DmgText.text = $"{hero.GetDisplayDamage(2)}";
        if (skill3DmgText != null)      // 화면 "스킬2 공격력" = 어택3
            skill3DmgText.text = $"{hero.GetDisplayDamage(3)}";

        if (cooldownText != null)
            cooldownText.text = $"{hero.attackInterval:F2}초";
    }

    private void UpdateButton(Button btn, TMP_Text txt, UpgradeData data, UnitAttack hero, string label)
    {
        if (data == null || txt == null) return;

        int lv = hero.GetUpgradeLevel(data.type);

        if (data.maxLevel > 0 && lv >= data.maxLevel)
        {
            txt.text = $"{label}\nMAX";
            if (btn != null) btn.interactable = false;
            return;
        }

        int cost = data.GetCost(lv);
        txt.text = $"{label}\nLv.{lv} / {cost}원";

        if (btn != null)
        {
            int gold = UpgradeManager.Instance.GetUnitGold(hero);
            btn.interactable = (gold >= cost);
        }
    }
}