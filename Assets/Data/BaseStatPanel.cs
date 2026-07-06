using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 능력치 강화 패널. 주인공(hero)에게만 적용.
// 강화 버튼 2개(공격력/공속) + 현재 스탯 요약(한 텍스트) + 주인공 초상화.
//
// 명칭: 어택1 = 기본 공격, 어택2 = 스킬1, 어택3 = 스킬2
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

    [Header("초상화")]
    public Image portraitImage;

    [Header("스탯 표시 (하나로)")]
    public TMP_Text statText;   // 모든 스탯을 여기 한 번에 표시

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
        Refresh();
    }

    public void Refresh()
    {
        UnitAttack hero = Hero();
        if (hero == null) return;

        // 초상화 갱신
        if (portraitImage != null && hero.portrait != null)
            portraitImage.sprite = hero.portrait;

        // 버튼 갱신
        UpdateButton(attackButton, attackButtonText, attackData, hero, "기본 공격력 강화");
        UpdateButton(speedButton, speedButtonText, speedData, hero, "공격 쿨타임 감소");

        // 스탯 요약 (한 텍스트로)
        if (statText != null)
        {
            statText.text =
                $"기본 공격력\t{hero.GetDisplayDamage(1)}\n" +
                $"스킬1 공격력\t{hero.GetDisplayDamage(2)}\n" +
                $"스킬1 확률\t{hero.skill02Chance * 100f:F0}%\n" +
                $"스킬2 공격력\t{hero.GetDisplayDamage(3)}\n" +
                $"스킬2 확률\t{hero.skill03Chance * 100f:F0}%\n" +
                $"공격 쿨타임\t{hero.attackInterval:F2}초";
        }
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