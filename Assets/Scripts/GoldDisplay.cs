using UnityEngine;
using TMPro;

// 주인공의 현재 골드를 우측 상단에 표시. 매 프레임 갱신(간단).
public class GoldDisplay : MonoBehaviour
{
    public TMP_Text goldText;

    void Update()
    {
        if (goldText == null || UpgradeManager.Instance == null) return;

        UnitAttack hero = UpgradeManager.Instance.hero;
        if (hero == null) return;

        int gold = UpgradeManager.Instance.GetUnitGold(hero);
        goldText.text = $"{gold}";
    }
}