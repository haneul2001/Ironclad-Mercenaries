using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 특성 강화 패널. 열릴 때 3종을 받아 슬롯에 표시, 슬롯 클릭 시 적용 후 닫기.
public class TraitPanel : MonoBehaviour
{
    [Header("슬롯 (3개, 순서 맞추기)")]
    public Button[] slotButtons;      // 슬롯 버튼 3개
    public Image[] slotIcons;         // 각 슬롯의 아이콘 이미지 3개
    public TMP_Text[] slotNames;      // 각 슬롯의 이름 텍스트 3개
    public TMP_Text[] slotDescs;      // 각 슬롯의 설명 텍스트 3개

    [Header("연결")]
    public UpgradeMenuController menu;   // 닫기 위해 참조

    // 이번에 제시된 3종 (슬롯 순서와 일치)
    private List<UpgradeData> currentChoices;

    void Start()
    {
        // 각 슬롯 버튼에 클릭 리스너 연결 (인덱스 기억)
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int idx = i;   // 클로저 캡처 방지용 지역 변수
            if (slotButtons[i] != null)
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(idx));
        }
    }

    // 특성 진입 성공 시, 뽑힌 3종을 받아 슬롯에 그림
    public void Show(List<UpgradeData> choices)
    {
        currentChoices = choices;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (i < choices.Count && choices[i] != null)
            {
                UpgradeData d = choices[i];

                if (slotIcons[i] != null)
                {
                    slotIcons[i].sprite = d.icon;
                    slotIcons[i].enabled = (d.icon != null);
                }
                if (slotNames[i] != null)
                    slotNames[i].text = d.displayName;
                if (slotDescs[i] != null)
                    slotDescs[i].text = d.description;

                if (slotButtons[i] != null)
                    slotButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // 뽑힌 게 3개 미만이면 남는 슬롯 숨김
                if (slotButtons[i] != null)
                    slotButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // 슬롯 클릭 → 해당 특성 적용 후 패널 닫기
    private void OnSlotClicked(int idx)
    {
        if (currentChoices == null || idx >= currentChoices.Count) return;

        UnitAttack hero = UpgradeManager.Instance != null ? UpgradeManager.Instance.hero : null;
        if (hero == null) return;

        UpgradeManager.Instance.SelectTrait(hero, currentChoices[idx]);

        // 선택 끝났으니 패널 닫기 (선택창으로 복귀)
        if (menu != null)
            menu.CloseCurrentPanel();
    }
}