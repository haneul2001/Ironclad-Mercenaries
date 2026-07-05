using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 정비소 안에서 "선택 메뉴 ↔ 각 강화 패널" 슬라이드 전환을 담당.
public class UpgradeMenuController : MonoBehaviour
{
    [Header("선택 메뉴 (특성/능력치/도박 묶음)")]
    public SlideInPanel selectMenu;

    [Header("강화 패널들")]
    public SlideInPanel traitPanel;
    public SlideInPanel statPanel;
    public SlideInPanel gamblePanel;

    [Header("특성 패널 스크립트")]
    public TraitPanel traitPanelScript;   // 특성 패널의 TraitPanel 컴포넌트

    [Header("전환 간격 (슬라이드 시간과 맞추기)")]
    public float transitionGap = 1f;

    private SlideInPanel currentPanel;

    // ── 선택창 버튼들이 호출 ──

    public void OpenTrait()
    {
        UnitAttack hero = UpgradeManager.Instance != null ? UpgradeManager.Instance.hero : null;
        if (hero == null) return;

        // 진입료 20원 차감 + 3종 뽑기
        List<UpgradeData> choices = UpgradeManager.Instance.EnterTraitUpgrade(hero);

        // 돈 부족 등으로 실패하면 아무것도 안 함
        if (choices == null || choices.Count == 0)
        {
            Debug.Log("특성 진입 실패 (골드 부족 등)");

            
            return;
        }

        // 슬롯에 3종 그리기
        if (traitPanelScript != null)
            traitPanelScript.Show(choices);

        // 전환 (선택창 올리고 특성 패널 내림)
        StartCoroutine(SwitchTo(traitPanel));
    }

    public void OpenStat()
    {
        StartCoroutine(SwitchTo(statPanel));
    }

    public void OpenGamble()
    {
        StartCoroutine(SwitchTo(gamblePanel));
    }

    // 선택창을 올리고, 다 올라간 뒤 지정 패널을 내림
    private IEnumerator SwitchTo(SlideInPanel panel)
    {
        if (panel == null) yield break;

        if (selectMenu != null) selectMenu.SlideUp();

        yield return new WaitForSecondsRealtime(transitionGap);

        panel.gameObject.SetActive(true);
        panel.SlideDown();
        currentPanel = panel;
    }

    // 각 강화 패널의 "닫기" 버튼이 호출
    public void CloseCurrentPanel()
    {
        StartCoroutine(CloseRoutine());
    }

    private IEnumerator CloseRoutine()
    {
        if (currentPanel != null)
        {
            currentPanel.SlideUp();
            SlideInPanel closing = currentPanel;
            currentPanel = null;

            yield return new WaitForSecondsRealtime(transitionGap);

            closing.gameObject.SetActive(false);
        }

        if (selectMenu != null)
        {
            selectMenu.gameObject.SetActive(true);
            selectMenu.SlideDown();
        }
    }
     // 정비소 닫을 때(다음날로) 모든 하위 패널을 강제로 숨김 상태로 리셋
    public void ResetPanels()
    {
        StopAllCoroutines();   // 진행 중인 전환 코루틴 정리

        if (traitPanel != null)  traitPanel.gameObject.SetActive(false);
        if (statPanel != null)   statPanel.gameObject.SetActive(false);
        if (gamblePanel != null) gamblePanel.gameObject.SetActive(false);

        currentPanel = null;
    }
}