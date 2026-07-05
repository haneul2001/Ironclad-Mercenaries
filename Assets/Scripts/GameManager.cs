using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 싱글톤 어디든지 참조 가능 ( 전역 )

    [Header("스테이지 설정")]
    public int targetWave = 3; // 목표 웨이브 수 (3웨이브 = 1일)

    [Header("Day")]
    public int currentDay = 1;          // 현재 며칠째
    public EnemySpawner spawner;        // 인스펙터에서 드래그
    public TMP_Text dayText;            // "DAY N" 표시

    [Header("UI 패널")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("정비 UI")]
    public GameObject upgradePanel;
    public UpgradeMenuController upgradeMenu;   // 정비소 패널 관리

    [Header("클릭 안내 텍스트")]
    public GameObject clickToContinueText; // "아무 곳이나 클릭하시오"

    // 게임 상태
    public enum GameState { Playing, Victory, Defeat }
    public GameState currentState = GameState.Playing;

    private bool waitingForClick = false; // 클릭 대기 중인지

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateDayText();

        // 1일차 시작 (스포너는 이제 스스로 시작 안 하고 여기서 시작시킴)
        if (spawner != null)
            spawner.StartDay(currentDay);
    }

    void Update()
    {
        // 클릭 대기 중이고, 마우스 클릭하면
        if (waitingForClick && Input.GetMouseButtonDown(0))
        {
            waitingForClick = false;
            OnVictoryClicked();
        }

        // ── 디버깅용 배속 ──
        if (Input.GetKeyDown(KeyCode.Alpha1)) Time.timeScale = 1f;   // 1번키: 정상
        if (Input.GetKeyDown(KeyCode.Alpha2)) Time.timeScale = 2f;   // 2번키: 2배속
        if (Input.GetKeyDown(KeyCode.Alpha3)) Time.timeScale = 4f;   // 3번키: 4배속
    }

    void UpdateDayText()
    {
        if (dayText != null)
            dayText.text = "DAY " + currentDay;
    }

    public void OnCoreDestroyed()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Defeat;
        Debug.Log("게임 오버! 성벽이 파괴되었습니다.");
        if (defeatPanel != null) defeatPanel.SetActive(true);
        EndGame();
    }

    public void OnAllWavesCleared()
    {
        if (currentState != GameState.Playing) return;
        currentState = GameState.Victory;
        Debug.Log("승리! 모든 웨이브를 클리어했습니다.");
        StartCoroutine(VictorySequence());
    }

    IEnumerator VictorySequence()
    {
        // 마지막 몬스터 죽고 3초 대기
        yield return new WaitForSeconds(3f);

        // 빅토리 이미지 내려옴
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            SlideInPanel slide = victoryPanel.GetComponentInChildren<SlideInPanel>();
            if (slide != null) slide.SlideDown();
        }

        // 내려온 후 2초 대기
        yield return new WaitForSeconds(2f);

        // "아무 곳이나 클릭하시오" 텍스트 표시
        if (clickToContinueText != null)
            clickToContinueText.SetActive(true);

        // 클릭 대기 시작
        waitingForClick = true;
    }

    void OnVictoryClicked()
    {
        // 클릭 텍스트 즉시 끄기
        if (clickToContinueText != null)
            clickToContinueText.SetActive(false);

        StartCoroutine(ToUpgradeSequence());
    }

    IEnumerator ToUpgradeSequence()
    {
        // 1) 빅토리 이미지 올라감 (사라짐)
        if (victoryPanel != null)
        {
            SlideInPanel slide = victoryPanel.GetComponentInChildren<SlideInPanel>();
            if (slide != null) slide.SlideUp();
        }

        // 2) 올라가는 시간 대기 (SlideInPanel duration이랑 맞추기)
        yield return new WaitForSecondsRealtime(0.5f);

        // 3) 빅토리 패널 끄기
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // 4) 정비 UI 켜고, 선택 메뉴만 내려오기 (강화 패널들은 숨은 채로)
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(true);

            if (upgradeMenu != null && upgradeMenu.selectMenu != null)
                upgradeMenu.selectMenu.SlideDown();
        }

        // 정비소 진입 → 각 유닛에 예산 지급
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.GrantBudgets();

        Debug.Log("정비 UI 내려옴");
    }

    // "다음 날로" 버튼이 호출
    public void OnNextDayClicked()
    {
        // 1) AI 자동 강화 (주인공 제외 나머지)
        if (UpgradeManager.Instance != null)
            UpgradeManager.Instance.RunAIUpgrades();

        // 2) 정비소 패널들 리셋 (열려있던 강화창 정리)
        if (upgradeMenu != null)
            upgradeMenu.ResetPanels();

        // 3) 정비 UI 닫기
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        if (clickToContinueText != null)
            clickToContinueText.SetActive(false);

        // 4) 다음 날로
        currentDay++;
        UpdateDayText();
        currentState = GameState.Playing;
        Time.timeScale = 1f;

        // 5) 스포너에 새 날 시작
        if (spawner != null)
            spawner.StartDay(currentDay);

        Debug.Log($"=== Day {currentDay} 시작 ===");
    }

    void EndGame()
    {
        Time.timeScale = 0f;
    }
}