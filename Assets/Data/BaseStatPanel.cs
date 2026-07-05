using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 능력치 강화 패널. 주인공(hero)에게만 적용.
// 강화 버튼 2개(공격력/공속) + 현재 스탯 요약 표시.
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
    public TMP_Text skill2ChanceText;   // 스킬2 확률
    public TMP_Text skill3ChanceText;   // 스킬3 확률
    public TMP_Text skill1DmgText;      // 스킬1 공격력
    public TMP_Text skill2DmgText;      // 스킬2 공격력
    public TMP_Text skill3DmgText;      // 스킬3 공격력

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
        UpdateButton(attackButton, attackButtonText, attackData, hero, "공격력");
        UpdateButton(speedButton, speedButtonText, speedData, hero, "쿨타임 감소");

        // 스탯 요약 갱신
        if (skill2ChanceText != null)
            skill2ChanceText.text = $"스킬2 확률: {hero.skill02Chance * 100f:F0}%";
        if (skill3ChanceText != null)
            skill3ChanceText.text = $"스킬3 확률: {hero.skill03Chance * 100f:F0}%";
        if (skill1DmgText != null)
            skill1DmgText.text = $"스킬1 공격력: {hero.GetDisplayDamage(1)}";
        if (skill2DmgText != null)
            skill2DmgText.text = $"스킬2 공격력: {hero.GetDisplayDamage(2)}";
        if (skill3DmgText != null)
            skill3DmgText.text = $"스킬3 공격력: {hero.GetDisplayDamage(3)}";
    }

    private void UpdateButton(Button btn, TMP_Text txt, UpgradeData data, UnitAttack hero, string label)
    {
        if (data == null || txt == null) return;

        int lv = hero.GetUpgradeLevel(data.type);

        if (data.maxLevel > 0 && lv >= data.maxLevel)
        {
            txt.text = $"{label} MAX";
            if (btn != null) btn.interactable = false;
            return;
        }

        int cost = data.GetCost(lv);
        txt.text = $"{label} Lv.{lv}\n{cost}원";

        if (btn != null)
        {
            int gold = UpgradeManager.Instance.GetUnitGold(hero);
            btn.interactable = (gold >= cost);
        }
    }
}